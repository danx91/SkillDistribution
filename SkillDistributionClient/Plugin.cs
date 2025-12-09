using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT;
using SkillDistribution.Helpers;
using SkillDistribution.Patches;
using System.IO;

namespace SkillDistribution
{
    [
        BepInPlugin("com.zgfuedkx.skilldistribution", "ZGFueDkx-SkillDistribution", "1.2.2"),
        BepInDependency("com.SPT.core", "4.0.0"),
    ]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource? LogSource;
        public static SkillManager? SkillManager;

        public void Awake()
        {
            LogSource = Logger;

            Settings.Init(Config);

            new ProfileSelectionPatch().Enable();
            SkillProgressUnsubscribePatch.EnableAll();
            new AbstractSkillPatch().Enable();
            new OnGameStartedPatch().Enable();
            new SkillPanelPatch().Enable();
            new SkillTooltipPatch().Enable();
            new WorkoutBehaviourPatch().Enable();

            LogSource.LogInfo($"SkillDistribution by ZGFueDkx version {Info.Metadata.Version} started");
        }

        public static void LogDebug(string msg)
        {
            if (Settings.ShowDebug!.Value)
            {
                LogSource?.LogDebug(msg);
            }
        }
    }
}
