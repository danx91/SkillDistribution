using System.Reflection;

using HarmonyLib;
using SPT.Reflection.Patching;

namespace SkillDistribution.Patches
{
    public static class SkillProgressUnsubscribePatch
    {
        public static void EnableAll()
        {
            new SkillUnsubscribePatch().Enable();
            new AbstractSkillUnsubscribePatch().Enable();
        }
    }

    class SkillUnsubscribePatch : ModulePatch
    {
        private static bool _fromMethod3 = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillClass), nameof(SkillClass.method_3));
        }

        [PatchPrefix]
        static bool Prefix()
        {
            _fromMethod3 = true;
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            _fromMethod3 = false;
        }
    }

    class AbstractSkillUnsubscribePatch : ModulePatch
    {
        public static bool fromMethod3 = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AbstractSkillClass), nameof(AbstractSkillClass.Unsubscribe));
        }

        [PatchPrefix]
        static bool Prefix(AbstractSkillClass __instance)
        {
            Plugin.LogDebug($"Preventing events unsubscribe on {__instance.Id}");
            return fromMethod3;
        }
    }
}
