using Newtonsoft.Json.Linq;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillDistribution.Helpers
{
    internal static class ServerConfig
    {
        public static bool AllowOverride { get; private set; }

        public static void Load(bool force = false)
        {
            Plugin.LogDebug("Loading settings from server...");

            JObject config = JObject.Parse(RequestHandler.GetJson("/skill-distribution/config"));

            if (bool.TryParse(config["allow_override"]?.Value<string>(), out bool allowOverride))
            {
                AllowOverride = allowOverride;

                Settings.DistributionMode.SetReadOnly(!allowOverride);
                Settings.SkillsCount.SetReadOnly(!allowOverride);
                Settings.AllowGym.SetReadOnly(!allowOverride);

                Plugin.LogDebug($"Allow override: {allowOverride}");

                if (allowOverride && !force)
                {
                    return;
                }
            }
            else
            {
                Plugin.LogSource.LogError("Failed to parse allow_override");
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

            if (int.TryParse(config["skills_count"]?.Value<string>(), out int count))
            {
                Settings.SkillsCount.Value = count;
                Plugin.LogDebug($"Skills count: {count}");
            }
            else
            {
                Plugin.LogSource.LogError("Failed to parse skills_count");
            }

            if (bool.TryParse(config["allow_gym"]?.Value<string>(), out bool allowGym))
            {
                Settings.AllowGym.Value = allowGym;
                Plugin.LogDebug($"Allow gym: {allowGym}");
            }
            else
            {
                Plugin.LogSource.LogError("Failed to parse allow_gym");
            }

            if (bool.TryParse(config["use_bonuses"]?.Value<string>(), out bool useBonuses))
            {
                Settings.UseBonuses.Value = useBonuses;
                Plugin.LogDebug($"Use bonuses: {useBonuses}");
            }
            else
            {
                Plugin.LogSource.LogError("Failed to parse use_bonuses");
            }

            if (bool.TryParse(config["use_effectiveness"]?.Value<string>(), out bool useEffectiveness))
            {
                Settings.UseEffectiveness.Value = useEffectiveness;
                Plugin.LogDebug($"Use effectiveness: {useEffectiveness}");
            }
            else
            {
                Plugin.LogSource.LogError("Failed to parse use_effectiveness");
            }

            if (bool.TryParse(config["cause_fatigue"]?.Value<string>(), out bool causeFatigue))
            {
                Settings.CauseFatigue.Value = causeFatigue;
                Plugin.LogDebug($"Cause fatigue: {causeFatigue}");
            }
            else
            {
                Plugin.LogSource.LogError("Failed to parse cause_fatigue");
            }
        }
    }
}
