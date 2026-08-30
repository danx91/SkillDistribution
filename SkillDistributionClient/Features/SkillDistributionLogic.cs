using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using SkillDistribution.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static SkillDistribution.Features.DistributionModes;

namespace SkillDistribution.Features
{
    internal static class SkillDistributionLogic
    {
        public enum EDistributionMode
        {
            [Description("Equal")]
            Equal,
            [Description("Round-robin")]
            RoundRobin,
            [Description("Random")]
            Random,
            [Description("Weighted random (Lowest skill(s))")]
            WeightedRandomMin,
            [Description("Weighted random (Highest skill(s))")]
            WeightedRandomMax,
            [Description("Lowest skill(s)")]
            Min,
            [Description("Highest skill(s)")]
            Max,
        }

        public enum ECompareMode
        {
            Min = 0,
            Max = 1,
        }

        public static readonly float MIN_XP = 1E-4f;
        public static readonly float ELITE_LEVEL = 5100f;

        private static bool _inDistribute = false;

        public static void DistributeSkillExperience(SkillManager manager, float xp, ESkillId source)
        {
            if (_inDistribute)
            {
                Plugin.LogDebug("Already in distribute. Abort...");
                return;
            }

            _inDistribute = true;

            try
            {
                float origXp = xp;
                xp *= Settings.ExperienceMultiplier!.Value;

                float postGlobal = xp;
                if (Settings.ApplyMultipliers!.Value)
                {
                    if (Settings.SkillMults.TryGetValue(source, out ConfigEntry<float> mult))
                    {
                        xp *= mult.Value;
                    }
                    else
                    {
                        Plugin.LogDebug($"Failed to get multiplier for skill ${source}");
                    }
                }

                Plugin.LogDebug($"Distribute XP - final: {xp}, post global: {postGlobal}, orig: {origXp}");

                if (xp <= MIN_XP)
                {
                    Plugin.LogDebug($"XP too low. Abort...");
                    return;
                }

                List<Skill>? selectedSkills = SelectSkills(manager, ref xp);
                if (selectedSkills is null || selectedSkills.Count == 0)
                {
                    Plugin.LogDebug("Failed to distribute skill XP - no skills selected! Abort...");
                    return;
                }

                foreach (Skill skill in selectedSkills)
                {
                    ApplySkillExperience(skill, xp);
                }
            }
            catch (Exception e)
            {
                Plugin.LogSource?.LogError($"Error while distributing XP");
                Plugin.LogSource?.LogError(e.ToString());
            }
            finally
            {
                _inDistribute = false;
            }
        }

        public static List<Skill>? SelectSkills(SkillManager manager, ref float xp)
        {
            if (!GetApplicableSkills(manager, out List<Skill> skills))
            {
                Plugin.LogDebug("Failed to distribute skill XP - no non-elite skills found! Abort...");
                return null;
            }

            int skillsCount = Settings.SkillsCount!.Value;

            if (skillsCount < 1)
            {
                skillsCount = 1;
            }

            switch (Settings.DistributionMode!.Value)
            {
                case EDistributionMode.Equal:
                    return EqualDistribution(skills, ref xp);
                case EDistributionMode.RoundRobin:
                    return RoundRobinDistribution(skills, ref xp);
                case EDistributionMode.Random:
                    return RandomDistribution(skills, ref xp, skillsCount);
                case EDistributionMode.WeightedRandomMin:
                    return WeightedRandomDistribution(skills, ref xp, skillsCount, ECompareMode.Min);
                case EDistributionMode.WeightedRandomMax:
                    return WeightedRandomDistribution(skills, ref xp, skillsCount, ECompareMode.Max);
                case EDistributionMode.Min:
                    return EdgeDistribution(skills, ref xp, skillsCount, ECompareMode.Min);
                case EDistributionMode.Max:
                    return EdgeDistribution(skills, ref xp, skillsCount, ECompareMode.Max);
                default:
                    Plugin.LogSource?.LogError("Unknown distribution mode. Abort...");
                    return null;
            }
        }

        private static bool GetApplicableSkills(SkillManager manager, out List<Skill> skills)
        {
            skills = [];

            foreach (Skill skill in manager.DisplayList)
            {
                if (!skill.Locked && skill.Current < ELITE_LEVEL)
                {
                    skills.Add(skill);
                }
            }

            return skills.Count > 0;
        }

        public static void ApplySkillExperience(Skill skill, float xp)
        {
            float origXp = xp;
            float xpPre = skill.Current;

            skill.SkillManager.SkillProgress.Complete(skill, xp);

            if (Settings.UseEffectiveness!.Value)
            {
                xp = skill.UseEffectiveness(xp);
            }
            else if (Settings.CauseFatigue!.Value)
            {
                if (Time.time > skill._fatigueTimer)
                {
                    skill._effectiveness = 1f;
                    skill._pointsEarned = Mathf.Min(skill.PointsEarned, Singleton<GlobalConfiguration>.Instance.SkillFreshPoints);
                }

                skill._pointsEarned += xp;
                skill._effectiveness = skill.SkillManager.GetEffectiveness((int)skill.PointsEarned);

                if (skill.Effectiveness <= 1.0f)
                {
                    skill._fatigueTimer = Time.time + Singleton<GlobalConfiguration>.Instance.SkillFatigueReset;
                }
            }

            float postFatigue = xp;

            if (Settings.UseBonuses!.Value)
            {
                xp = (float)skill.SkillManager.BonusController.Calculate(skill, (double)xp);
            }

            float postBonus = xp;

            if (skill.Level < 9)
            {
                xp = skill.CalculateExpOnFirstLevels(xp);
            }

            skill.SetCurrent(skill.Current + xp, true);
            skill.LastCall = DateTimeExtensions.UtcNow;

            Plugin.LogDebug($"\tskill: {skill.Id}, xp: {origXp}, effectiveness: {postFatigue}, bonus: {postBonus}, lvlCorr: {xp}; pre: {xpPre}, final: {skill.Current}");
        }
    }
}
