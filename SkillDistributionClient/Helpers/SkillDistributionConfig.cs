using Newtonsoft.Json;
using System.Collections.Generic;

namespace SkillDistribution.Helpers
{
    internal class SkillDistributionConfig
    {
        [JsonProperty("allow_override")]
        public bool? AllowOverride { get; set; }

        [JsonProperty("distribution_mode")]
        public string? DistributionMode { get; set; }

        [JsonProperty("skills_count")]
        public int? SkillsCount { get; set; }

        [JsonProperty("allow_gym")]
        public bool? AllowGym { get; set; }

        [JsonProperty("use_bonuses")]
        public bool? UseBonuses { get; set; }

        [JsonProperty("use_effectiveness")]
        public bool? UseEffectiveness { get; set; }

        [JsonProperty("cause_fatigue")]
        public bool? CauseFatigue { get; set; }

        [JsonProperty("xp_multiplier")]
        public float? XpMultiplier { get; set; }

        [JsonProperty("gym_multiplier")]
        public float? GymMultiplier { get; set; }

        [JsonProperty("enable_multipliers")]
        public bool? EnableMultipliers { get; set; }

        [JsonProperty("skill_multipliers")]
        public Dictionary<string, float>? SkillMultipliers { get; set; }
    }
}
