using BepInEx.Configuration;
using Newtonsoft.Json.Linq;
using SPT.Common.Http;
using System;

namespace SkillDistribution.Helpers
{
    internal static class ServerConfig
    {
        public static bool AllowOverride { get; private set; }

        public static void Load(bool force = false)
        {
            Plugin.LogDebug("Loading settings from server...");

            JObject config = JObject.Parse(RequestHandler.GetJson("/skill-distribution/config"));

            ParseAndApply<bool>(config, "allow_override", bool.TryParse, callback: allowOverride =>
            {
                AllowOverride = allowOverride;

                foreach (ConfigEntryBase entry in Settings.ConfigEntries)
                {
                    entry.SetReadOnly(!allowOverride);
                }
            });

            if (AllowOverride && !force)
            {
                return;
            }

            if (Enum.TryParse(config["distribution_mode"]?.Value<string>(), true, out SkillHelper.EDistributionMode mode))
            {
                Settings.DistributionMode.Value = mode;
                Plugin.LogDebug($"Distribution mode: {mode}");
            }
            else
            {
                Plugin.LogSource.LogError("Failed to parse distribution_mode");
            }

            ParseAndApply(config, "skills_count", int.TryParse, Settings.SkillsCount);
            ParseAndApply(config, "allow_gym", bool.TryParse, Settings.AllowGym);
            ParseAndApply(config, "use_effectiveness", bool.TryParse, Settings.UseEffectiveness);
            ParseAndApply(config, "cause_fatigue", bool.TryParse, Settings.CauseFatigue);
            ParseAndApply(config, "use_bonuses", bool.TryParse, Settings.UseBonuses);
            ParseAndApply(config, "xp_multiplier", float.TryParse, Settings.ExperienceMultiplier);
            ParseAndApply(config, "gym_multiplier", float.TryParse, Settings.GymExperienceMultiplier);
        }

        public static void ParseAndApply<T>(
            JObject config,
            string key,
            TryParseDelegate<T> tryParse,
            ConfigEntry<T> entry = null,
            Action<T> callback = null)
        {
            string rawValue = config[key]?.Value<string>();
            if (rawValue != null && tryParse(rawValue, out T result))
            {
                Plugin.LogDebug($"{entry?.Definition.Key ?? key}: {result}");

                if (entry != null)
                {
                    entry.Value = result;
                }

                callback?.Invoke(result);
            }
            else
            {
                Plugin.LogSource.LogError($"Failed to parse {key}");
            }
        }

        public delegate bool TryParseDelegate<T>(string value, out T result);
    }
}
