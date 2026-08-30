using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

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
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Skill), nameof(Skill.UpdateRules));
        }

        [PatchPrefix]
        private static bool Prefix()
        {
            AbstractSkillUnsubscribePatch._fromUpdateRules = true;
            return true;
        }

        [PatchPostfix]
        private static void Postfix()
        {
            AbstractSkillUnsubscribePatch._fromUpdateRules = false;
        }
    }

    class AbstractSkillUnsubscribePatch : ModulePatch
    {
        public static bool _fromUpdateRules = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BaseSkill), nameof(BaseSkill.Unsubscribe));
        }

        [PatchPrefix]
        private static bool Prefix(BaseSkill __instance)
        {
            if (_fromUpdateRules)
            {
                Plugin.LogDebug($"Preventing events unsubscribe on {__instance.Id}");
            }

            return !_fromUpdateRules;
        }
    }
}
