using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Collections.Concurrent;

namespace SkillDistribution
{
    [Injectable(InjectionType.Singleton)]
    internal class WorkoutController(SaveServer saveServer, HideoutTable hideoutTable, HttpResponseUtil response, TimeUtil timeUtil)
    {
        private readonly ConcurrentDictionary<MongoId, WorkoutHistory> _historyByProfile = new();

        private sealed class WorkoutHistory
        {
            public SemaphoreSlim Lock { get; } = new(1, 1);
            public HashSet<Guid> Completed { get; } = [];
        }

        public async ValueTask<string> FinishWorkout(MongoId sessionId, WorkoutRequest request, CancellationToken cancellationToken)
        {
            WorkoutHistory history = _historyByProfile.GetOrAdd(sessionId, _ => new WorkoutHistory());
            await history.Lock.WaitAsync(cancellationToken);
            try
            {
                if (!history.Completed.Contains(request.WorkoutId))
                {
                    if (SkillDisctributionMod.Config is { AllowOverride: false, AllowGym: false })
                    {
                        return response.GetBody(string.Empty, BackendErrorCodes.HTTPBadRequest, "Gym distribution is disabled");
                    }

                    var profile = saveServer.GetProfile(sessionId);
                    string? error = ApplyWorkout(profile.CharacterData?.PmcData, request);
                    if (error != null)
                    {
                        return response.GetBody(string.Empty, BackendErrorCodes.HTTPBadRequest, error);
                    }

                    history.Completed.Add(request.WorkoutId);
                }

                // Finish saving the applied workout even if the client disconnects.
                await saveServer.SaveProfileAsync(sessionId, CancellationToken.None);
                return response.NullResponse();
            }
            finally
            {
                history.Lock.Release();
            }
        }

        private string? ApplyWorkout(PmcData? pmc, WorkoutRequest request)
        {
            var effects = hideoutTable.Qte.FirstOrDefault(qte => qte.Id == request.QteId)?.Results;
            if (effects == null || !effects.TryGetValue(QteEffectType.singleSuccessEffect, out var success) ||
                !effects.TryGetValue(QteEffectType.singleFailEffect, out var failure))
            {
                return $"Unknown workout QTE: {request.QteId}";
            }

            if (request.WorkoutId == Guid.Empty || request.Results is not { Length: > 0 } || request.FinalSeed is not { Length: 4 } ||
                request.Skills == null || request.FracturedArms == null)
            {
                return "Incomplete workout result";
            }

            if (pmc?.Health?.Energy?.Current == null || pmc.Health.Hydration?.Current == null ||
                pmc.Health.BodyParts == null || pmc.Hideout == null || pmc.Skills?.Common == null)
            {
                return "Incomplete workout profile";
            }

            var bodyParts = pmc.Health.BodyParts;
            if (request.FracturedArms.Any(arm => arm is not ("LeftArm" or "RightArm") || !bodyParts.ContainsKey(arm)))
            {
                return "Invalid workout fracture";
            }

            if (request.Skills.Any(entry => !Enum.IsDefined(entry.Key) || entry.Value == null ||
                !double.IsFinite(entry.Value.Progress) || entry.Value.Progress is < 0 or > 5100 ||
                !double.IsFinite(entry.Value.PointsEarnedDuringSession) || entry.Value.PointsEarnedDuringSession < 0))
            {
                return "Invalid workout skill progress";
            }

            ApplySkillProgress(pmc.Skills, request.Skills);
            ApplyWorkoutCosts(pmc.Health.Energy, pmc.Health.Hydration, request.Results, success, failure);

            foreach (string arm in request.FracturedArms)
            {
                var bodyPart = bodyParts[arm];
                bodyPart.Effects ??= [];
                bodyPart.Effects["Fracture"] = new BodyPartEffectProperties { Time = -1 };
            }

            if (effects.TryGetValue(QteEffectType.finishEffect, out var finish) && bodyParts.TryGetValue("Chest", out var chest))
            {
                ApplyMusclePain(chest, finish);
            }

            int[] seed = request.FinalSeed;
            pmc.Hideout.Seed = new QteRandomUtil(seed[0], seed[1], seed[2], seed[3]).ToSeedHex();
            return null;
        }

        private void ApplySkillProgress(Skills skills, Dictionary<SkillTypes, WorkoutSkillProgress> changes)
        {
            if (changes.Count == 0)
            {
                return;
            }

            var common = skills.Common.ToList();
            foreach (var (id, result) in changes)
            {
                var skill = common.FirstOrDefault(entry => entry.Id == id);
                if (skill == null)
                {
                    skill = new CommonSkill { Id = id };
                    common.Add(skill);
                }

                skill.Progress = result.Progress;
                skill.PointsEarnedDuringSession = result.PointsEarnedDuringSession;
                skill.LastAccess = timeUtil.GetTimeStamp();
            }
            skills.Common = common;
        }

        private static void ApplyWorkoutCosts(CurrentMinMax energy, CurrentMinMax hydration, bool[] results, QteResult success, QteResult failure)
        {
            foreach (bool succeeded in results)
            {
                QteResult effect = succeeded ? success : failure;
                energy.Current += effect.Energy ?? 0;
                hydration.Current += effect.Hydration ?? 0;
            }

            energy.Current = Math.Max(1, energy.Current!.Value);
            hydration.Current = Math.Max(1, hydration.Current!.Value);
        }

        private static void ApplyMusclePain(BodyPartHealth chest, QteResult finish)
        {
            var duration = finish.RewardEffects?.FirstOrDefault(effect => effect.Type == QteRewardType.MusclePain)?.Time;
            if (duration == null)
            {
                return;
            }

            chest.Effects ??= [];
            bool hadPain = chest.Effects.Remove("MildMusclePain") || chest.Effects.ContainsKey("SevereMusclePain");
            chest.Effects[hadPain ? "SevereMusclePain" : "MildMusclePain"] = new BodyPartEffectProperties { Time = duration };
        }
    }
}
