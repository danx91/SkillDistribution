using HarmonyLib;
using SkillDistribution.Helpers;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SkillDistribution.Patches
{
    class AbstractSkillPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AbstractSkillClass), nameof(AbstractSkillClass.OnTrigger));
        }

        [PatchPrefix]
        static bool Prefix(AbstractSkillClass __instance, ref float __state)
        {
            __state = __instance.Current;
            return true;
        }

        [PatchPostfix]
        static void Postfix(AbstractSkillClass __instance, float val, float __state)
        {
            if (!(__instance is SkillClass skill) || Plugin.SkillManager == null || Plugin.SkillManager != skill.SkillManager)
            {
                return;
            }

            float xpLeft = val - (skill.Current - __state);

            Plugin.LogDebug($"Skill {skill.Id} xp change by {val}. Prev: {__state}, Cur: {skill.Current}, xp left: {xpLeft}");

            if(xpLeft < 1.5E-4f)
            {
                Plugin.LogDebug("\tToo close to 0 - return");
                return;
            }

            if(!skill.IsEliteLevel)
            {
                Plugin.LogDebug("\tNot elite skill - return");
                return;
            }

            SkillHelper.DistributeSkillExperience(skill.SkillManager, xpLeft);
        }
    }
}
