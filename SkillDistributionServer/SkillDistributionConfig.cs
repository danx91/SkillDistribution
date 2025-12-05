using System.Text.Json.Serialization;

namespace SkillDistribution
{
    internal class SkillDistributionConfig
    {
        [JsonPropertyName("allow_override")]
        public bool? AllowOverride { get; set; }

        [JsonPropertyName("distribution_mode")]
        public string? DistributionMode { get; set; }

        [JsonPropertyName("skills_count")]
        public int? SkillsCount { get; set; }

        [JsonPropertyName("allow_gym")]
        public bool? AllowGym { get; set; }

        [JsonPropertyName("use_bonuses")]
        public bool? UseBonuses { get; set; }

        [JsonPropertyName("use_effectiveness")]
        public bool? UseEffectiveness { get; set; }

        [JsonPropertyName("cause_fatigue")]
        public bool? CauseFatigue { get; set; }

        [JsonPropertyName("xp_multiplier")]
        public int? XpMultiplier { get; set; }

        [JsonPropertyName("gym_multiplier")]
        public int? GymMultiplier { get; set; }
    }
}
