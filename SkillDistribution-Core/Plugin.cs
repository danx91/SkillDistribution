using BepInEx;
using BepInEx.Logging;
using EFT;
using SkillDistribution.Helpers;
using SkillDistribution.Patches;

namespace SkillDistribution
{
    [
        BepInPlugin("ZGFueDkx.SkillDistribution", "SkillDistribution", "1.0.0"),
        BepInDependency("com.SPT.core", "3.11"),
    ]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static SkillManager SkillManager;

        public void Awake()
        {
            LogSource = Logger;

            Settings.Init(Config);

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
            if (Settings.ShowDebug.Value)
            {
                LogSource.LogDebug(msg);
            }
        }
    }
}
