using BepInEx.Configuration;
using EFT;
using SkillDistribution.Features;
using SkillDistribution.Helpers;
using SPT.Reflection.Utils;
using System.Collections.Generic;
using ZGFueDkx.ZGCLib.Config;

namespace SkillDistribution.Config
{
    internal static class Settings
    {
        public static ConfigEntry<SkillDistributionLogic.EDistributionMode>? DistributionMode;
        public static ConfigEntry<int>? SkillsCount;
        public static ConfigEntry<bool>? AllowGym;
        public static ConfigEntry<bool>? UseBonuses;
        public static ConfigEntry<bool>? UseEffectiveness;
        public static ConfigEntry<bool>? CauseFatigue;
        public static ConfigEntry<float>? ExperienceMultiplier;
        public static ConfigEntry<float>? GymExperienceMultiplier;
        public static ConfigEntry<bool>? ShowDebug;

        public static List<ConfigEntryBase> ConfigEntries = [];

        public static ConfigEntry<bool>? ApplyMultipliers;
        public static Dictionary<ESkillId, ConfigEntry<float>> SkillMults = [];

        private static ConfigCategory? _mults;
        public static void Init(ConfigFile config)
        {
            ConfigCategory general = config.MakeCategory(1, "General config");
            _mults = config.MakeCategory(2, "Skill Multipliers");
            ConfigCategory debug = config.MakeCategory(9, "Debug");

            DistributionMode = general.BindConfig(
                "Experience distribution mode",
                SkillDistributionLogic.EDistributionMode.WeightedRandomMax,
                "Determines how skill experience is distributed\n" +
                "Equal - All experience is equally distributed to all skills (if there is not enough XP, random will be used instead)\n" +
                "RoundRobin - Distribute XP to one skill after another in cyclic manner\n" +
                "Random - Distribute XP to random skill(s)\n" +
                "WeightedRandomMin - Distribute XP to random skill(s), skill with lower level have higher chance\n" +
                "WeightedRandomMax - Distribute XP to random skill(s), skill with higher level have higher chance\n" +
                "Min - Distribute XP to skill(s) with lowest level\n" +
                "Max - Distribute XP to skill(s) with highest level"
            );

            SkillsCount = general.BindConfig(
                "Skills count",
                3,
                "Number of skills to distribute experience to"
            );

            AllowGym = general.BindConfig(
                "Allow gym",
                true,
                "Whether or not XP from gym should be also distributed if strength/endurance is maxed"
            );

            UseBonuses = general.BindConfig(
               "Use bonuses",
               true,
               "Whether or not distributed XP used target skill bonuses"
            );

            UseEffectiveness = general.BindConfig(
               "Use effectiveness",
               true,
               "Whether or not distributed XP use and cause target skill fatigue"
            );

            CauseFatigue = general.BindConfig(
               "Cause fatigue",
               true,
               "Whether or not distributed XP cause target skill fatigue when use_effectiveness is false. This option has no effect if use effectiveness is set to true"
            );

            ExperienceMultiplier = general.BindConfig(
                "Experience multiplier",
                1.0f,
                "Experience multiplier of distributed XP. Use it to increase or decrease XP that is distributed. Cumulates with specific skill multiplier!"
            );

            GymExperienceMultiplier = general.BindConfig(
                "Experience multiplier (gym)",
                1.0f,
                "Experience multiplier of distributed XP from workout"
            );

            general.BindButton("Reset to server values", "Reset", "Pull settings from server and apply them", () =>
            {
                if (!ServerConfig.AllowOverride)
                {
                    return;
                }

                Plugin.LogDebug("Resetting to server values");
                ServerConfig.Load(true);
                NotificationHelper.ShowNotification("Applied server settings");
            });

            ApplyMultipliers = _mults.BindConfig(
                "Enable multipliers",
                false,
                "Whether to apply multipliers listed below (doesn't apply to workout - use 'Experience multiplier (gym)' instead)"
            );

            ShowDebug = debug.BindConfig(
                "Debug logs",
                false,
                "Log debug info to Player.log"
            );

            ConfigEntries =
            [
                DistributionMode,
                SkillsCount,
                AllowGym,
                UseBonuses,
                UseEffectiveness,
                CauseFatigue,
                ExperienceMultiplier,
                GymExperienceMultiplier,
                ApplyMultipliers,
            ];

            ServerConfig.Load();
        }

        public static void BuildMultipliers()
        {
            if (_mults is null)
            {
                Plugin.LogSource?.LogError("Failed to build multipliers - _mults is null");
                return;
            }

            SkillManager skillManager = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile.Skills;

            foreach (Skill skill in skillManager.DisplayList)
            {
                if (skill.Locked)
                {
                    continue;
                }

                ConfigEntry<float> entry = _mults.BindConfig(
                    $"{skill.Id} multiplier",
                    1f,
                    $"Set distributed XP multiplier when elite {skill.Id} is source of the XP. Cumulates with global experience multiplier!"
                );

                SkillMults.Add(skill.Id, entry);
            }

            ServerConfig.LoadMultipliers();
        }
    }
}
