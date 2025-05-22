using Comfort.Common;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static SkillDistribution.Helpers.SkillDistributions;

namespace SkillDistribution.Helpers
{
    internal static class SkillHelper
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
        private static readonly AccessTools.FieldRef<SkillClass, float> _pointsEarnedRef = AccessTools.FieldRefAccess<SkillClass, float>("float_2");
        private static readonly AccessTools.FieldRef<SkillClass, float> _effectivenessRef = AccessTools.FieldRefAccess<SkillClass, float>("float_3");
        private static readonly AccessTools.FieldRef<SkillClass, float> _fatigueResetRef = AccessTools.FieldRefAccess<SkillClass, float>("float_4");

        public static void DistributeSkillExperience(SkillManager manager, float xp)
        {
            if(_inDistribute)
            {
                Plugin.LogDebug("Already in distribute. Abort...");
                return;
            }

            _inDistribute = true;

            try
            {
                float origXp = xp;

                xp *= Settings.ExperienceMultiplier.Value;

                Plugin.LogDebug($"Distribute XP - xp: {xp}, orig: {origXp}");

                List<SkillClass> selectedSkills = SelectSkills(manager, ref xp);
                if (selectedSkills == null || selectedSkills.Count == 0)
                {
                    Plugin.LogDebug("Failed to distribute skill XP - no skills selected! Abort...");
                    return;
                }

                foreach (SkillClass skill in selectedSkills)
                {
                    ApplySkillExperience(skill, xp);
                }
            }
            catch (Exception e)
            {
                Plugin.LogSource.LogError($"Error while distributing XP");
                Plugin.LogSource.LogError(e.ToString());
            }
            finally
            {
                _inDistribute = false;
            }
        }

        public static List<SkillClass> SelectSkills(SkillManager manager, ref float xp)
        {
            if (!GetApplicableSkills(manager, out List<SkillClass> skills))
            {
                Plugin.LogDebug("Failed to distribute skill XP - no non-elite skills found! Abort...");
                return null;
            }

            int skillsCount = Settings.SkillsCount.Value;

            if (skillsCount < 1)
            {
                skillsCount = 1;
            }

            switch (Settings.DistributionMode.Value)
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
                    Plugin.LogSource.LogError("Unknown distribution mode. Abort...");
                    return null;
            }
        }

        private static bool GetApplicableSkills(SkillManager manager, out List<SkillClass> skills)
        {
            skills = new List<SkillClass>();

            foreach (SkillClass skill in manager.DisplayList)
            {
                if(!skill.Locked && skill.Current < ELITE_LEVEL)
                {
                    skills.Add(skill);
                }
            }

            return skills.Count > 0;
        }

        public static void ApplySkillExperience(SkillClass skill, float xp)
        {
            float origXp = xp;
            float xpPre = skill.Current;

            skill.SkillManager.SkillProgress.Complete(skill, xp);

            if(Settings.UseEffectiveness.Value)
            {
                xp = skill.UseEffectiveness(xp);
            } else if(Settings.CauseFatigue.Value)
            {
                if (Time.time > _fatigueResetRef(skill))
                {
                    _effectivenessRef(skill) = 1f;
                    _pointsEarnedRef(skill) = Mathf.Min(skill.PointsEarned, (float) Singleton<BackendConfigSettingsClass>.Instance.SkillFreshPoints);
                }

                _pointsEarnedRef(skill) += xp;
                _effectivenessRef(skill) = skill.SkillManager.GetEffectiveness((int) (skill.PointsEarned));

                if(skill.Effectiveness <= 1.0f)
                {
                    _fatigueResetRef(skill) = Time.time + (float) Singleton<BackendConfigSettingsClass>.Instance.SkillFatigueReset;
                }
            }

            float postFatigue = xp;

            if (Settings.UseBonuses.Value)
            {
                xp = (float) skill.SkillManager.BonusController.Calculate(skill, (double) xp);
            }

            float postBonus = xp;

            if(skill.Level < 9)
            {
                xp = skill.CalculateExpOnFirstLevels(xp);
            }

            skill.SetCurrent(skill.Current + xp, true);
            skill.LastCall = EFTDateTimeClass.UtcNow;

            Plugin.LogDebug($"\tskill: {skill.Id}, xp: {origXp}, effectiveness: {postFatigue}, bonus: {postBonus}, lvlCorr: {xp}; pre: {xpPre}, final: {skill.Current}");
        }
    }
}
