using EternalCycleServer;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using System.Reflection;
using System.Text;
using static EternalCycleServer.ContextManager;
using SPTarkov.Server.Core.Generators.Bot;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Services.InRaid;

namespace VulcanMod
{

    public class BotGeneratorPatch
    {
        public static VulcanModConfigClass modConfig = ConfigManager.GetConfig();
        public static List<string> alterBossName = new List<string>{
                "Dullahan",
                "Golyat",
                "Argus",
                "Sanitar",
                "Ghroth",
                "Punisher",
                "Obsidian"
                };
        [Injectable]
        public class GenerateBotPatch : AbstractPatch
        {
            // 异格判定状态原先在这里（SanitarCounter / GluharCocunter / KolontayCocunter /
            // ObsidianCounter / GoonsCounter）。已统一改为 AlterBossDecision（见文件末尾）：
            // 旧的「每 5 次调用无条件复位 + 无锁 static」会让同一局里异格和原版混着出场，
            // 而战利品池是按角色每局缓存一次的 —— 混出场时异格会抽到原版的池子。
            private static readonly object _dogTagLock = new object();

            public static string BotLoation = "";

            private static TemplateTable _templateTable = default!;
            private static LocaleTable _localeTable = default!;
            private static GlobalTable _globalTable = default!;
            private static TradersTable _tradersTable = default!;
            private static HideoutTable _hideoutTable = default!;
            private static LocationTable _locationTable = default!;
            private static BotTable _botTable = default!;
            private static JsonUtil _jsonUtil = default!;
            private static ConfigServer _configServer = default!;
            private static ModHelper _modHelper = default!;
            private static ItemHelper _itemHelper = default!;
            private static LocaleService _localeService = default!;
            private static ICloner _cloner = default!;
            private static PresetHelper _presetHelper = default!;
            private static ImageRouter _imageRouter = default!;
            private static ECLogger _logger = default!;
            public GenerateBotPatch(
            TemplateTable templateTable,
            LocaleTable localeTable,
            GlobalTable globalTable,
            TradersTable tradersTable,
            HideoutTable hideoutTable,
            LocationTable locationTable,
            BotTable botTable,
            JsonUtil jsonUtil,
            ConfigServer configServer,
            ModHelper modHelper,
            ItemHelper itemHelper,
            LocaleService localeService,
            ICloner cloner,
            PresetHelper presetHelper,
            ImageRouter imageRouter)
            {
                _templateTable = templateTable;
                _localeTable = localeTable;
                _globalTable = globalTable;
                _tradersTable = tradersTable;
                _hideoutTable = hideoutTable;
                _locationTable = locationTable;
                _botTable = botTable;
                _jsonUtil = jsonUtil;
                _configServer = configServer;
                _modHelper = modHelper;
                _itemHelper = itemHelper;
                _localeService = localeService;
                _cloner = cloner;
                _presetHelper = presetHelper;
                _imageRouter = imageRouter;
                _logger = new ECLogger("BotGenerator", true);
            }
            protected override MethodBase GetTargetMethod()
            {
                return typeof(BotGenerator).GetMethod("GenerateBot", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }
            [PatchPrefix]
            public static bool Prefix(BotGenerator __instance, MongoId sessionId, BotBase bot, BotType botJsonTemplate, BotGenerationDetails botGenerationDetails, ref BotBase __result)
            {
                var botConfig = _configServer.GetConfig<BotConfig>();
                var botRoleLowercase = botGenerationDetails.Role.ToLowerInvariant();
                var modpath = System.IO.Path.Combine(ConfigManager.modPath, $"{ConfigManager.dataPath}");
                var botSanitar = _modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Sanitar.json");
                var botKnight = _modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Knight.json");
                var botBigPipe = _modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/BigPipe.json");
                var botBirdeye = _modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Birdeye.json");
                var botGluhar = _modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Gluhar.json");
                var botKolontay = _modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Kolontay.json");
                var botObsidian = _modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Obsidian.json");
                try
                {
                    lock (_dogTagLock)
                    {
                        if (!botConfig.BotRolesWithDogTags.Contains(botRoleLowercase))
                        {
                            botConfig.BotRolesWithDogTags.Add(botRoleLowercase);
                        }
                    }

                    if (BotLoation != botGenerationDetails.Location)
                    {
                        BotLoation = (string)botGenerationDetails.Location;
                        //logger.LogWithColor($"Correct Location: {botGenerationDetails.Location}", LogTextColor.Magenta);
                    }
                    //logger.LogWithColor($"Bot Location: {botGenerationDetails.Location}", LogTextColor.Magenta);
                    if (modConfig.EnableAlterBoss)
                    {
                        if (botRoleLowercase == "bossknight")
                        {
                            // Goons 三只共用 "goons" 这个 key —— 小队同进同退
                            if (AlterBossDecision.IsAltered("goons", modConfig.AlterBossChance.Goons, out _))
                            {
                                botJsonTemplate.BotAppearance = botKnight.BotAppearance;
                                botJsonTemplate.BotExperience = botKnight.BotExperience;
                                botJsonTemplate.BotHealth = botKnight.BotHealth;
                                botJsonTemplate.BotSkills = botKnight.BotSkills;
                                botJsonTemplate.BotInventory = botKnight.BotInventory;
                                botJsonTemplate.BotChances = botKnight.BotChances;
                                botJsonTemplate.FirstNames = botKnight.FirstNames;
                                botJsonTemplate.BotGeneration = botKnight.BotGeneration;
                                //唱片和箭头预留部分
                            }
                        }
                        if (botRoleLowercase == "followerbigpipe")
                        {
                            // Goons 三只共用 "goons" 这个 key —— 小队同进同退
                            if (AlterBossDecision.IsAltered("goons", modConfig.AlterBossChance.Goons, out _))
                            {
                                botJsonTemplate.BotAppearance = botBigPipe.BotAppearance;
                                botJsonTemplate.BotExperience = botBigPipe.BotExperience;
                                botJsonTemplate.BotHealth = botBigPipe.BotHealth;
                                botJsonTemplate.BotSkills = botBigPipe.BotSkills;
                                botJsonTemplate.BotInventory = botBigPipe.BotInventory;
                                botJsonTemplate.BotChances = botBigPipe.BotChances;
                                botJsonTemplate.FirstNames = botBigPipe.FirstNames;
                                botJsonTemplate.LastNames = botBigPipe.LastNames;
                                botJsonTemplate.BotGeneration = botBigPipe.BotGeneration;
                                //唱片预留
                            }
                        }
                        if (botRoleLowercase == "followerbirdeye")
                        {
                            // Goons 三只共用 "goons" 这个 key —— 小队同进同退
                            if (AlterBossDecision.IsAltered("goons", modConfig.AlterBossChance.Goons, out _))
                            {
                                botJsonTemplate.BotAppearance = botBirdeye.BotAppearance;
                                botJsonTemplate.BotExperience = botBirdeye.BotExperience;
                                botJsonTemplate.BotHealth = botBirdeye.BotHealth;
                                botJsonTemplate.BotSkills = botBirdeye.BotSkills;
                                botJsonTemplate.BotInventory = botBirdeye.BotInventory;
                                botJsonTemplate.BotChances = botBirdeye.BotChances;
                                botJsonTemplate.FirstNames = botBirdeye.FirstNames;
                                botJsonTemplate.BotGeneration = botBirdeye.BotGeneration;
                                //唱片预留
                            }
                        }
                        if (botRoleLowercase == "bosssanitar")
                        {
                            if (AlterBossDecision.IsAltered("bosssanitar", modConfig.AlterBossChance.Sanitar, out _))
                            {
                                botJsonTemplate.BotAppearance = botSanitar.BotAppearance;
                                botJsonTemplate.BotExperience = botSanitar.BotExperience;
                                botJsonTemplate.BotHealth = botSanitar.BotHealth;
                                botJsonTemplate.BotSkills = botSanitar.BotSkills;
                                botJsonTemplate.BotInventory = botSanitar.BotInventory;
                                botJsonTemplate.BotChances = botSanitar.BotChances;
                                botJsonTemplate.FirstNames = botSanitar.FirstNames;
                                botJsonTemplate.BotGeneration = botSanitar.BotGeneration;
                            }
                        }
                        if (botRoleLowercase == "bossgluhar")
                        {
                            if (AlterBossDecision.IsAltered("bossgluhar", modConfig.AlterBossChance.Gluhar, out _))
                            {
                                botJsonTemplate.BotAppearance = botGluhar.BotAppearance;
                                botJsonTemplate.BotExperience = botGluhar.BotExperience;
                                botJsonTemplate.BotHealth = botGluhar.BotHealth;
                                botJsonTemplate.BotSkills = botGluhar.BotSkills;
                                botJsonTemplate.BotInventory = botGluhar.BotInventory;
                                botJsonTemplate.BotChances = botGluhar.BotChances;
                                botJsonTemplate.FirstNames = botGluhar.FirstNames;
                                botJsonTemplate.BotGeneration = botGluhar.BotGeneration;
                            }
                        }
                        if (botRoleLowercase == "bosskolontay")
                        {
                            if (AlterBossDecision.IsAltered("bosskolontay", modConfig.AlterBossChance.Kolontay, out _))
                            {
                                botJsonTemplate.BotAppearance = botKolontay.BotAppearance;
                                botJsonTemplate.BotExperience = botKolontay.BotExperience;
                                botJsonTemplate.BotHealth = botKolontay.BotHealth;
                                botJsonTemplate.BotSkills = botKolontay.BotSkills;
                                botJsonTemplate.BotInventory = botKolontay.BotInventory;
                                botJsonTemplate.BotChances = botKolontay.BotChances;
                                botJsonTemplate.FirstNames = botKolontay.FirstNames;
                                botJsonTemplate.BotGeneration = botKolontay.BotGeneration;
                            }
                        }
                        if (botRoleLowercase == "bosswedgelab")
                        {
                            // 黑曜石(Obsidian) = 伟哥小队本尊(bossWedgeLab) 的异格变体。
                            // 一局只判定一次：当局内要么全是黑曜石、要么全是原版伟哥，
                            // 否则战利品池缓存（按角色、每局一次）会和模板错配。
                            if (AlterBossDecision.IsAltered("bosswedgelab", modConfig.AlterBossChance.Obsidian, out _))
                            {
                                // 本局命中：用文件数据覆盖原版模板
                                botJsonTemplate.BotAppearance = botObsidian.BotAppearance;
                                botJsonTemplate.BotExperience = botObsidian.BotExperience;
                                botJsonTemplate.BotHealth = botObsidian.BotHealth;
                                botJsonTemplate.BotSkills = botObsidian.BotSkills;
                                botJsonTemplate.BotInventory = botObsidian.BotInventory;
                                botJsonTemplate.BotChances = botObsidian.BotChances;
                                botJsonTemplate.FirstNames = botObsidian.FirstNames;
                                botJsonTemplate.LastNames = botObsidian.LastNames;
                                botJsonTemplate.BotGeneration = botObsidian.BotGeneration;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("错误: BotGeneratorPatch运行时出现问题: ", ex);
                }

                return true;
            }

            [PatchPostfix]
            public static void Postfix(BotGenerator __instance, MongoId sessionId, BotBase bot, BotType botJsonTemplate, BotGenerationDetails botGenerationDetails, ref BotBase __result)
            {
                var context = new LoadModContext
                {
                    DB = new DatabaseService(_templateTable, _localeTable, _globalTable, _tradersTable, _hideoutTable, _locationTable, _botTable),
                    JsonUtil = _jsonUtil,
                    ConfigServer = _configServer,
                    ModHelper = _modHelper,
                    Logger = Utils.commonLogger,
                    PresetHelper = _presetHelper,
                    ImageRouter = _imageRouter,
                    ItemHelper = _itemHelper,
                    Cloner = _cloner
                };

                var botRoleLowercase = botGenerationDetails.Role.ToLowerInvariant();
                var botRole = __result.Info.Settings.Role;
                var botRoleLower = botRole.ToLower();
                var botNickName = __result.Info.Nickname;
                switch (botRoleLower)
                {
                    case "bossbully":
                        AddLootToInventory(__result.Inventory, "宿舍楼管理员钥匙".ConvertHashID(), "Pockets", __result, context);
                        AddLootToInventory(__result.Inventory, ItemTpl.INFO_NOTE_WITH_CODE_WORD_VORON, "Pockets", __result, context);
                        break;
                    case "bosstagilla":
                        AddLootToInventory(__result.Inventory, "仿制工厂钥匙".ConvertHashID(), "Pockets", __result, context);
                        AddLootToInventory(__result.Inventory, ItemTpl.INFO_NOTE_WITH_CODE_WORD_ARK, "Pockets", __result, context);
                        break;
                    case "bosstagillaagro":
                        AddLootToInventory(__result.Inventory, ItemTpl.KEY_ARIADNE_SYMBOL, "Pockets", __result, context);
                        break;
                    case "bosssanitar":
                        AddLootToInventory(__result.Inventory, "疗养院管理员钥匙".ConvertHashID(), "Pockets", __result, context);
                        AddLootToInventory(__result.Inventory, ItemTpl.INFO_NOTE_WITH_CODE_WORD_HEARTBEAT, "Pockets", __result, context);
                        break;
                    case "bosskilla":
                        AddLootToInventory(__result.Inventory, "仿制11SR".ConvertHashID(), "Pockets", __result, context);
                        break;
                    case "bossgluhar":
                        AddLootToInventory(__result.Inventory, "储备站管理员钥匙".ConvertHashID(), "Pockets", __result, context);
                        AddLootToInventory(__result.Inventory, ItemTpl.INFO_MINEFIELD_MAP_RESERVE, "Pockets", __result, context);
                        break;
                    case "bosskojaniy":
                        {
                            AddLootToInventory(__result.Inventory, ItemTpl.INFO_MINEFIELD_MAP_WOODS, "Pockets", __result, context);
                        }
                        break;
                    case "bossboar":
                        {
                            AddLootToInventory(__result.Inventory, ItemTpl.INFO_NOTE_WITH_CODE_WORD_ONYX, "Pockets", __result, context);
                        }
                        break;
                    case "sectantpriest":
                        AddLootToInventory(__result.Inventory, "通用符号钥匙".ConvertHashID(), "Pockets", __result, context);
                        break;
                }
                if (botNickName == "Obsidian")
                {
                    AddLootToInventory(__result.Inventory, "实验室管理员钥匙卡".ConvertHashID(), "Pockets", __result, context);
                    AddLootToInventory(__result.Inventory, "黑L4G24夜视仪支架".ConvertHashID(), "Backpack", __result, context);
                    AddLootToInventory(__result.Inventory, "PVS31A".ConvertHashID(), "Backpack", __result, context);

                }
                if (alterBossName.Contains(botNickName))
                {
                    AddLootToInventory(__result.Inventory, "封装英雄之证".ConvertHashID(), "Pockets", __result, context);
                    if (__result.Inventory != null && __result.Inventory.Items != null && bot.Inventory.Items.Count > 0)
                    {
                        __result.Inventory.Items.ForEach(item =>
                        {
                            if (item.SlotId == "FirstPrimaryWeapon" || item.SlotId == "SecondPrimaryWeapon" || item.SlotId == "Holster")
                            {
                                if (item.Upd != null && item.Upd.Repairable != null)
                                {
                                    item.Upd.Repairable.Durability = 100;
                                    item.Upd.Repairable.MaxDurability = 100;
                                }
                            }
                        });
                    }
                }
                if (botNickName == "Obsidian" || botRoleLowercase == "arenafighterevent" || botRoleLowercase == "followerbully" || botRoleLowercase == "bossbully")
                {
                    if (__result.Inventory != null && __result.Inventory.Items != null && __result.Inventory.Items.Count > 0)
                    {
                        __result.Inventory.Items.ForEach(item =>
                        {
                            if (item.SlotId == "FirstPrimaryWeapon" || item.SlotId == "SecondPrimaryWeapon" || item.SlotId == "Holster")
                            {
                                if (item.Upd != null && item.Upd.Repairable != null)
                                {
                                    item.Upd.Repairable.Durability = 100;
                                    item.Upd.Repairable.MaxDurability = 100;
                                }
                            }
                        });
                    }
                }
            }
        }

        [Injectable]
        public class AddDogtagToBotPatch : AbstractPatch
        {

            private static TemplateTable _templateTable = default!;
            private static LocaleTable _localeTable = default!;
            private static GlobalTable _globalTable = default!;
            private static TradersTable _tradersTable = default!;
            private static HideoutTable _hideoutTable = default!;
            private static LocationTable _locationTable = default!;
            private static BotTable _botTable = default!;
            private static JsonUtil _jsonUtil = default!;
            private static ConfigServer _configServer = default!;
            private static ModHelper _modHelper = default!;
            private static ItemHelper _itemHelper = default!;
            private static LocaleService _localeService = default!;
            private static ICloner _cloner = default!;
            private static PresetHelper _presetHelper = default!;
            private static ImageRouter _imageRouter = default!;
            private static ECLogger _logger = default!;
            public AddDogtagToBotPatch(
            TemplateTable templateTable,
            LocaleTable localeTable,
            GlobalTable globalTable,
            TradersTable tradersTable,
            HideoutTable hideoutTable,
            LocationTable locationTable,
            BotTable botTable,
            JsonUtil jsonUtil,
            ConfigServer configServer,
            ModHelper modHelper,
            ItemHelper itemHelper,
            LocaleService localeService,
            ICloner cloner,
            PresetHelper presetHelper,
            ImageRouter imageRouter)
            {
                _templateTable = templateTable;
                _localeTable = localeTable;
                _globalTable = globalTable;
                _tradersTable = tradersTable;
                _hideoutTable = hideoutTable;
                _locationTable = locationTable;
                _botTable = botTable;
                _jsonUtil = jsonUtil;
                _configServer = configServer;
                _modHelper = modHelper;
                _itemHelper = itemHelper;
                _localeService = localeService;
                _cloner = cloner;
                _presetHelper = presetHelper;
                _imageRouter = imageRouter;
                _logger = new ECLogger("AddDogtagToBot", true);
            }
            protected override MethodBase GetTargetMethod()
            {
                return typeof(BotGenerator).GetMethod("AddDogtagToBot", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }

            [PatchFinalizer]
            public static Exception Finalizer(Exception __exception, BotBase bot)
            {
                // 如果有异常，且是我们预期的 APBS 抛出的 "Unknown side" 异常
                if (__exception is ArgumentException argEx && argEx.Message.Contains("Unknown side"))
                {
                    // 1. 吃掉异常，拯救生成线程
                    __exception = null;

                    GenerateCustomDogtag(bot);
                }

                return __exception;
            }

            [PatchPrefix]
            public static bool Prefix(BotGenerator __instance, BotBase bot)
            {
                if (bot.Info.Side != "Savage")
                {
                    return true;
                }

                GenerateCustomDogtag(bot);
                return false;
            }
        }

        private static void GenerateCustomDogtag(BotBase bot)
        {
            var bossList = new List<string>
        {
            "bosstagilla", "bosstagillaagro", "bossbully", "bossboar",
            "bossgluhar", "bosssanitar", "bosskilla", "bosskillaagro",
            "bosskojaniy", "bosszryachiy", "bosskolontay", "bossknight",
            "followerbigpipe", "followerbirdeye", "bosspartisan",
            "bosswedgelab"
        };

            var botRole = bot.Info.Settings.Role;
            var botRoleLower = botRole.ToLower();
            var botNickName = bot.Info.Nickname;
            var botName = "Nikita";

            Item dogTagItem = new Item
            {
                Id = new MongoId(),
                Template = "Scav狗牌".ConvertHashID()
            };

            try
            {
                MongoId? equipment = bot.Inventory.Equipment;
                dogTagItem.ParentId = (equipment.HasValue ? ((string)equipment.GetValueOrDefault()) : null);
                dogTagItem.SlotId = "Dogtag";
                dogTagItem.Upd = new Upd
                {
                    SpawnedInSession = true,
                    Dogtag = new UpdDogtag()
                };

                var dogtag = "Scav狗牌".ConvertHashID();

                if (bossList.Contains(botRoleLower))
                {
                    switch (botRoleLower)
                    {
                        case "bossbully": botName = "Reshala"; break;
                        case "bossboar": botName = "Kaban"; break;
                        case "bosskojaniy": botName = "Shturman"; break;
                        case "bossknight":
                        case "followerbigpipe":
                        case "followerbirdeye":
                        case "bosswedgelab":
                            // bossWedgeLab 的代号来自名字池（池里只有 "Wedge"），
                            // 保留本体名字，不要按角色名派生
                            botName = bot.Info.Nickname; break;
                        case "bossgluhar":
                            botName = bot.Info.Nickname == "Ghroth" ? bot.Info.Nickname : botRole.Substring(4); break;
                        case "bosskolontay":
                            botName = bot.Info.Nickname == "Punisher" ? bot.Info.Nickname : botRole.Substring(4); break;
                        case "bosskillaagro":
                            botName = bot.Info.Nickname == "Obsidian" ? bot.Info.Nickname : "Killa"; break;
                        case "bosstagillaagro":
                            botName = "Tagilla"; break; //未来会有守门人
                        default:
                            botName = botRole.Substring(4); break;
                    }
                    dogtag = "Boss狗牌".ConvertHashID();
                    dogTagItem.Upd.Dogtag.Nickname = botName;
                    dogTagItem.Upd.Dogtag.Side = SPTarkov.Server.Core.Models.Enums.DogtagSide.Bear;
                    dogTagItem.Upd.Dogtag.Level = 100;
                }
                else
                {
                    // 这里调用你的俄罗斯语转拉丁语的方法
                    botName = RussianToLatinApproximation(botNickName);
                    dogTagItem.Upd.Dogtag.Nickname = botName;
                    dogTagItem.Upd.Dogtag.Side = SPTarkov.Server.Core.Models.Enums.DogtagSide.Bear;
                    dogTagItem.Upd.Dogtag.Level = 80;
                }

                // 特殊名字判断
                if (alterBossName.Contains(botNickName))
                {
                    dogtag = "水晶狗牌_红".ConvertHashID();
                    dogTagItem.Upd.Dogtag.Nickname = botName;
                    dogTagItem.Upd.Dogtag.Side = SPTarkov.Server.Core.Models.Enums.DogtagSide.Bear;
                    dogTagItem.Upd.Dogtag.Level = 200;
                }

                // 邪教徒判断
                if (botRoleLower.Contains("sectant"))
                {
                    dogtag = "邪教徒狗牌".ConvertHashID();
                    dogTagItem.Upd.Dogtag.Nickname = botName;
                    dogTagItem.Upd.Dogtag.Side = SPTarkov.Server.Core.Models.Enums.DogtagSide.Usec;
                    dogTagItem.Upd.Dogtag.Level = 99;
                }

                dogTagItem.Template = dogtag;
                bot.Inventory.Items.Add(dogTagItem);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        public static string RussianToLatinApproximation(string russianString)
        {
            // 俄文字母到拉丁字母的映射表
            var russianToLatinMap = new Dictionary<char, string>
                {
                    {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
                    {'е', "e"}, {'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
                    {'й', "j"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
                    {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
                    {'у', "u"}, {'ф', "f"}, {'х', "h"}, {'ц', "c"}, {'ч', "ch"},
                    {'ш', "sh"}, {'щ', "sch"}, {'ъ', "j"}, {'ы', "i"}, {'ь', "j"},
                    {'э', "e"}, {'ю', "yu"}, {'я', "ya"},
                    {'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"},
                    {'Е', "E"}, {'Ё', "Yo"}, {'Ж', "Zh"}, {'З', "Z"}, {'И', "I"},
                    {'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"}, {'Н', "N"},
                    {'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"},
                    {'У', "U"}, {'Ф', "F"}, {'Х', "Kh"}, {'Ц', "Ts"}, {'Ч', "Ch"},
                    {'Ш', "Sh"}, {'Щ', "Sch"}, {'Э', "E"}, {'Ю', "Yu"}, {'Я', "Ya"}
                };

            // 如果传入字符串为空
            if (string.IsNullOrEmpty(russianString))
            {
                return "不知道发生了什么, 这个AI没有名字, 也许是尼基塔死了妈妈";
            }

            var latinString = new StringBuilder();

            foreach (var ch in russianString)
            {
                // 如果字符在映射表中，进行替换
                if (russianToLatinMap.ContainsKey(ch))
                {
                    latinString.Append(russianToLatinMap[ch]);
                }
                else
                {
                    // 如果字符不在映射表中，直接添加
                    latinString.Append(ch);
                }
            }

            return latinString.ToString();
        }
        public static void AddLootToInventory(BotBaseInventory inventory, MongoId itemid, string slot, BotBase bot, ContextManager.LoadModContext context)
        {
            //这个方法默认只支持1x1战利品, 因此不再做旋转检测
            var container = inventory.Items.FirstOrDefault(x => x.SlotId == slot);
            var haveitem = inventory.Items.FirstOrDefault(x => x.Template == itemid);
            var containerMap = new Dictionary<string, int[,]>();
            if (container == null || haveitem != null)
            {
                //logger.LogWithColor($"警告: 无法在{bot.Info.Settings.Role}身上找到{slot}, 或者{itemid}已经存在", LogTextColor.Yellow);
                return;
            }
            var containerItem = ItemUtils.GetItem(container.Template, context);
            if (containerItem != null && containerItem.Properties != null && containerItem.Properties.Grids != null)
            {
                foreach (var grids in containerItem.Properties.Grids)
                {
                    if (grids.Properties == null || grids == null) continue;
                    containerMap.TryAdd(grids.Name, new int[(int)grids.Properties.CellsV, (int)grids.Properties.CellsH]);
                }
            }
            var itemList = inventory.Items.Where(x => x.ParentId != null && x.ParentId == container.Id).ToList();
            foreach (var item in itemList)
            {
                var itemTemplate = ItemUtils.GetItem(item.Template, context);
                if (itemTemplate == null) continue;
                //出门
                //防止思路断掉, 打个备注
                //接下来取物品大小覆盖到map上完成最终成果
                //回来了, 继续
                if (item.Location == null) continue;
                var location = (ItemLocation)item.Location;
                containerMap.TryGetValue(item.SlotId, out var map);
                if (map == null) continue;
                var itemwidth = location.R == ItemRotation.Horizontal ? itemTemplate.Properties.Width : itemTemplate.Properties.Height;
                var itemheight = location.R == ItemRotation.Horizontal ? itemTemplate.Properties.Height : itemTemplate.Properties.Width;
                var itemlocationx = (int)location.X;
                var itemlocationy = (int)location.Y;
                for (int x = itemlocationy; x < itemlocationy + itemheight; x++)
                {
                    for (int y = itemlocationx; y < itemlocationx + itemwidth; y++)
                    {
                        // 确保不会超出 map 的边界
                        if (x >= 0 && x < map.GetLength(0) && y >= 0 && y < map.GetLength(1))
                        {
                            map[x, y] = 1; // 标记为已占用
                        }
                    }
                }
            }
            var targetlocation = FindPlacementSpace(containerMap);
            if (targetlocation == null)
            {
                //logger.LogWithColor($"警告: 无法在{bot.Info.Settings.Role}的{slot}上找到有效空间", LogTextColor.Yellow);
                return;
            }
            //logger.LogWithColor($"成功在{bot.Info.Settings.Role}的{slot}添加了{itemid}", LogTextColor.Yellow);
            inventory.Items.Add(new Item
            {
                Id = new MongoId(),
                Template = itemid,
                ParentId = container.Id,
                SlotId = targetlocation.Value.Key,
                Upd = new Upd
                {
                    SpawnedInSession = true,
                    StackObjectsCount = 1
                },
                Location = targetlocation.Value.Value
            });
        }
        public static KeyValuePair<string, ItemLocation>? FindPlacementSpace(Dictionary<string, int[,]> containerMap)
        {
            foreach (var container in containerMap)
            {
                var map = container.Value;
                for (int x = 0; x < map.GetLength(0); x++)
                {
                    for (int y = 0; y < map.GetLength(1); y++)
                    {
                        if (map[x, y] == 0)
                        {
                            //logger.LogWithColor($"坐标查找成功", LogTextColor.Green);
                            return new KeyValuePair<string, ItemLocation>(container.Key, new ItemLocation
                            {
                                X = y,
                                Y = x,
                                R = ItemRotation.Horizontal  // 默认横向放置，物品是 1x1 所以旋转无意义
                            });
                        }
                    }
                }
            }
            //logger.LogWithColor($"警告: 空间已满", LogTextColor.Yellow);
            // 如果没有找到空格子，则返回 null
            return null;
        }
        /// <summary>
        /// 异格判定器 —— 每局只摇一次，当局内同组角色共用同一个结论。
        ///
        /// 为什么不沿用「每 5 次调用复位」的老写法（踩坑记录）：
        ///
        ///  ① SPT 的 bot 战利品池是按「角色」缓存的（BotLootCacheService）：
        ///     每局只建一次，由该角色这一局**第一个**跑到「生成战利品」那一步的 bot 来建，
        ///     且 EndLocalRaid 才 ClearCache。所以同一局里这个角色必须只有一种模板，
        ///     否则池子会和模板错配 —— 错配时异格抽到的是原版的池子。
        ///     原版 bosswedgelab 的 inventory.items.Backpack 是空对象（且 Backpack 装备几率= 0，
        ///     本尊根本不背背包），而黑曜石的身家全在背包池 → 一旦错配，黑曜石背包全空。
        ///
        ///  ② BotController.GenerateBotWave 用 .AsParallel() 并发生成，而客户端是
        ///     **一波一个 /client/game/bot/generate 请求**（日志实测同一局里有
        ///     08:54:10 与 08:54:13 两个 bossWedgeLab 请求）。旧的 no-lock static
        ///     Chance / Counter / Access 既会丢计数，又会让复位点落在波中间
        ///     → 同一局里一半异格一半原版 → 战利品缓存被原版抢先。
        ///
        ///  ③ 现改为：在 LocationLifecycleService.StartLocalRaidAsync 之前复位（即每局一次），
        ///     之后「首次判定即定局」，当局内只读、不再累加。
        /// </summary>
        public static class AlterBossDecision
        {
            private static readonly object Gate = new object();

            /// <summary>本局已定的结论：key（角色或角色组）→ 是否异格</summary>
            private static readonly Dictionary<string, bool> Decided = new Dictionary<string, bool>();

            /// <summary>本局摇出的点数，仅用于排查</summary>
            private static readonly Dictionary<string, int> Rolls = new Dictionary<string, int>();

            /// <summary>每局开局复位（挂在 LocationLifecycleService.StartLocalRaidAsync 的前缀上）</summary>
            public static void ResetForNewRaid()
            {
                lock (Gate)
                {
                    Decided.Clear();
                    Rolls.Clear();
                }
            }

            /// <summary>
            /// 取本局该组的异格结论；本局第一次被问到时摇一次，之后固定不变。
            /// </summary>
            /// <param name="groupKey">角色或角色组（Goons 三只共用同一个 key，保证小队同进同退）</param>
            /// <param name="chancePercent">命中概率（0-100）</param>
            /// <param name="roll">本局摇出的点数</param>
            public static bool IsAltered(string groupKey, int chancePercent, out int roll)
            {
                lock (Gate)
                {
                    if (!Decided.TryGetValue(groupKey, out bool hit))
                    {
                        roll = (int)Math.Floor(new Random().NextDouble() * 100);
                        hit = roll <= chancePercent;
                        Decided[groupKey] = hit;
                        Rolls[groupKey] = roll;
                    }
                    else
                    {
                        roll = Rolls[groupKey];
                    }

                    return hit;
                }
            }
        }

        /// <summary>
        /// 每局开局复位异格判定。
        /// 战利品池缓存（BotLootCacheService）的寿命是「按角色 + 每局一次」，在 EndLocalRaid 清；
        /// 本复位的时机（StartLocalRaid）与它的生命周期对齐 —— 保证一局内判定只有一份。
        /// </summary>
        [Injectable]
        public class ResetAlterBossDecisionPatch : AbstractPatch
        {
            private static readonly MethodBase Target =
                typeof(LocationLifecycleService).GetMethod(
                    "StartLocalRaidAsync",
                    BindingFlags.Public | BindingFlags.Instance);

            protected override MethodBase GetTargetMethod()
            {
                return Target!;
            }

            [PatchPrefix]
            public static void Prefix()
            {
                AlterBossDecision.ResetForNewRaid();
            }
        }
    }
}