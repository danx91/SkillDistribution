using EFT;
using EFT.Hideout;
using HarmonyLib;
using SkillDistribution.Config;
using SkillDistribution.Features;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SkillDistribution.Patches
{
    class WorkoutPreparePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.Prepare));
        }

        [PatchPrefix]
        private static void Prefix(WorkoutBehaviour __instance)
        {
            WorkoutSession.Current = Settings.AllowGym!.Value && __instance._hideout._session is EftClientBackendSession
                ? new WorkoutSession(__instance) : null;
            Plugin.LogDebug(WorkoutSession.Current == null ? "Preparing workout (vanilla)" : "Preparing workout (modded)");
        }
    }
}
