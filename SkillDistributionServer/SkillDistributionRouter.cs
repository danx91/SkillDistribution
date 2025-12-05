using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SkillDistribution
{
    [Injectable]
    internal class SkillDistributionRouter : StaticRouter
    {
        private static JsonUtil? _jsonUtil;

        public SkillDistributionRouter(JsonUtil jsonUtil) : base(jsonUtil, GetRoutes())
        {
            _jsonUtil = jsonUtil;
        }

        private static List<RouteAction> GetRoutes()
        {
            return [
                new RouteAction<EmptyRequestData>(
                    "/skill-distribution/config",
                    (url, info, sessionId, output) => HandleRoute()
                )
            ];
        }

        private static ValueTask<string> HandleRoute()
        {
            return new ValueTask<string>(_jsonUtil!.Serialize(SkillDisctributionMod.Config!)!);
        }
    }
}
