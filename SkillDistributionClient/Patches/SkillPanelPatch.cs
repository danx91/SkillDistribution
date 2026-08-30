using EFT;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using ZGFueDkx.ZGCLib.helpers;

namespace SkillDistribution.Patches
{
    class SkillPanelPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillPanel), nameof(SkillPanel.OnSkillLevelChanged));
        }

        [PatchPostfix]
        private static void Postfix(SkillPanel __instance, Skill ____skill)
        {
            __instance._effectivenessDown.SetActive(____skill.Effectiveness < 1f && RaidUtils.IsInRaid());
            __instance._effectivenessUp.SetActive(____skill.Effectiveness > 1f && RaidUtils.IsInRaid());
        }
    }
}
