using BepInEx.Configuration;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using ZGFueDkx.ZGCLib.Config;
using ZGFueDkx.ZGCLib.Helpers;

namespace SkillDistribution.Helpers
{
    internal static class ServerConfig
    {
        public static bool AllowOverride { get; private set; }
        private static Dictionary<string, float>? _skillMultipliers;

        public static void Load(bool force = false)
        {
            _skillMultipliers = null;

            Plugin.LogSource?.LogInfo("Loading settings from server");

            string response = RequestHandler.GetJson("/skill-distribution/config");

            SkillDistributionConfig? config = JsonConvert.DeserializeObject<SkillDistributionConfig>(
                response,
                JsonSettingsFactory.GetJsonSerializerSettings(Plugin.LogSource)
            );

            Plugin.LogDebug(response);

            if(config is null)
            {
                Plugin.LogSource?.LogError("Failed to parse _questData!");
                Plugin.LogSource?.LogError(response);
                return;
            }

            if(config.AllowOverride is bool allowOverride)
            {
                AllowOverride = allowOverride;

                foreach (ConfigEntryBase entry in Settings.ConfigEntries!)
                {
                    entry.SetReadOnly(!allowOverride);
                }
            }

            if (config.SkillMultipliers is not null)
            {
                _skillMultipliers = config.SkillMultipliers;
            }
            else
            {
                Plugin.LogSource?.LogError("SkillMultipliers is null");
            }

            if (AllowOverride && !force)
            {
                LoadMultipliers();
                return;
            }

            if (config.DistributionMode is string distributionMode && Enum.TryParse(distributionMode, true, out SkillHelper.EDistributionMode mode))
            {
                Settings.DistributionMode!.Value = mode;
                Plugin.LogDebug($"Distribution mode: {mode}");
            }
            else
            {
                Plugin.LogSource?.LogError("Failed to parse distribution_mode");
            }

            TryApply(config.SkillsCount, Settings.SkillsCount!);
            TryApply(config.AllowGym, Settings.AllowGym!);
            TryApply(config.UseEffectiveness, Settings.UseEffectiveness!);
            TryApply(config.CauseFatigue, Settings.CauseFatigue!);
            TryApply(config.UseBonuses, Settings.UseBonuses!);
            TryApply(config.XpMultiplier, Settings.ExperienceMultiplier!);
            TryApply(config.GymMultiplier, Settings.GymExperienceMultiplier!);

            TryApply(config.EnableMultipliers, Settings.ApplyMultipliers!);

            LoadMultipliers(force);
        }

        public static void LoadMultipliers(bool force = false)
        {
            if (_skillMultipliers is null)
            {
                Plugin.LogSource?.LogError("_skillMultipliers is null");
                return;
            }

            if (Settings.SkillMults.Count == 0)
            {
                Plugin.LogDebug("LoadMultipliers called too early - SkillMults is empty");
                return;
            }

            Plugin.LogDebug("Loading skill multipliers");

            Dictionary<ESkillId, float> parsed = [];

            foreach (var (skillIdRaw, mult) in _skillMultipliers)
            {
                if (!Enum.TryParse(skillIdRaw, true, out ESkillId skillId))
                {
                    Plugin.LogSource?.LogError($"Unknown skill type: {skillIdRaw}");
                    continue;
                }

                Plugin.LogDebug($"Skill parsed: {skillId} = {mult}");
                parsed[skillId] = mult;
            }

            foreach (var (skillId, entry) in Settings.SkillMults)
            {
                entry.SetReadOnly(!AllowOverride);

                if (AllowOverride && !force)
                {
                    continue;
                }

                if (parsed.TryGetValue(skillId, out float overrideValue))
                {
                    entry.Value = overrideValue;
                    Plugin.LogDebug($"Skill value override: {skillId} = {overrideValue}");
                }
                else
                {
                    entry.Value = (float)entry.DefaultValue;
                    Plugin.LogDebug($"Skill ${skillId} not found - using default");
                }
            }
        }

        public static void TryApply<T> (
            T? value,
            ConfigEntry<T> entry
        ) where T : struct
        {
            if (value is T result)
            {
                Plugin.LogDebug($"{entry.Definition.Key}: {result}");
                entry.Value = result;
            }
            else
            {
                Plugin.LogSource?.LogError($"Failed to parse {entry.Definition.Key}");
            }
        }
    }
}
