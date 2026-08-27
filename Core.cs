using EternalCycleServer;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Modding.Custom;
using SPTarkov.Server.Core.Services.Ragfair;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using System.Reflection;
using static EternalCycleServer.ContextManager;

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
    public record VulcanInfinity : IModMetadata
    {
        /// <summary>
        /// Any string can be used for a modId, but it should ideally be unique and not easily duplicated
        /// a 'bad' ID would be: "mymod", "mod1", "questmod"
        /// It is recommended (but not mandatory) to use the reverse domain name notation,
        /// see: https://docs.oracle.com/javase/tutorial/java/package/namingpkgs.html
        /// </summary>
        public  string ModGuid { get; init; } = "com.hiddenhiragi.vulcanmod";

        /// <summary>
        /// The name of your mod
        /// </summary>
        public  string Name { get; init; } = "火神重工";

        /// <summary>
        /// Who created the mod (you!)
        /// </summary>
        public  string Author { get; init; } = "HiddenHiragi";

        /// <summary>
        /// A list of people who helped you create the mod
        /// </summary>
        public  List<string>? Contributors { get; init; }

        /// <summary>
        ///  The version of the mod, follows SEMVER rules (https://semver.org/)
        /// </summary>
        public  SemanticVersioning.Version Version { get; init; } = new("1.1.1");

        /// <summary>
        /// What version of SPT is your mod made for, follows SEMVER rules (https://semver.org/)
        /// </summary>
        public  SemanticVersioning.Range SptVersion { get; init; } = new("~4.1.0");

        /// <summary>
        /// ModIds that you know cause problems with your mod
        /// </summary>
        public  List<string>? Incompatibilities { get; init; }

        /// <summary>
        /// ModIds your mod REQUIRES to function
        /// </summary>
        public  Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new()
{
    { "projectspark.hiddenhiragi.eternalcycleserver", new SemanticVersioning.Range(">=1.2.0") }
};
        /// <summary>
        /// Where to find your mod online
        /// </summary>
        public  string? Url { get; init; } = "https://github.com/sp-tarkov/server-mod-examples";

        /// <summary>
        /// Does your mod load bundles? (e.g. new weapon/armor mods)
        /// </summary>
        public  bool? IsBundleMod { get; init; } = true;

        /// <summary>
        /// What Licence does your mod use
        /// </summary>
        public  string? License { get; init; } = "MIT";
        public bool HasPrepatcher { get; init; } = false;
    }

    [Injectable(TypePriority = OnLoadOrder.Preload + 1)]
    public class PatchEnabler(IEnumerable<IRuntimePatch> patches) : IOnLoad
    {
        public Task OnLoadAsync(CancellationToken cancellationToken)
        {
            foreach (var patch in patches)
                patch.Enable();
            return Task.CompletedTask;
        }
    }

    // We want to load after PreSptModLoader is complete, so we set our type priority to that, plus 1.
    [Injectable(TypePriority = OnLoadOrder.Preload + 1)]
    public class Core(
        CustomItemService customItemService,
        ModHelper modHelper,
        ItemHelper itemHelper,
        JsonUtil jsonUtil,
        ICloner cloner,
        ConfigServer configServer,
        ImageRouter imageRouter,
        PresetHelper presetHelper,
        RagfairOfferService ragfairOfferService,
        RagfairController ragfairController,
        TemplateTable templateTable,
        LocaleTable localeTable,
        GlobalTable globalTable,
        TradersTable tradersTable,
        HideoutTable hideoutTable,
        LocationTable locationTable,
        HandbookHelper handbookHelper
        ) // We inject a logger for use inside our class, it must have the class inside the diamond <> brackets
        : IOnLoad // Implement the IOnLoad interface so that this mod can do something on server load
    {
        public string modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
        public async Task OnLoadAsync(CancellationToken cancellationToken)
        {
            Utils.commonLogger.Info("不破其旧，无以立新！");
            Utils.commonLogger.Info("我知晓所有的道路，它们都通往同一个地方。");
            //coreconfigs.Fixes.RemoveInvalidTradersFromProfile = true;
            //coreconfigs.Fixes.RemoveModItemsFromProfile = true;
            //coreconfigs.Fixes.FixProfileBreakingInventoryItemIssues = true;
            EventManager.DataLoadEvent.PreDataLoadEvent += VulcanMod.Init;
            //new RagfairLoadPatch().Enable();
            //logger.Debug("This is a debug message that gets written to the log file, not the console");


            // Inform the server our mod has finished doing work
            return;
        }
    }

}