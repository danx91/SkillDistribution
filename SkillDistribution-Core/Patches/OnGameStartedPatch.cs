using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace SkillDistribution.Patches
{
    class OnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        static void Postfix(GameWorld __instance)
        {
            Plugin.SkillManager = __instance.MainPlayer.Skills;
        }
    }
}
