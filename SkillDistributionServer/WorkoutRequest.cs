using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using System.Text.Json.Serialization;

namespace SkillDistribution
{
    internal class WorkoutRequest : IRequestData
    {
        [JsonPropertyName("workoutId")]
        public required Guid WorkoutId { get; set; }

        [JsonPropertyName("qteId")]
        public required string QteId { get; set; }

        [JsonPropertyName("results")]
        public required bool[] Results { get; set; }

        [JsonPropertyName("skills")]
        public required Dictionary<SkillTypes, WorkoutSkillProgress> Skills { get; set; }

        [JsonPropertyName("fracturedArms")]
        public required string[] FracturedArms { get; set; }

        [JsonPropertyName("finalSeed")]
        public required int[] FinalSeed { get; set; }
    }

    internal class WorkoutSkillProgress
    {
        [JsonPropertyName("progress")]
        public required double Progress { get; set; }

        [JsonPropertyName("pointsEarnedDuringSession")]
        public required double PointsEarnedDuringSession { get; set; }
    }
}
