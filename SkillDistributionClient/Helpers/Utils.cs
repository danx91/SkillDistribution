using Comfort.Common;
using EFT;

namespace SkillDistribution.Helpers
{
    internal static class Utils
    {
        public static bool IsInRaid()
        {
            bool? inRaid = Singleton<AbstractGame>.Instance?.InRaid;
            return inRaid.HasValue && inRaid.Value;
        }
    }
}
