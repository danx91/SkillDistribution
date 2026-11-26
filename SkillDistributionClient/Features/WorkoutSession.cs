using EFT;
using EFT.Hideout;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillDistribution.Features
{
    internal class WorkoutSession
    {
        public static WorkoutSession? Current { get; set; }
        public WorkoutBehaviour Workout { get; }

        private readonly Guid _id = Guid.NewGuid();
        private readonly HashSet<Skill> _changedSkills = [];
        private readonly EBodyPart[] _healthyArms;

        public WorkoutSession(WorkoutBehaviour workout)
        {
            Workout = workout;
            EBodyPart[] arms = [EBodyPart.LeftArm, EBodyPart.RightArm];
            _healthyArms = [.. arms.Where(arm => !workout._offlineHealthController.IsBodyPartBroken(arm))];
        }

        public void RecordChanges(Dictionary<Skill, float> previous)
        {
            foreach (var (skill, progress) in previous)
            {
                if (skill.Current > progress)
                {
                    _changedSkills.Add(skill);
                }
            }
        }

        public object GetResult(string qteId, IEnumerable<bool> results)
        {
            HideoutRepresentation hideout = Workout._hideout;
            Seed seed = hideout._randomXorShift?.GetNewSeed() ?? hideout._seed;
            return new
            {
                workoutId = _id,
                qteId,
                results = results.ToArray(),
                skills = _changedSkills.ToDictionary(skill => skill.Id.ToString(), skill => new
                {
                    progress = skill.Current,
                    pointsEarnedDuringSession = skill.PointsEarned
                }),
                fracturedArms = _healthyArms.Where(Workout._offlineHealthController.IsBodyPartBroken)
                    .Select(arm => arm.ToString()).ToArray(),
                finalSeed = new[] { seed.x, seed.y, seed.z, seed.w }
            };
        }
    }
}
