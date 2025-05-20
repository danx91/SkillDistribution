using BepInEx.Configuration;
using System;
using System.Linq;
using UnityEngine;

namespace SkillDistribution.Helpers
{
    internal static class Settings
    {
        public static ConfigEntry<SkillHelper.EDistributionMode> DistributionMode;
        public static ConfigEntry<int> SkillsCount;
        public static ConfigEntry<bool> AllowGym;
        public static ConfigEntry<bool> UseEffectiveness;
        public static ConfigEntry<bool> CauseFatigue;
        public static ConfigEntry<bool> UseBonuses;
        public static ConfigEntry<bool> ShowDebug;

        public static void Init(ConfigFile config)
        {
            DistributionMode = config.Bind(
                "1. General config",
                "Experience distribution mode",
                SkillHelper.EDistributionMode.Equal,
                MakeDescription("Determines how skill experience is distributed\n" +
                    "Equal - All experience is equally distributed to all skills (if there is not enough XP, random will be used instead)\n" +
                    "RoundRobin - Distribute XP to one skill after another in cyclic manner\n" +
                    "Random - Distribute XP to random skill(s)\n" +
                    "WeightedRandomMin - Distribute XP to random skill(s), skill with lower level have higher chance\n" +
                    "WeightedRandomMax - Distribute XP to random skill(s), skill with higher level have higher chance\n" +
                    "Min - Distribute XP to skill(s) with lowest level\n" +
                    "Max - Distribute XP to skill(s) with highest level",
                    100
                )
            );

            SkillsCount = config.Bind(
                "1. General config",
                "Skills count",
                3,
                MakeDescription("Number of skills to distribute experience to", 99)
            );

            AllowGym = config.Bind(
                "1. General config",
                "Allow gym",
                true,
                MakeDescription("Whether or not XP from gym should be also distributed if strength/endurance is maxed", 98)
            );

            UseBonuses = config.Bind(
               "1. General config",
               "Use bonuses",
               true,
               MakeDescription("Whether or not distributed XP used target skill bonuses", 97)
            );

            UseEffectiveness = config.Bind(
               "1. General config",
               "Use effectiveness",
               true,
               MakeDescription("Whether or not distributed XP use and cause traget skill fatigue", 96)
            );

            CauseFatigue = config.Bind(
               "1. General config",
               "Cause fatigue",
               true,
               MakeDescription("Whether or not distributed XP cause traget skill fatigue when use_effectiveness is false. This option has no effect if use effectiveness is set to true", 95)
            );

            //Should 

            config.BindButton("2. Reset", "Reset to server values", "Reset", "Pull settings from server and apply them", 50, () =>
            {
                if(!ServerConfig.AllowOverride)
                {
                   return;
                }

                Plugin.LogDebug("Resetting to server values");
                ServerConfig.Load(true);
                Notifications.ShowNotification("Applied server settings");
            });
            
            ShowDebug = config.Bind(
                "9. Debug",
                "Debug logs",
                false,
                "Log debug info to Player.log"
            );

            ServerConfig.Load();
        }

        private static ConfigDescription MakeDescription(string description, int order)
        {
            return new ConfigDescription(
                description,
                null,
                new ConfigurationManagerAttributes
                {
                    IsAdvanced = false,
                    Order = order,
                }
            );
        }

        public static void BindButton(this ConfigFile config, string section, string key, string text, string description, int order, Action action)
        {
            config.Bind(section, key, "", 
                new ConfigDescription(description, null, new ConfigurationManagerAttributes
                {
                    CustomDrawer = entry =>
                    {
                        if (GUILayout.Button(text, GUILayout.ExpandWidth(true)))
                        {
                            action();
                        }
                    },
                    Order = order,
                })
            );
        }

        public static void SetReadOnly<T>(this ConfigEntry<T> entry, bool readOnly)
        {
            ConfigurationManagerAttributes attributes = (ConfigurationManagerAttributes)
                entry.Description.Tags.FirstOrDefault(t => t.GetType() == typeof(ConfigurationManagerAttributes));

            if(attributes == null)
            {
                return;
            }

            attributes.ReadOnly = readOnly;
        }
    }
}
