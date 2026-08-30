using EFT;
using HarmonyLib;
using SkillDistribution.Config;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SkillDistribution.Patches
{
    internal class ProfileSelectionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EftClientBackendSession.CG_SetMainProfile), nameof(EftClientBackendSession.CG_SetMainProfile.method_0));
        }

        [PatchPostfix]
        private static void Postfix()
        {
            Plugin.LogDebug("Profile selected");
            Settings.BuildMultipliers();
        }
    }
}
