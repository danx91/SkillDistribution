using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers.Server;
using System.Reflection;

namespace SkillDistribution
{
    [Injectable(TypePriority = OnLoadOrder.Preload + 1)]
    public class SkillDisctributionMod(ModHelper modHelper) : IOnLoad
    {
        internal static SkillDistributionConfig? Config { get; set; }

        private readonly ModHelper _modHelper = modHelper;

        public async Task OnLoadAsync(CancellationToken cancellationToken)
        {
            string path = Path.Join(_modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config");
            Config = _modHelper.GetJsonDataFromFile<SkillDistributionConfig>(path, "config.jsonc");
        }
    }
}
