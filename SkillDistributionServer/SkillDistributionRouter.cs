using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SkillDistribution
{
    [Injectable(TypePriority = OnLoadOrder.Routers + 1)]
    internal class SkillDistributionRouter : StaticRouter
    {
        private static JsonUtil? _jsonUtil;

        public SkillDistributionRouter(JsonUtil jsonUtil, WorkoutController workoutController) : base(jsonUtil, GetRoutes(workoutController))
        {
            _jsonUtil = jsonUtil;
        }

        private static List<RouteAction> GetRoutes(WorkoutController workoutController)
        {
            return [
                new RouteAction<EmptyRequestData>(
                    "/skill-distribution/config",
                    (url, info, sessionId, output, cancellationToken) => new ValueTask<string>(_jsonUtil!.Serialize(SkillDisctributionMod.Config!)!)
                ),
                new RouteAction<WorkoutRequest>(
                    "/skill-distribution/workout",
                    (url, info, sessionId, output, cancellationToken) => workoutController.FinishWorkout(sessionId, info, cancellationToken)
                )
            ];
        }
    }
}
