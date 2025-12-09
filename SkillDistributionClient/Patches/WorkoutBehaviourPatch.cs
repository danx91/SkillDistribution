using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Hideout;
using HarmonyLib;
using SkillDistribution.Helpers;
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
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.method_18));
        }

        [PatchPrefix]
        static bool Prefix(WorkoutBehaviour __instance)
        {
            if (!Settings.AllowGym!.Value)
            {
                return true;
            }

            SkillManager manager = __instance.HideoutPlayerOwner_0.HideoutPlayer.Skills;
            SkillClass[]? skills = GetWorkoutSkills(__instance, __instance.QteHandleData_0, manager,
                out float xpMult, out QteEffect.SkillExperienceMultiplierData[]? multipliers);

            if(skills is null)
            {
                Plugin.LogDebug("Workout skills are null. Abort...");
                return true;
            }

            float effectiveness = (__instance.HealthControllerClass.HasSevereMusclePainEffect() ?
                __instance.HealthControllerClass.GetSevereMusclePainSettings().GymEffectivity : (__instance.HealthControllerClass.HasMildMusclePainEffect() ?
                __instance.HealthControllerClass.GetMildMusclePainSettings().GymEffectivity : 0f));

            Plugin.LogDebug($"Workout skills: {skills.Length}, xpMult: {xpMult}, effectiveness: {effectiveness}");

            xpMult *= Settings.GymExperienceMultiplier!.Value;

            foreach (SkillClass skill in skills)
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
                    NotificationManagerClass.DisplayNotification(new GClass2551(
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
                NotificationManagerClass.DisplayNotification(new GClass2551(
                    $"Total of {skills.Length} skills increased during workout",
                    ENotificationDurationType.Default,
                    ENotificationIconType.Default,
                    null));
            }

            return false;
        }

        private static SkillClass[]? GetWorkoutSkills(WorkoutBehaviour workoutBehaviour, QteHandleData qteHandleData, SkillManager manager,
            out float xpMult, out QteEffect.SkillExperienceMultiplierData[]? multipliers)
        {
            QteEffect[] qteAllEffects = qteHandleData.Results[QteData.EQteEffectType.SingleSuccessEffect].Effects;
            QteEffect[] qteEffects = [.. qteAllEffects.Where(new Func<QteEffect, bool>(workoutBehaviour.method_21))];

            if (qteEffects.Length > 0)
            {
                QteEffect qteEffect = qteEffects[qteEffects.Length == 1 ? 0 : Singleton<HideoutClass>.Instance.QteRandomNext(0, qteEffects.Length)];

                xpMult = 1.0f;
                multipliers = qteEffect.SkillExpMultiplierData;
                return
                [
                    manager.GetSkill(qteEffect.Skill)
                ];
            }

            xpMult = 1.0f;
            List<SkillClass>? skills = SkillHelper.SelectSkills(manager, ref xpMult);
            if (skills is null || qteAllEffects.Length == 0)
            {
                Plugin.LogDebug("Gym distribution failed!");

                xpMult = 1.0f;
                multipliers = null;
                return null;
            }

            multipliers = qteAllEffects[Singleton<HideoutClass>.Instance.QteRandomNext(0, qteAllEffects.Length)].SkillExpMultiplierData;
            return [.. skills];
        }
    }
}
