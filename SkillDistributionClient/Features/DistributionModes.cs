using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using static SkillDistribution.Features.SkillDistributionLogic;

namespace SkillDistribution.Features
{
    internal static class DistributionModes
    {
        private static int _globalRoundRobinIndex = -1;
        private static readonly Dictionary<ESkillClass, int> _classRoundRobinIndices = [];

        private static bool TryAdjustCount(float xp, int available, ref int count, out float xpPerSkill)
        {
            count = Math.Clamp(count, 1, available);
            if ((xpPerSkill = xp / count) >= MIN_XP)
            {
                return true;
            }

            if ((count = (int)(xp / MIN_XP)) <= 0)
            {
                Plugin.LogDebug("Failed to adjust count - not enough XP. Abort...");
                return false;
            }

            xpPerSkill = xp / count;

            return true;
        }

        public static List<Skill>? EqualDistribution(List<Skill> skills, ref float xp)
        {
            float xpPerSkill = xp / skills.Count;
            Plugin.LogDebug($"Equal ditribution. To ditribute: {xp}, per skill: {xpPerSkill}, total skills {skills.Count}");

            if (xpPerSkill < MIN_XP)
            {
                Plugin.LogDebug("XP per skill is too low - switch to random");
                return RandomDistribution(skills, ref xp, skills.Count);
            }

            xp = xpPerSkill;
            return skills;
        }

        public static List<Skill> RoundRobinDistribution(List<Skill> skills, float xp, ESkillClass? skillClass)
        {
            int index = skillClass.HasValue ? _classRoundRobinIndices.GetValueOrDefault(skillClass.Value, -1) : _globalRoundRobinIndex;
            if (index == -1)
            {
                index = UnityEngine.Random.Range(0, skills.Count);
            }
            else if (index >= skills.Count)
            {
                index = 0;
            }

            if (skillClass.HasValue)
            {
                _classRoundRobinIndices[skillClass.Value] = index + 1;
            }
            else
            {
                _globalRoundRobinIndex = index + 1;
            }

            Plugin.LogDebug($"Round robin distribution. To distribute: {xp}, total skills {skills.Count}, index: {index}");
            return [skills[index]];
        }

        public static List<Skill>? RandomDistribution(List<Skill> skills, ref float xp, int count)
        {
            int dbgCount = count;
            float dbgXp = xp;

            if (!TryAdjustCount(xp, skills.Count, ref count, out xp))
            {
                return null;
            }

            Plugin.LogDebug($"Random ditribution. To ditribute: {dbgXp}, per skill: {xp}, count {count} ({dbgCount}), total skills {skills.Count}");

            List<int> indexes = [.. Enumerable.Range(0, skills.Count)];
            List<Skill> selectedSkills = new(count);

            for (int i = 0; i < count; i++)
            {
                int rng = UnityEngine.Random.Range(0, indexes.Count);
                int index = indexes[rng];
                indexes.RemoveAt(rng);

                selectedSkills.Add(skills[index]);
            }

            return selectedSkills;
        }

        public static List<Skill>? WeightedRandomDistribution(List<Skill> skills, ref float xp, int count, ECompareMode mode)
        {
            int dbgCount = count;
            float dbgXp = xp;

            if (!TryAdjustCount(xp, skills.Count, ref count, out xp))
            {
                return null;
            }

            List<int> indexes = [.. Enumerable.Range(0, skills.Count)];
            List<float> weights = [];

            foreach (Skill skill in skills)
            {
                weights.Add(Math.Max(mode == ECompareMode.Min ? ELITE_LEVEL - skill.Current : skill.Current, 100));
            }

            float sum = weights.Sum();
            List<Skill> selectedSkills = new(count);
            Plugin.LogDebug($"Weighted random ditribution. To ditribute: {dbgXp}, per skill: {xp}, count {count} ({dbgCount}), total skills {skills.Count}, weights sum: {sum}");

            for (int i = 0; i < count; i++)
            {
                float dice = UnityEngine.Random.Range(0f, sum);
                Plugin.LogDebug($"{i} : {dice}");

                for (int j = 0; j < weights.Count; j++)
                {
                    dice -= weights[j];
                    if (dice <= 0 || j == weights.Count - 1)
                    {
                        selectedSkills.Add(skills[indexes[j]]);

                        sum -= weights[j];
                        weights.RemoveAt(j);
                        indexes.RemoveAt(j);

                        break;
                    }
                }
            }

            return selectedSkills;
        }

        public static List<Skill>? EdgeDistribution(List<Skill> skills, ref float xp, int count, ECompareMode mode)
        {
            int dbgCount = count;
            float dbgXp = xp;

            if (!TryAdjustCount(xp, skills.Count, ref count, out xp))
            {
                return null;
            }

            Plugin.LogDebug($"Min/Max ditribution. To ditribute: {dbgXp}, per skill: {xp}, count {count} ({dbgCount}), total skills {skills.Count}");

            List<Skill> sortedSkills = [.. skills.OrderBy(s => mode == ECompareMode.Min ? s.Current : -s.Current)];
            List<Skill> selectedSkills = new(count);

            for (int i = 0; i < count; i++)
            {
                selectedSkills.Add(sortedSkills[i]);
            }

            return selectedSkills;
        }
    }
}
