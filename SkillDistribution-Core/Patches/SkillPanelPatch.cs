using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

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
            ____effectivenessDown.SetActive(___skillClass.Effectiveness < 1f && GClass2064.InRaid);
            ____effectivenessUp.SetActive(___skillClass.Effectiveness > 1f && GClass2064.InRaid);
        }
    }
}
