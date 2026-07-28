using EternalCycleServer;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Constants;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Generators;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Bot;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using System;
using System.Reflection;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace VulcanInfinity
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
                "Punisher"
                };
        public class GenerateBotPatch : AbstractPatch
        {
            public static AlterBossCounter SanitarCounter = new AlterBossCounter
            {
                Chance = 0,
                Counter = 0,
                Access = false
            };
            public static AlterBossCounter GluharCocunter = new AlterBossCounter
            {
                Chance = 0,
                Counter = 0,
                Access = false
            };
            public static AlterBossCounter KolontayCocunter = new AlterBossCounter
            {
                Chance = 0,
                Counter = 0,
                Access = false
            };
            public static AlterGoonsCounter GoonsCounter = new AlterGoonsCounter
            {
                Chance = 0,
                KnightCounter = 0,
                PipeCounter = 0,
                EyesCounter = 0,
                KnightAccess = false,
                PipeAccess = false,
                EyesAccess = false,
                Access = false
            };
            public static string BotLoation = "";
            protected override MethodBase GetTargetMethod()
            {
                return typeof(BotGenerator).GetMethod("GenerateBot", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            }
            [PatchPrefix]
            public static bool Prefix(BotGenerator __instance, MongoId sessionId, BotBase bot, BotType botJsonTemplate, BotGenerationDetails botGenerationDetails, ref BotBase __result)
            {

                var logger = ServiceLocator.ServiceProvider.GetService<ISptLogger<BotGenerator>>();
                var botLevelGenerator = ServiceLocator.ServiceProvider.GetService<BotLevelGenerator>();
                var botEquipmentFilterService = ServiceLocator.ServiceProvider.GetService<BotEquipmentFilterService>();
                var botNameService = ServiceLocator.ServiceProvider.GetService<BotNameService>();
                var databaseService = ServiceLocator.ServiceProvider.GetService<DatabaseService>();
                var seasonalEventService = ServiceLocator.ServiceProvider.GetService<SeasonalEventService>();
                var weightedRandomHelper = ServiceLocator.ServiceProvider.GetService<WeightedRandomHelper>();
                var botInventoryGenerator = ServiceLocator.ServiceProvider.GetService<BotInventoryGenerator>();
                var modHelper = ServiceLocator.ServiceProvider.GetService<ModHelper>();
                var configServer = ServiceLocator.ServiceProvider.GetService<ConfigServer>();
                var botConfig = configServer.GetConfig<BotConfig>();

                var botRoleLowercase = botGenerationDetails.Role.ToLowerInvariant();
                var modpath = System.IO.Path.Combine(ConfigManager.modPath, $"{ConfigManager.dataPath}");
                var botSanitar = modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Sanitar.json");
                var botKnight = modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Knight.json");
                var botBigPipe = modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/BigPipe.json");
                var botBirdeye = modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Birdeye.json");
                var botGluhar = modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Gluhar.json");
                var botKolontay = modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Kolontay.json");
                var botObsidian = modHelper.GetJsonDataFromFile<BotType>(modpath, "bots/Obsidian.json");
                try
                {
                    if (!botConfig.BotRolesWithDogTags.Contains(botRoleLowercase))
                    {
                        botConfig.BotRolesWithDogTags.Add(botRoleLowercase);
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
                            if (GoonsCounter.Chance == 0)
                            {
                                GoonsCounter.Chance = (int)Math.Floor(new Random().NextDouble() * 100);
                            }
                            if (GoonsCounter.Chance <= modConfig.AlterBossChance.Goons)
                            {
                                GoonsCounter.Access = true;
                            }
                            if (GoonsCounter.Access)
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
                                GoonsCounter.KnightAccess = true;
                            }
                            GoonsCounter.KnightCounter++;
                            if (GoonsCounter.KnightCounter >= 5 && GoonsCounter.KnightAccess != true)
                            {
                                logger.LogWithColor("转化失败复位", LogTextColor.Gray);
                                //转化失败复位
                                GoonsCounter.Chance = 0;
                                GoonsCounter.KnightCounter = 0;
                                GoonsCounter.PipeCounter = 0;
                                GoonsCounter.EyesCounter = 0;
                                GoonsCounter.KnightAccess = false;
                                GoonsCounter.PipeAccess = false;
                                GoonsCounter.EyesAccess = false;
                                GoonsCounter.Access = false;
                            }
                        }
                        if (botRoleLowercase == "followerbigpipe")
                        {
                            if (GoonsCounter.Access)
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
                                GoonsCounter.PipeAccess = true;
                                //唱片预留
                            }
                            GoonsCounter.PipeCounter++;
                            if (GoonsCounter.PipeCounter >= 5 && GoonsCounter.EyesCounter >= 5)
                            {
                                logger.LogWithColor("转化完成复位, from bigpipe", LogTextColor.Gray);
                                //转化完成复位
                                //大管和鸟眼双计数, 防止提前复位
                                GoonsCounter.Chance = 0;
                                GoonsCounter.KnightCounter = 0;
                                GoonsCounter.PipeCounter = 0;
                                GoonsCounter.EyesCounter = 0;
                                GoonsCounter.KnightAccess = false;
                                GoonsCounter.PipeAccess = false;
                                GoonsCounter.EyesAccess = false;
                                GoonsCounter.Access = false;
                            }
                        }
                        if (botRoleLowercase == "followerbirdeye")
                        {
                            if (GoonsCounter.Access)
                            {
                                botJsonTemplate.BotAppearance = botBirdeye.BotAppearance;
                                botJsonTemplate.BotExperience = botBirdeye.BotExperience;
                                botJsonTemplate.BotHealth = botBirdeye.BotHealth;
                                botJsonTemplate.BotSkills = botBirdeye.BotSkills;
                                botJsonTemplate.BotInventory = botBirdeye.BotInventory;
                                botJsonTemplate.BotChances = botBirdeye.BotChances;
                                botJsonTemplate.FirstNames = botBirdeye.FirstNames;
                                botJsonTemplate.BotGeneration = botBirdeye.BotGeneration;
                                GoonsCounter.EyesAccess = true;
                                //唱片预留
                            }
                            GoonsCounter.EyesCounter++;
                            if (GoonsCounter.PipeCounter >= 5 && GoonsCounter.EyesCounter >= 5)
                            {
                                logger.LogWithColor("转化完成复位, from birdeye", LogTextColor.Gray);
                                //转化完成复位
                                //大管和鸟眼双计数, 防止提前复位
                                GoonsCounter.Chance = 0;
                                GoonsCounter.KnightCounter = 0;
                                GoonsCounter.PipeCounter = 0;
                                GoonsCounter.EyesCounter = 0;
                                GoonsCounter.KnightAccess = false;
                                GoonsCounter.PipeAccess = false;
                                GoonsCounter.EyesAccess = false;
                                GoonsCounter.Access = false;
                            }
                        }
                        if (botRoleLowercase == "bosssanitar")
                        {
                            //从文件读取AI数据
                            if (SanitarCounter.Chance == 0)
                            {
                                //初始化生成概率
                                SanitarCounter.Chance = (int)Math.Floor(new Random().NextDouble() * 100);
                            }
                            if (SanitarCounter.Chance <= modConfig.AlterBossChance.Sanitar)
                            {
                                //命中概率
                                //这里的100是测试, 实际从配置读取
                                SanitarCounter.Access = true;
                            }
                            if (SanitarCounter.Access)
                            {
                                //概率命中, 使用文件的数据覆盖原数据
                                botJsonTemplate.BotAppearance = botSanitar.BotAppearance;
                                botJsonTemplate.BotExperience = botSanitar.BotExperience;
                                botJsonTemplate.BotHealth = botSanitar.BotHealth;
                                botJsonTemplate.BotSkills = botSanitar.BotSkills;
                                botJsonTemplate.BotInventory = botSanitar.BotInventory;
                                botJsonTemplate.BotChances = botSanitar.BotChances;
                                botJsonTemplate.FirstNames = botSanitar.FirstNames;
                                botJsonTemplate.BotGeneration = botSanitar.BotGeneration;
                                //配置器计数+1
                            }
                            SanitarCounter.Counter++;
                            //AI生成完成(每个AI的生成会调用5次
                            if (SanitarCounter.Counter >= 5)
                            {
                                //配置器复位
                                SanitarCounter.Chance = 0;
                                SanitarCounter.Counter = 0;
                                SanitarCounter.Access = false;
                            }
                        }
                        if (botRoleLowercase == "bossgluhar")
                        {
                            //从文件读取AI数据
                            if (GluharCocunter.Chance == 0)
                            {
                                //初始化生成概率
                                GluharCocunter.Chance = (int)Math.Floor(new Random().NextDouble() * 100);
                            }
                            if (GluharCocunter.Chance <= modConfig.AlterBossChance.Gluhar)
                            {
                                //命中概率
                                //这里的100是测试, 实际从配置读取
                                GluharCocunter.Access = true;
                            }
                            if (GluharCocunter.Access)
                            {
                                //概率命中, 使用文件的数据覆盖原数据
                                botJsonTemplate.BotAppearance = botGluhar.BotAppearance;
                                botJsonTemplate.BotExperience = botGluhar.BotExperience;
                                botJsonTemplate.BotHealth = botGluhar.BotHealth;
                                botJsonTemplate.BotSkills = botGluhar.BotSkills;
                                botJsonTemplate.BotInventory = botGluhar.BotInventory;
                                botJsonTemplate.BotChances = botGluhar.BotChances;
                                botJsonTemplate.FirstNames = botGluhar.FirstNames;
                                botJsonTemplate.BotGeneration = botGluhar.BotGeneration;
                                //配置器计数+1
                            }
                            GluharCocunter.Counter++;
                            //AI生成完成(每个AI的生成会调用5次
                            if (GluharCocunter.Counter >= 5)
                            {
                                //配置器复位
                                GluharCocunter.Chance = 0;
                                GluharCocunter.Counter = 0;
                                GluharCocunter.Access = false;
                            }
                        }
                        if (botRoleLowercase == "bosskolontay")
                        {
                            //从文件读取AI数据
                            if (KolontayCocunter.Chance == 0)
                            {
                                //初始化生成概率
                                KolontayCocunter.Chance = (int)Math.Floor(new Random().NextDouble() * 100);
                            }
                            if (KolontayCocunter.Chance <= modConfig.AlterBossChance.Kolontay)
                            {
                                //命中概率
                                //这里的100是测试, 实际从配置读取
                                KolontayCocunter.Access = true;
                            }
                            if (KolontayCocunter.Access)
                            {
                                //概率命中, 使用文件的数据覆盖原数据
                                botJsonTemplate.BotAppearance = botKolontay.BotAppearance;
                                botJsonTemplate.BotExperience = botKolontay.BotExperience;
                                botJsonTemplate.BotHealth = botKolontay.BotHealth;
                                botJsonTemplate.BotSkills = botKolontay.BotSkills;
                                botJsonTemplate.BotInventory = botKolontay.BotInventory;
                                botJsonTemplate.BotChances = botKolontay.BotChances;
                                botJsonTemplate.FirstNames = botKolontay.FirstNames;
                                botJsonTemplate.BotGeneration = botKolontay.BotGeneration;
                                //配置器计数+1
                            }
                            KolontayCocunter.Counter++;
                            //AI生成完成(每个AI的生成会调用5次
                            if (KolontayCocunter.Counter >= 5)
                            {
                                //配置器复位
                                KolontayCocunter.Chance = 0;
                                KolontayCocunter.Counter = 0;
                                KolontayCocunter.Access = false;
                            }
                        }
                    }
                    if (modConfig.EnableBlackDivision)
                    {
                        if (botRoleLowercase == "bosskillaagro" && botGenerationDetails.Location != "Labyrinth")
                        {
                            //测试代码, 数据部分暂时搁置
                            //打个时间戳看看我能鸽多久
                            //12.10.2025
                            //从文件读取AI数据
                            botJsonTemplate.BotAppearance = botObsidian.BotAppearance;
                            botJsonTemplate.BotExperience = botObsidian.BotExperience;
                            botJsonTemplate.BotHealth = botObsidian.BotHealth;
                            botJsonTemplate.BotSkills = botObsidian.BotSkills;
                            botJsonTemplate.BotInventory = botObsidian.BotInventory;
                            botJsonTemplate.BotChances = botObsidian.BotChances;
                            botJsonTemplate.FirstNames = botObsidian.FirstNames;
                            botJsonTemplate.LastNames = botObsidian.LastNames;
                            botJsonTemplate.BotGeneration = botObsidian.BotGeneration;
                            //bot.Health.BodyParts;
                            //3c6269d6143b2cf96fb0224f //BD1
                            //3c6269d6143b2cf96fb0224f //BD2
                            //logger.LogWithColor($"Debug: {bot.Customization.Voice}", LogTextColor.Gray);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWithColor(ex.StackTrace, LogTextColor.Yellow, LogBackgroundColor.Default);
                }

                return true;
            }

            [PatchPostfix]
            public static void Postfix(BotGenerator __instance, MongoId sessionId, BotBase bot, BotType botJsonTemplate, BotGenerationDetails botGenerationDetails, ref BotBase __result)
            {
                var logger = ServiceLocator.ServiceProvider.GetService<ISptLogger<BotGenerator>>();
                var botLevelGenerator = ServiceLocator.ServiceProvider.GetService<BotLevelGenerator>();
                var botEquipmentFilterService = ServiceLocator.ServiceProvider.GetService<BotEquipmentFilterService>();
                var botNameService = ServiceLocator.ServiceProvider.GetService<BotNameService>();
                var databaseService = ServiceLocator.ServiceProvider.GetService<DatabaseService>();
                var seasonalEventService = ServiceLocator.ServiceProvider.GetService<SeasonalEventService>();
                var weightedRandomHelper = ServiceLocator.ServiceProvider.GetService<WeightedRandomHelper>();
                var botInventoryGenerator = ServiceLocator.ServiceProvider.GetService<BotInventoryGenerator>();
                var presetHelper = ServiceLocator.ServiceProvider.GetService<PresetHelper>();
                var jsonUtil = ServiceLocator.ServiceProvider.GetService<JsonUtil>();
                var imageRouter = ServiceLocator.ServiceProvider.GetService<ImageRouter>();
                var itemHelper = ServiceLocator.ServiceProvider.GetService<ItemHelper>();
                var cloner = ServiceLocator.ServiceProvider.GetService<ICloner>();
                var modHelper = ServiceLocator.ServiceProvider.GetService<ModHelper>();
                var configServer = ServiceLocator.ServiceProvider.GetService<ConfigServer>();
                var botConfig = configServer.GetConfig<BotConfig>();

                var context = new ContextManager.LoadModContext
                {
                    DB = databaseService,
                    JsonUtil = jsonUtil,
                    ConfigServer = configServer,
                    ModHelper = modHelper,
                    Logger = Utils.commonLogger,
                    ImageRouter = imageRouter,
                    PresetHelper = presetHelper,
                    ItemHelper = itemHelper,
                    Cloner = cloner
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
                if (botNickName == "Obsidian" || botRoleLowercase == "arenafighterevent")
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
        public class AddDogtagToBotPatch : AbstractPatch
        {
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
            "followerbigpipe", "followerbirdeye", "bosspartisan"
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
            var logger = ServiceLocator.ServiceProvider.GetService<ISptLogger<BotGenerator>>();
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
            var targetlocation = FindPlacementSpace(containerMap, logger);
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
        public static KeyValuePair<string, ItemLocation>? FindPlacementSpace(Dictionary<string, int[,]> containerMap, ISptLogger<BotGenerator> logger)
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
        public class AlterGoonsCounter
        {
            public int Chance { get; set; }
            public int KnightCounter { get; set; }
            public int PipeCounter { get; set; }
            public int EyesCounter { get; set; }
            public bool Access { get; set; }
            public bool KnightAccess { get; set; }
            public bool PipeAccess { get; set; }
            public bool EyesAccess { get; set; }
        }
        public class AlterBossCounter
        {
            public int Chance { get; set; }
            public int Counter { get; set; }
            public bool Access { get; set; }
        }
    }
}