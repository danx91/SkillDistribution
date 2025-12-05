using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using System.Reflection;

namespace SkillDistribution
{
    [Injectable(TypePriority = OnLoadOrder.PreSptModLoader + 1)]
    public class SkillDisctributionMod(ModHelper modHelper) : IOnLoad
    {
        internal static SkillDistributionConfig? Config { get; set; }

        private readonly ModHelper _modHelper = modHelper;

        public Task OnLoad()
        {
            string path = Path.Join(_modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly()), "config");
            Config = _modHelper.GetJsonDataFromFile<SkillDistributionConfig>(path, "config.jsonc");

            return Task.CompletedTask;
        }
    }
}
