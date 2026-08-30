using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Hideout;
using HarmonyLib;
using SkillDistribution.Config;
using SkillDistribution.Features;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UI.Hideout;

namespace SkillDistribution.Patches
{
    class WorkoutBehaviourPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.CalculateExperience));
        }

        [PatchPrefix]
        private static bool Prefix(WorkoutBehaviour __instance)
        {
            if (!Settings.AllowGym!.Value)
            {
                return true;
            }

            SkillManager manager = __instance._playerOwner.HideoutPlayer.Skills;
            Skill[]? skills = GetWorkoutSkills(__instance, __instance._qteData, manager,
                out float xpMult, out QteEffect.SkillExperienceMultiplierData[]? multipliers);

            if (skills is null)
            {
                Plugin.LogDebug("Workout skills are null. Abort...");
                return true;
            }

            float effectiveness = (__instance._offlineHealthController.HasSevereMusclePainEffect() ?
                __instance._offlineHealthController.GetSevereMusclePainSettings().GymEffectivity : (__instance._offlineHealthController.HasMildMusclePainEffect() ?
                __instance._offlineHealthController.GetMildMusclePainSettings().GymEffectivity : 0f));

            Plugin.LogDebug($"Workout skills: {skills.Length}, xpMult: {xpMult}, effectiveness: {effectiveness}");

            xpMult *= Settings.GymExperienceMultiplier!.Value;

            foreach (Skill skill in skills)
            {
                float skillMultiplier = 0f;
                foreach (QteEffect.SkillExperienceMultiplierData multiplierData in multipliers!)
                {
                    if (skill.Level >= multiplierData.level)
                    {
                        skillMultiplier = multiplierData.value;
                    }
                }

                float factor = manager.SkillProgress.Factor(skillMultiplier - skillMultiplier * effectiveness, true).FactorValue * xpMult;
                float xp = (skill.Level > 9 ? factor : skill.CalculateExpOnFirstLevels(factor));

                skill.SetCurrent(skill.Current + xp, true);
                skill.AddPointsEarnedForWorkout(xp);
                Plugin.LogDebug($"\tGym - skill: {skill.Id}, xp: {xp}, skillMult: {skillMultiplier}, factor: {factor}, userMult: {Settings.GymExperienceMultiplier.Value}");

                if (skills.Length <= 3)
                {
                    NotificationManager.DisplayNotification(new CustomNotification(
                        string.Format(
                            "Skill '{0}' increased by {1}".Localized(null),
                            skill.Id.ToString().Localized(null),
                            Math.Round((double)factor, 2)
                        ),
                        ENotificationDurationType.Default,
                        ENotificationIconType.Default,
                        null));
                }
            }

            if (skills.Length > 3)
            {
                NotificationManager.DisplayNotification(new CustomNotification(
                    $"Total of {skills.Length} skills increased during workout",
                    ENotificationDurationType.Default,
                    ENotificationIconType.Default,
                    null));
            }

            return false;
        }

        private static Skill[]? GetWorkoutSkills(WorkoutBehaviour workoutBehaviour, QteHandleData qteHandleData, SkillManager manager,
            out float xpMult, out QteEffect.SkillExperienceMultiplierData[]? multipliers)
        {
            QteEffect[] qteAllEffects = qteHandleData.Results[QteData.EQteEffectType.SingleSuccessEffect].Effects;
            QteEffect[] qteEffects = [.. qteAllEffects.Where(new Func<QteEffect, bool>(workoutBehaviour.CG_CalculateExperience))];

            if (qteEffects.Length > 0)
            {
                QteEffect qteEffect = qteEffects[qteEffects.Length == 1 ? 0 : Singleton<HideoutRepresentation>.Instance.QteRandomNext(0, qteEffects.Length)];

                xpMult = 1.0f;
                multipliers = qteEffect.SkillExpMultiplierData;
                return
                [
                    manager.GetSkill(qteEffect.Skill)
                ];
            }

            xpMult = 1.0f;
            List<Skill>? skills = SkillDistributionLogic.SelectSkills(manager, ref xpMult);
            if (skills is null || qteAllEffects.Length == 0)
            {
                Plugin.LogDebug("Gym distribution failed!");

                xpMult = 1.0f;
                multipliers = null;
                return null;
            }

            multipliers = qteAllEffects[Singleton<HideoutRepresentation>.Instance.QteRandomNext(0, qteAllEffects.Length)].SkillExpMultiplierData;
            return [.. skills];
        }
    }
}
