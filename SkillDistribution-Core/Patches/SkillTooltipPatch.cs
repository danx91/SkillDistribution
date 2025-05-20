using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SkillDistribution.Patches
{
    class SkillTooltipPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillTooltip), nameof(SkillTooltip.Show), new[] { typeof(SkillClass) });
        }

        [PatchTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_0 &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 1].operand is FieldInfo fieldInfo &&
                    fieldInfo.Name == "skill" &&
                    codes[i + 2].opcode == OpCodes.Callvirt &&
                    codes[i + 2].operand is MethodInfo methodInfo &&
                    methodInfo.Name == "get_IsEliteLevel")
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldc_I4_0)
                        .WithLabels(codes[i].labels);
                    codes.RemoveAt(i + 1);
                    codes.RemoveAt(i + 1);

                    break;
                }
            }

            return codes;
        }
    }
}
