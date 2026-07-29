using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using System.Reflection;
using EternalCycleServer;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace VulcanMod
{

    /// <summary>
    /// This is the replacement for the former package.json data. This is required for all mods.
    ///
    /// This is where we define all the metadata associated with this mod.
    /// You don't have to do anything with it, other than fill it out.
    /// All properties must be overriden, properties you don't use may be left null.
    /// It is read by the mod loader when this mod is loaded.
    /// </summary>
    public record VulcanInfinity : AbstractModMetadata
    {
        /// <summary>
        /// Any string can be used for a modId, but it should ideally be unique and not easily duplicated
        /// a 'bad' ID would be: "mymod", "mod1", "questmod"
        /// It is recommended (but not mandatory) to use the reverse domain name notation,
        /// see: https://docs.oracle.com/javase/tutorial/java/package/namingpkgs.html
        /// </summary>
        public override string ModGuid { get; init; } = "com.hiddenhiragi.vulcanmod";

        /// <summary>
        /// The name of your mod
        /// </summary>
        public override string Name { get; init; } = "火神重工";

        /// <summary>
        /// Who created the mod (you!)
        /// </summary>
        public override string Author { get; init; } = "HiddenHiragi";

        /// <summary>
        /// A list of people who helped you create the mod
        /// </summary>
        public override List<string>? Contributors { get; init; }

        /// <summary>
        ///  The version of the mod, follows SEMVER rules (https://semver.org/)
        /// </summary>
        public override SemanticVersioning.Version Version { get; init; } = new("1.0.3");

        /// <summary>
        /// What version of SPT is your mod made for, follows SEMVER rules (https://semver.org/)
        /// </summary>
        public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.13");

        /// <summary>
        /// ModIds that you know cause problems with your mod
        /// </summary>
        public override List<string>? Incompatibilities { get; init; }

        /// <summary>
        /// ModIds your mod REQUIRES to function
        /// </summary>
        public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new()
{
    { "projectspark.hiddenhiragi.eternalcycleserver", new SemanticVersioning.Range(">=1.1.0") }
};
        /// <summary>
        /// Where to find your mod online
        /// </summary>
        public override string? Url { get; init; } = "https://github.com/sp-tarkov/server-mod-examples";

        /// <summary>
        /// Does your mod load bundles? (e.g. new weapon/armor mods)
        /// </summary>
        public override bool? IsBundleMod { get; init; } = true;

        /// <summary>
        /// What Licence does your mod use
        /// </summary>
        public override string? License { get; init; } = "MIT";
    }

    // We want to load after PreSptModLoader is complete, so we set our type priority to that, plus 1.
    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
    public class Core(
        DatabaseService databaseService,
        CustomItemService customItemService,
        ModHelper modHelper,
        JsonUtil jsonutil,
        ICloner cloner,
        ConfigServer configServer,
        ImageRouter imageRouter
        ) // We inject a logger for use inside our class, it must have the class inside the diamond <> brackets
        : IOnLoad // Implement the IOnLoad interface so that this mod can do something on server load
    {
        public string modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        public Task OnLoad()
        {
            Utils.commonLogger.Info("不破其旧，无以立新！");
            Utils.commonLogger.Info("我知晓所有的道路，它们都通往同一个地方。");
            var modConfig = ConfigManager.GetConfig();
            var coreconfigs = configServer.GetConfig<CoreConfig>();
            //coreconfigs.Fixes.RemoveInvalidTradersFromProfile = true;
            //coreconfigs.Fixes.RemoveModItemsFromProfile = true;
            //coreconfigs.Fixes.FixProfileBreakingInventoryItemIssues = true;
            VulcanMod.Init(databaseService, customItemService, modHelper, jsonutil, cloner, configServer, imageRouter);
            //new RagfairLoadPatch().Enable();
            //logger.Debug("This is a debug message that gets written to the log file, not the console");


            // Inform the server our mod has finished doing work
            return Task.CompletedTask;
        }
    }

}