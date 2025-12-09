using HarmonyLib;
using SkillDistribution.Helpers;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SkillDistribution.Patches
{
    internal class ProfileSelectionPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Class308.Class1596), nameof(Class308.Class1596.method_0));
        }

        [PatchPostfix]
        static void Postfix()
        {
            Plugin.LogDebug("Profile selected");
            Settings.BuildMultipliers();
        }
    }
}
