using EFT;
using EFT.Hideout;
using HarmonyLib;
using SkillDistribution.Features;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SkillDistribution.Patches
{
    class WorkoutSingleQteFinishedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.OnSingleQteFinished));
        }

        [PatchPrefix]
        [HarmonyPriority(Priority.First)]
        private static void Prefix(WorkoutBehaviour __instance, out Dictionary<Skill, float>? __state)
        {
            __state = WorkoutSession.Current?.Workout == __instance
                ? __instance._playerOwner.HideoutPlayer.Skills.Skills.ToDictionary(skill => skill, skill => skill.Current)
                : null;
        }

        [PatchPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(Dictionary<Skill, float>? __state)
        {
            if (__state != null)
            {
                WorkoutSession.Current?.RecordChanges(__state);
            }
        }
    }
}
