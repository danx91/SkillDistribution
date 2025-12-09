using EFT.UI;
using HarmonyLib;
using SkillDistribution.Helpers;
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
            return AccessTools.Method(typeof(SkillPanel), nameof(SkillPanel.method_1));
        }

        [PatchPostfix]
        static void Postfix(SkillClass ___skillClass, GameObject ____effectivenessDown, GameObject ____effectivenessUp)
        {
            ____effectivenessDown.SetActive(___skillClass.Effectiveness < 1f && RaidUtils.IsInRaid());
            ____effectivenessUp.SetActive(___skillClass.Effectiveness > 1f && RaidUtils.IsInRaid());
        }
    }
}
