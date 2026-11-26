using EFT.Hideout;
using HarmonyLib;
using SkillDistribution.Features;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SkillDistribution.Patches
{
    class WorkoutClearPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.Clear));
        }

        [PatchPostfix]
        private static void Postfix(WorkoutBehaviour __instance)
        {
            if (WorkoutSession.Current?.Workout == __instance)
            {
                WorkoutSession.Current = null;
            }
        }
    }
}
