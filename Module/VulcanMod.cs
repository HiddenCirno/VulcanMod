using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using EternalCycleServer;
using SPTarkov.Server.Core.Loaders;
namespace VulcanInfinity
{
    public class VulcanMod
    {
        public static VulcanModConfigClass modConfig = ConfigManager.GetConfig();
        public static void Init(
            DatabaseService databaseService,
            CustomItemService customItemService,
            ModHelper modHelper,
            JsonUtil jsonutil,
            ICloner cloner,
            ConfigServer configServer,
            ImageRouter imageRouter
            )
        {
            var preSptModLoader = ServiceLocator.ServiceProvider.GetService<PreSptModLoader>();
            var globals = databaseService.GetGlobals();
            var items = databaseService.GetItems();
            var prices = databaseService.GetPrices();
            var zhCNLang = databaseService.GetLocales().Global["ch"];
            globals.Configuration.ItemsCommonSettings.MaxBackpackInserting = 99999999;
            InitVanillaItemEdit(databaseService);
            RemoveBlackAltynLockedCondition(databaseService);
            InitModBaseData(databaseService, customItemService, modHelper, jsonutil, cloner, configServer, imageRouter);
            //var botConfig = configServer.GetConfig<BotConfig>();
            //botConfig.BotRolesWithDogTags.Add("assault");
            new BotGeneratorPatch.GenerateBotPatch().Enable();
            new BotGeneratorPatch.AddDogtagToBotPatch().Enable();
            if (modConfig.EnableReshalaEdit)
            {
                InitReshalaEdit(modConfig, databaseService, modHelper);
            }
            if (modConfig.EnableBlackDivision)
            {
                InitBDReplace(databaseService, modHelper, cloner);
            }
            InitBotEdit(databaseService, modHelper);
                InitHideoutAreaEdit(databaseService);
            if (modConfig.EnableRecipeEdit)
            {
                InitHideoutRecipeEdit(databaseService);
            }
                items[ItemTpl.CONTAINER_STREAMER_ITEM_CASE].Properties.Grids.First().Properties.CellsH = 16;
                items[ItemTpl.CONTAINER_STREAMER_ITEM_CASE].Properties.Grids.First().Properties.CellsV = 16;

            if (modConfig.EnableKabanInShoreline)
            {
                AddKabanToShoreline(modConfig, databaseService);
            }
            ForcedUnlockEventQuest(databaseService, configServer);

        }
        public static void InitModBaseData(
            DatabaseService databaseService,
            CustomItemService customItemService,
            ModHelper modHelper,
            JsonUtil jsonUtil,
            ICloner cloner,
            ConfigServer configServer,
            ImageRouter imageRouter
            )
        {

            var itemHelper = ServiceLocator.ServiceProvider.GetService<ItemHelper>();
            var presetHelper = ServiceLocator.ServiceProvider.GetService<PresetHelper>();
            var logger = new ECLogger("PostRagfairLoadEvent", true);
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

            var creator = "<color=#55FFFF>火神重工</color>";
            var modName = creator;
            var modpath = System.IO.Path.Combine(ConfigManager.modPath, $"{ConfigManager.dataPath}");
            var imagepath = System.IO.Path.Combine(modpath, "res/");
            var iconpath = System.IO.Path.Combine(imagepath, "icon/");
            var questimagepath = System.IO.Path.Combine(imagepath, "questimage/");
            //var items = modHelper.GetJsonDataFromFile<Dictionary<string, CustomItemTemplate>>(pathToMod, "vulcanmod/newitem.json");
            ItemUtils.RegisterItem(modpath, "items_normal.json", creator, modName);
            ItemUtils.RegisterItem(modpath, "items_ammochest.json", creator, modName);
            ItemUtils.RegisterItem(modpath, "items_skillchest.json", creator, modName);
            ItemUtils.RegisterDrawPool(modpath, "newdrawpool.json");

            TraderUtils.RegisterTrader(modpath, "trader/base.json", "res/avatar/", creator, modName);
            AssortUtils.RegisterAssort(modpath, "traderdata/assort_mod.json");
            AssortUtils.RegisterAssort(modpath, "traderdata/assort_vanilla.json");
            AssortUtils.RegisterAssort(modpath, "traderdata/assort_ammochest.json");
            AssortUtils.RegisterAssort(modpath, "traderdata/assort_skillchest.json");

            QuestUtils.RegisterQuest(modpath, "traderdata/quest/init.json", "res/questimage/");
            QuestUtils.RegisterQuest(modpath, "traderdata/quest/init_event.json", "res/questimage/");
            QuestUtils.RegisterQuestLogicTree(modpath, "traderdata/quest/logic.json");
            QuestUtils.RegisterQuestLogicTree(modpath, "traderdata/quest/logic_event.json");
            QuestUtils.RegisterQuestRewards(modpath, "traderdata/quest/rewards_vanilla.json");
            QuestUtils.RegisterQuestRewards(modpath, "traderdata/quest/achievement_rewards_vanilla.json");
            AchievementUtils.RegisterAchievement(modpath, "traderdata/quest/achievement.json", "res/icon/");

            RecipeUtils.RegisterRecipe(modpath, "hideout/recipe.json");
            RecipeUtils.RegisterScavCaseRecipe(modpath, "hideout/scavcase.json");
            RecipeUtils.RegisterCultistCircleRecipe(modpath, "hideout/circle.json");

            PresetUtils.RegisterPreset(modpath, "preset.json");
            SuitUtils.RegisterSuit(modpath, "suits.json");
            CustomizationUtils.RegisterCustomization(modpath, "custom.json", "res/deco/");
            CustomizationUtils.RegisterHideoutCustomization(modpath, "hideout/custom.json");

            //ResourceUtils.RegisterDecoIconResource(modpath, "res/deco/");
            ResourceUtils.RegisterRigLayoutResource(modpath, "res/layout/");

            LocaleUtils.RegisterQuestLocale(modpath, "locales/quest/", creator, modName);

            LocaleUtils.RegisterLocaleText(modpath, "locales/text/");
            EventManager.DataLoadEvent.LoadItemEvent += (loadContext) =>
            {
                try
                {
                    ItemUtils.GetItem("盒装闪光子弹".ConvertHashID(), loadContext).Properties.StackSlots.First().MaxCount = 10;

                    var pocketsjaney = ItemUtils.GetItem("1x2x4口袋".ConvertHashID(), loadContext).Properties.Grids;
                    foreach (var grid in pocketsjaney)
                    {
                        grid.Properties.CellsV = 2;
                    }
                }
                catch (Exception ex)
                {
                }
            }; 
            EventManager.DataLoadEvent.LoadQuestDataEvent += (loadContext) =>
            {
                try
                {
                    InitOracleQuestData();
                }
                catch (Exception ex)
                {
                }
            }; 
            EventManager.DataLoadEvent.LoadItemEvent += (loadContext) =>
            {
                try
                {
                    var kappa = QuestUtils.GetQuest(QuestTpl.COLLECTOR, loadContext);
                    var conditions = kappa.Conditions.AvailableForFinish;
                    if (!conditions.Any(x =>(x.Target.IsList && x.Target.List.Contains("6937ecf8628ee476240c07cb")) || (x.Target.IsItem && x.Target.Item == "6937ecf8628ee476240c07cb")))
                    {
                        var twitchcase = ItemUtils.GetItem(ItemTpl.CONTAINER_STREAMER_ITEM_CASE, loadContext);
                        var itemid = Utils.ConvertHashID("黄金哑铃");
                        var questid = $"Kappa_黄金哑铃".ConvertHashID();
                        var itemid2 = Utils.ConvertHashID("Tigz的骨折环");
                        var questid2 = $"Kappa_Tigz的骨折环".ConvertHashID();
                        EventManager.DataLoadEvent.LoadQuestDataEvent += (eventContext) =>
                        {
                            try
                            {
                                QuestUtils.InitHandoverItemDataConditions(conditions, new HandoverItemData
                                {
                                    Id = questid,
                                    FindInRaid = true,
                                    ItemId = itemid,
                                    Count = 1,
                                    AutoLocale = true
                                },
                                eventContext); 
                                QuestUtils.InitHandoverItemDataConditions(conditions, new HandoverItemData
                                {
                                    Id = questid2,
                                    FindInRaid = true,
                                    ItemId = itemid2,
                                    Count = 1,
                                    AutoLocale = true
                                },
                                eventContext);
                                var twitchcasecontainer = twitchcase.Properties.Grids.First().Properties.Filters.First().Filter;
                                if (!twitchcasecontainer.Contains(itemid))
                                {
                                    twitchcasecontainer.Add(itemid);
                                }
                                if (!twitchcasecontainer.Contains(itemid2))
                                {
                                    twitchcasecontainer.Add(itemid2);
                                }
                            }
                            catch (Exception ex)
                            {
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                }
            };
        }
        public static void InitReshalaEdit(VulcanModConfigClass config, DatabaseService databaseService, ModHelper modHelper)
        {
            var bots = databaseService.GetBots();
            var reshala = bots.Types["bossbully"];
            var followerreshala = bots.Types["followerbully"];
            var botReshala = modHelper.GetJsonDataFromFile<BotType>(ConfigManager.modPath, "moddata/bots/Reshala.json");
            var botFollowerReshala = modHelper.GetJsonDataFromFile<BotType>(ConfigManager.modPath, "moddata/bots/ReshalaFollower.json");
            reshala.BotChances.EquipmentChances = botReshala.BotChances.EquipmentChances;
            reshala.BotChances.WeaponModsChances = botReshala.BotChances.WeaponModsChances;
            reshala.BotChances.EquipmentModsChances = botReshala.BotChances.EquipmentModsChances;
            reshala.BotExperience.Reward = botReshala.BotExperience.Reward;
            reshala.BotHealth = botReshala.BotHealth;
            reshala.BotInventory = botReshala.BotInventory;
            reshala.BotSkills = botReshala.BotSkills;
            reshala.BotGeneration = botReshala.BotGeneration;
            followerreshala.BotChances.EquipmentChances = botFollowerReshala.BotChances.EquipmentChances;
            followerreshala.BotChances.WeaponModsChances = botFollowerReshala.BotChances.WeaponModsChances;
            followerreshala.BotChances.EquipmentModsChances = botFollowerReshala.BotChances.EquipmentModsChances;
            followerreshala.BotExperience.Reward = botFollowerReshala.BotExperience.Reward;
            followerreshala.BotHealth = botFollowerReshala.BotHealth;
            followerreshala.BotInventory = botFollowerReshala.BotInventory;
            followerreshala.BotSkills = botFollowerReshala.BotSkills;
            followerreshala.BotGeneration = botFollowerReshala.BotGeneration;
        }
        public static void InitBDReplace(DatabaseService databaseService, ModHelper modHelper, ICloner cloner)
        {
            var bots = databaseService.GetBots();
            var getedlocations = databaseService.GetLocations();
            var locations = new List<SPTarkov.Server.Core.Models.Eft.Common.Location> {
                getedlocations.Bigmap,
                getedlocations.Woods,
                getedlocations.Factory4Day,
                getedlocations.Factory4Night,
                getedlocations.Laboratory,
                getedlocations.Shoreline,
                getedlocations.RezervBase,
                getedlocations.Interchange,
                getedlocations.Lighthouse,
                getedlocations.TarkovStreets,
                getedlocations.Sandbox,
                getedlocations.SandboxHigh
            };
            var jsonUtil = ServiceLocator.ServiceProvider.GetService<JsonUtil>();
            var bloodhound = bots.Types["arenafighterevent"];
            var zhCNLang = databaseService.GetLocales().Global["ch"];
            var botBDOperator = modHelper.GetJsonDataFromFile<BotType>(ConfigManager.modPath, "moddata/bots/BDOperator.json");
            bloodhound.BotAppearance = botBDOperator.BotAppearance;
            bloodhound.BotChances.EquipmentChances = botBDOperator.BotChances.EquipmentChances;
            bloodhound.BotChances.WeaponModsChances = botBDOperator.BotChances.WeaponModsChances;
            bloodhound.BotChances.EquipmentModsChances = botBDOperator.BotChances.EquipmentModsChances;
            bloodhound.BotExperience.Reward = botBDOperator.BotExperience.Reward;
            bloodhound.BotHealth = botBDOperator.BotHealth;
            bloodhound.BotInventory = botBDOperator.BotInventory;
            bloodhound.BotSkills = botBDOperator.BotSkills;
            bloodhound.BotGeneration = botBDOperator.BotGeneration;
            zhCNLang.AddTransformer(lang =>
            {
                lang["ScavRole/ArenaFighterEvent"] = "黑色军团";
                return lang;
            });
            foreach (var location in locations)
            {
                var map = location.Base;
                if (map == null) continue;
                if (map == getedlocations.Woods.Base && modConfig.BlackDivisionMapConfig["Woods"] == true)
                {
                    InitBDEditForMap(map.BossLocationSpawn);
                }
                if (map == getedlocations.Bigmap.Base && modConfig.BlackDivisionMapConfig["Custom"] == true)
                {
                    InitBDEditForMap(map.BossLocationSpawn);
                }
            }
            var bdspawn = cloner.Clone(getedlocations.Bigmap.Base.BossLocationSpawn.Find(x => x.BossName == "bossKillaAgro" || x.BossName == "arenaFighterEvent"));
            if (bdspawn != null)
            {
                if (bdspawn.BossName != "bossKillaAgro")
                {
                    bdspawn.BossName = "bossKillaAgro";
                    bdspawn.BossEscortAmount = "3";
                    bdspawn.BossChance = (double)modConfig.BlackDivisionChance;
                }
                bdspawn.ForceSpawn = true;
                bdspawn.IgnoreMaxBots = true;
                bdspawn.Supports = null;
                //VulcanLog.Debug("进入添加流程", logger);
                if (modConfig.BlackDivisionMapConfig["Lighthouse"] == true)
                {
                    var lighthouse = cloner.Clone(bdspawn);
                    lighthouse.BossZone = "Zone_OldHouse,Zone_Village";
                    databaseService.GetLocations().Lighthouse.Base.BossLocationSpawn.Add(lighthouse);
                }
                if (modConfig.BlackDivisionMapConfig["Labs"] == true)
                {
                    var labs = cloner.Clone(bdspawn);
                    labs.BossZone = "BotZoneFloor1";
                    databaseService.GetLocations().Laboratory.Base.BossLocationSpawn.Add(labs);
                }
                if (modConfig.BlackDivisionMapConfig["Shoreline"] == true)
                {
                    var shoreline = cloner.Clone(bdspawn);
                    shoreline.BossZone = "ZoneSanatorium1,ZoneSanatorium2,ZoneSmuglers";
                    databaseService.GetLocations().Shoreline.Base.BossLocationSpawn.Add(shoreline);
                }
                if (modConfig.BlackDivisionMapConfig["Streets"] == true)
                {
                    var street = cloner.Clone(bdspawn);
                    street.BossZone = "ZoneFactory,ZoneConcordiaParking";
                    databaseService.GetLocations().TarkovStreets.Base.BossLocationSpawn.Add(street);
                }
                //VulcanLog.Log(jsonUtil.Serialize(databaseService.GetLocations().Shoreline.Base.BossLocationSpawn, true), logger);
            }
        }
        public static void InitBDEditForMap(List<BossLocationSpawn> locationSpawns)
        {
            if (locationSpawns == null) return;
            foreach (var boss in locationSpawns)
            {
                if (boss == null) return;
                if (boss.BossName == "arenaFighterEvent")
                {
                    boss.BossName = "bossKillaAgro";
                    boss.BossEscortAmount = "3";
                    boss.BossChance = (double)modConfig.BlackDivisionChance;
                }
            }
        }
        public static void InitBotEdit(DatabaseService databaseService, ModHelper modHelper)
        {
            var bots = databaseService.GetBots();
            var sectantpriest = bots.Types["sectantpriest"];
            sectantpriest.BotInventory.Equipment[SPTarkov.Server.Core.Models.Enums.EquipmentSlots.Pockets].Clear();
            sectantpriest.BotInventory.Equipment[SPTarkov.Server.Core.Models.Enums.EquipmentSlots.Pockets].TryAdd("60c7272c204bc17802313365", 1);
            var mchead = modConfig.MCHeadData;
            foreach (var data in mchead)
            {
                foreach (var name in data.Bot)
                {
                    var bot = bots.Types[name];
                    foreach (var equip in data.Data)
                    {
                        bot.BotInventory.Equipment[SPTarkov.Server.Core.Models.Enums.EquipmentSlots.FaceCover].TryAdd(equip[0].ConvertHashID(), double.Parse(equip[1]));
                    }
                }
            }
        }
        public static void InitHideoutAreaEdit(DatabaseService databaseService)
        {
            var items = databaseService.GetItems();
            var areas = databaseService.GetHideout().Areas;
            var 蓄电池 = items["5733279d245977289b77ec24"];
            var 坦克电池 = items["5d03794386f77420415576f5"];
            蓄电池.Properties.MaxResource = 40;
            蓄电池.Properties.Resource = 40;
            蓄电池.Parent = "5d650c3e815116009f6201d2";
            坦克电池.Properties.MaxResource = 150;
            坦克电池.Properties.Resource = 150;
            坦克电池.Parent = "5d650c3e815116009f6201d2";
            var solarpower = areas.Find(x => x.Type == SPTarkov.Server.Core.Models.Enums.Hideout.HideoutAreas.SolarPower);
            solarpower.Stages["1"].Bonuses.First().Value = -60;
            var solarpowerrequirement = solarpower.Stages["1"].Requirements;
            solarpowerrequirement.Clear();
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TemplateId = "5d0375ff86f774186372f685",
                Count = 8,
                IsFunctional = false,
                IsEncoded = false,
                IsSpawnedInSession = true,
                Type = "Item"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TemplateId = "5d03775b86f774203e7e0c4b",
                Count = 4,
                IsFunctional = false,
                IsEncoded = false,
                IsSpawnedInSession = true,
                Type = "Item"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TemplateId = "5d0378d486f77420421a5ff4",
                Count = 4,
                IsFunctional = false,
                IsEncoded = false,
                IsSpawnedInSession = true,
                Type = "Item"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TemplateId = "6389c85357baa773a825b356",
                Count = 2,
                IsFunctional = false,
                IsEncoded = false,
                IsSpawnedInSession = true,
                Type = "Item"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TemplateId = "5d0376a486f7747d8050965c",
                Count = 2,
                IsFunctional = false,
                IsEncoded = false,
                IsSpawnedInSession = true,
                Type = "Item"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TemplateId = "太阳能模块".ConvertHashID(),
                Count = 1,
                IsFunctional = false,
                IsEncoded = false,
                IsSpawnedInSession = false,
                Type = "Item"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TemplateId = "5696686a4bdc2da3298b456a",
                Count = 50000,
                IsFunctional = false,
                IsEncoded = false,
                IsSpawnedInSession = true,
                Type = "Item"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                AreaType = 4,
                RequiredLevel = 3,
                Type = "Area"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TraderId = "Persicaria".ConvertHashID(),
                LoyaltyLevel = 4,
                Type = "TraderLoyalty"
            });
            solarpowerrequirement.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.StageRequirement
            {
                TraderId = Traders.MECHANIC,
                LoyaltyLevel = 4,
                Type = "TraderLoyalty"
            });
            solarpower.Stages["1"].ConstructionTime = 259200.0;
            areas.Find(x => x.Type == SPTarkov.Server.Core.Models.Enums.Hideout.HideoutAreas.Generator).Stages["1"].Bonuses.First().Filter.Add(ItemTpl.BARTER_CAR_BATTERY);
            areas.Find(x => x.Type == SPTarkov.Server.Core.Models.Enums.Hideout.HideoutAreas.Generator).Stages["2"].Bonuses.First().Filter.Add(ItemTpl.BARTER_6STEN140M_MILITARY_BATTERY);
            areas.Find(x => x.Type == SPTarkov.Server.Core.Models.Enums.Hideout.HideoutAreas.BitcoinFarm).Stages["3"].Requirements.RemoveAll(r => r.Type == "Area" && r.AreaType == 18);
        }
        public static void InitHideoutRecipeEdit(DatabaseService databaseService)
        {
            var recipes = databaseService.GetHideout().Production.Recipes;
            recipes.Find(x => x.EndProduct == "5e85a9f4add9fe03027d9bf1").Requirements.Add(new SPTarkov.Server.Core.Models.Eft.Hideout.Requirement
            {
                TemplateId = "荧石粉".ConvertHashID(),
                Count = 1,
                IsFunctional = false,
                IsEncoded = false,
                Type = "Item"
            }); recipes.Find(x => x.EndProduct == "5a0c27731526d80618476ac4").Requirements.Find(r => r.TemplateId == "590c5a7286f7747884343aea").TemplateId = "荧石粉".ConvertHashID();

        }
        public static void ForcedUnlockEventQuest(DatabaseService databaseService, ConfigServer configServer)
        {
            var eventlist = new List<string>
        {
            "641dbfd7f43eda9d810d7137", //重要伤员
            "64764abcd125ab430a14ccb5", //寻血猎犬
            "647710905320c660d91c15a5", //杀鸡儆猴
            "64916da7ad4e722c106f2345", //东窗事发
            "649af47d717cb30e7e4b5e26", //品酒师
            "655e427b64d09b4122018228", //惩罚者大丰收
            "6672ec2a2b6f3b71be794cc5"  //大妈彩色卡
        };
            var questconfig = configServer.GetConfig<QuestConfig>();
            var quests = databaseService.GetQuests();
            var zhCNLang = databaseService.GetLocales().Global["ch"];
            foreach (var key in eventlist)
            {
                questconfig.EventQuests.Remove(key);
            }
            var 重要伤员 = quests["641dbfd7f43eda9d810d7137"];
            var 寻血猎犬 = quests["64764abcd125ab430a14ccb5"];
            var 东窗事发 = quests["64916da7ad4e722c106f2345"];
            var 品酒师 = quests["649af47d717cb30e7e4b5e26"];
            var 大丰收 = quests["655e427b64d09b4122018228"];
            var smnjtlist = new List<string>
        {
            "59f32bb586f774757e1e8442",
            "59f32c3b86f77472a31742f0",
            "6662ea05f6259762c56f3189",
            "6662e9cda7e0b43baa3d5f76",
            "6662e9f37fa79a6d83730fa0",
            "6662e9aca7e0b43baa3d5f74",
            "6764207f2fa5e32733055c4a",
            "675dc9d37ae1a8792107ca96",
            "6764202ae307804338014c1a",
            "675dcb0545b1a2d108011b2b"
        };
            重要伤员.Conditions.AvailableForFinish[0].OnlyFoundInRaid = true;
            重要伤员.Conditions.AvailableForFinish[0].Value = 100;
            重要伤员.Conditions.AvailableForFinish[1].OnlyFoundInRaid = true;
            重要伤员.Conditions.AvailableForFinish[1].Value = 100;
            寻血猎犬.Conditions.AvailableForFinish[0].Counter.Conditions[0].SavageRole = new List<string> { "exUsec", "pmcBot" };
            东窗事发.Conditions.Fail.Clear();
            品酒师.Conditions.Fail.Clear();
            大丰收.Conditions.AvailableForFinish[1].Target.List.Clear();
            foreach (var key in smnjtlist)
            {
                大丰收.Conditions.AvailableForFinish[1].Target.List.Add(key);
            }
        }
        public static void InitVanillaItemEdit(DatabaseService databaseService)
        {
            var items = databaseService.GetItems();
            var prices = databaseService.GetPrices();

            //原版修改
            //火神头内衬
            items["657bbe73a1c61ee0c303632b"].Properties.ArmorClass = 6;
            items["657bbed0aab96fccee08be96"].Properties.ArmorClass = 6;
            items["657bbefeb30eca9763051189"].Properties.ArmorClass = 6;
            /*
            //市场调整
            //762bp
            items["59e0d99486f7744a32234762"].Properties.CanSellOnRagfair = true;
            prices["59e0d99486f7744a32234762"] = 1751;
            //762ap
            items["601aa3d2b2bcb34913271e6d"].Properties.CanSellOnRagfair = true;
            prices["601aa3d2b2bcb34913271e6d"] = 3299;
            //856a1
            prices["59e6906286f7746c9f75e847"] = 695;
            //855a1
            items["54527ac44bdc2d36668b4567"].Properties.CanSellOnRagfair = true;
            prices["54527ac44bdc2d36668b4567"] = 999;
            //m995
            items["59e690b686f7746c9f75e848"].Properties.CanSellOnRagfair = true;
            prices["59e690b686f7746c9f75e848"] = 2999;
            //hybrid
            items["6529243824cbe3c74a05e5c1"].Properties.CanSellOnRagfair = true;
            prices["6529243824cbe3c74a05e5c1"] = 1799;
            //.300M62 
            prices["619636be6db0f2477964e710"] = 599;
            //CBJ
            items["64b8725c4b75259c590fa899"].Properties.CanSellOnRagfair = true;
            prices["64b8725c4b75259c590fa899"] = 1099;
            //AP20
            items["5d6e68a8a4b9360b6c0d54e2"].Properties.CanSellOnRagfair = true;
            //338FMJ
            items["5fc275cf85fd526b824a571a"].Properties.CanSellOnRagfair = true;
            //PS12B 
            items["5cadf6eeae921500134b2799"].Properties.CanSellOnRagfair = true;
            prices["5cadf6eeae921500134b2799"] = 1999;
            //5457n40
            items[ItemTpl.AMMO_545X39_7N40].Properties.CanSellOnRagfair = true;
            prices[ItemTpl.AMMO_545X39_7N40] = 799;
            //545bp
            items["56dfef82d2720bbd668b4567"].Properties.CanSellOnRagfair = true;
            prices["56dfef82d2720bbd668b4567"] = 1099;
            //545bs
            items["56dff026d2720bb8668b4567"].Properties.CanSellOnRagfair = true;
            prices["56dff026d2720bb8668b4567"] = 2599;
            //m62
            items[ItemTpl.AMMO_762X51_M62].Properties.CanSellOnRagfair = true;
            prices[ItemTpl.AMMO_762X51_M62] = 849;
            //m80
            items["58dd3ad986f77403051cba8f"].Properties.CanSellOnRagfair = true;
            prices["58dd3ad986f77403051cba8f"] = 899;
            //pab9 
            items["61962d879bb3d20b0946d385"].Properties.CanSellOnRagfair = true;
            prices["61962d879bb3d20b0946d385"] = 899;
            //sp6
            items["57a0e5022459774d1673f889"].Properties.CanSellOnRagfair = true;
            prices["57a0e5022459774d1673f889"] = 1599;
            //7n12
            items["5c0d688c86f77413ae3407b2"].Properties.CanSellOnRagfair = true;
            prices["5c0d688c86f77413ae3407b2"] = 2999;
            //lps
            prices["5887431f2459777e1612938f"] = 799;
            //7bt1
            items["5e023d34e8a400319a28ed44"].Properties.CanSellOnRagfair = true;
            prices["5e023d34e8a400319a28ed44"] = 1899;
            //m61
            items["5a6086ea4f39f99cd479502f"].Properties.CanSellOnRagfair = true;
            prices["5a6086ea4f39f99cd479502f"] = 1899;
            //366ap
            items["5f0596629e22f464da6bbdd9"].Properties.CanSellOnRagfair = true;
            prices["5f0596629e22f464da6bbdd9"] = 1199;
            //snb
            items["560d61e84bdc2da74d8b4571"].Properties.CanSellOnRagfair = true;
            prices["560d61e84bdc2da74d8b4571"] = 3999;
            //m80a1
            items["6768c25aa7b238f14a08d3f6"].Properties.CanSellOnRagfair = true;
            prices["6768c25aa7b238f14a08d3f6"] = 4999;
            //7n39
            items["5c0d5e4486f77478390952fe"].Properties.CanSellOnRagfair = true;
            prices["5c0d5e4486f77478390952fe"] = 3499;
            //APSX
            items["5ba26835d4351e0035628ff5"].Properties.CanSellOnRagfair = true;
            prices["5ba26835d4351e0035628ff5"] = 2699;
            //武器调整
            //黑dt
            items["5dcbd56fdbd3d91b3e5468d5"].Properties.CanSellOnRagfair = true;
            prices["65290f395ae2ae97b80fdf2d"] = 186666;
            //spear
            items["65290f395ae2ae97b80fdf2d"].Properties.CanSellOnRagfair = true;
            prices["65290f395ae2ae97b80fdf2d"] = 319000;
            //vss
            items["57838ad32459774a17445cd2"].Properties.CanSellOnRagfair = true;
            prices["57838ad32459774a17445cd2"] = 139999;
            //M10
            items["673cab3e03c6a20581028bc1"].Properties.CanSellOnRagfair = true;
            prices["673cab3e03c6a20581028bc1"] = 299999;
            //配件
            //宙斯热成像 
            items["63fc44e2429a8a166c7f61e6"].Properties.CanSellOnRagfair = true;
            prices["63fc44e2429a8a166c7f61e6"] = 699999;
            //Trijicon 
            items["5a1eaa87fcdbcb001865f75e"].Properties.CanSellOnRagfair = true;
            prices["5a1eaa87fcdbcb001865f75e"] = 799999;
            //四眼夜视仪 
            items["5c0558060db834001b735271"].Properties.CanSellOnRagfair = true;
            prices["5c0558060db834001b735271"] = 199999;
            //6.8弹鼓 
            items["6761770e48fa5c377e06fc3c"].Properties.CanSellOnRagfair = true;
            //556弹鼓 
            items["59c1383d86f774290a37e0ca"].Properties.CanSellOnRagfair = true;
            //55660发弹匣
            items["544a37c44bdc2d25388b4567"].Properties.CanSellOnRagfair = true;
            //RS32热成像 
            items["5d1b5e94d7ad1a2b865a96b0"].Properties.CanSellOnRagfair = true;
            prices["5d1b5e94d7ad1a2b865a96b0"] = 799999;
            //插板
            //原色Killa面
            items["5c0919b50db834001b7ce3b9"].Properties.CanSellOnRagfair = true;
            prices["5c0919b50db834001b7ce3b9"] = 89999;
            //火神面
            items["5ca2113f86f7740b2547e1d2"].Properties.CanSellOnRagfair = true;
            //黑阿尔金面罩 
            items["5f60c85b58eff926626a60f7"].Properties.CanSellOnRagfair = true;
            prices["5f60c85b58eff926626a60f7"] = 129999;
            //5级Killa背板
            items["654a4a964b446df1ad03f192"].Properties.CanSellOnRagfair = true;
            prices["654a4a964b446df1ad03f192"] = 119999;
            //BR4 55耐5级陶瓷美板
            items["65573fa5655447403702a816"].Properties.CanSellOnRagfair = true;
            prices["65573fa5655447403702a816"] = 99999;
            //Cult 5级钛美板
            items["656fa8d700d62bcd2e024084"].Properties.CanSellOnRagfair = true;
            prices["656fa8d700d62bcd2e024084"] = 119999;
            //5级复合美板
            items["656fa53d94b480b8a500c0e4"].Properties.CanSellOnRagfair = true;
            prices["656fa53d94b480b8a500c0e4"] = 139999;
            //45耐5级PE美板
            items["656fae5f7c2d57afe200c0d7"].Properties.CanSellOnRagfair = true;
            prices["656fae5f7c2d57afe200c0d7"] = 169999;
            //5级俄甲背板
            items["657b2797c3dbcb01d60c35ea"].Properties.CanSellOnRagfair = true;
            prices["657b2797c3dbcb01d60c35ea"] = 69999;
            //5级俄甲前板
            items["656f664200d62bcd2e024077"].Properties.CanSellOnRagfair = true;
            prices["656f664200d62bcd2e024077"] = 69999;
            //5级俄甲菱形板
            items["656f611f94b480b8a500c0db"].Properties.CanSellOnRagfair = true;
            prices["656f611f94b480b8a500c0db"] = 79999;
            //5级跳弹板
            items["5c0e66e2d174af02a96252f4"].Properties.CanSellOnRagfair = true;
            prices["5c0e66e2d174af02a96252f4"] = 149999;
            //Galvion3级头
            items["5f60b34a41e30a4ab12a6947"].Properties.CanSellOnRagfair = true;
            prices["5f60b34a41e30a4ab12a6947"] = 69999;
            //新钻石头 
            items["65709d2d21b9f815e208ff95"].Properties.CanSellOnRagfair = true;
            prices["65709d2d21b9f815e208ff95"] = 99999;
            //黄fastmt
            items["5ac8d6885acfc400180ae7b0"].Properties.CanSellOnRagfair = true;
            prices["5ac8d6885acfc400180ae7b0"] = 129999;
            //fastmt
            items["5a154d5cfcdbcb001a3b00da"].Properties.CanSellOnRagfair = true;
            prices["5a154d5cfcdbcb001a3b00da"] = 149999;
            //面罩温迪
            items["5e01ef6886f77445f643baa4"].Properties.CanSellOnRagfair = true;
            prices["5e01ef6886f77445f643baa4"] = 233333;
            //温迪
            items["5e00c1ad86f774747333222c"].Properties.CanSellOnRagfair = true;
            prices["5e00c1ad86f774747333222c"] = 139999;
            //黄温迪面
            items["5e01f37686f774773c6f6c15"].Properties.CanSellOnRagfair = true;
            prices["5e01f37686f774773c6f6c15"] = 59999;
            //黑温迪面
            items["5e00cdd986f7747473332240"].Properties.CanSellOnRagfair = true;
            prices["5e00cdd986f7747473332240"] = 69999;
            */
        }
        public static void AddKabanToShoreline(VulcanModConfigClass config, DatabaseService databaseService)
        {
            var shoreline = databaseService.GetLocations().Shoreline.Base;
            shoreline.BossLocationSpawn.Add(new BossLocationSpawn
            {
                BossChance = (double)modConfig.KabanInShorelineChance,
                BossDifficulty = "normal",
                BossEscortAmount = "2",
                BossEscortDifficulty = "normal",
                BossEscortType = "followerBoar",
                BossName = "bossBoar",
                IsBossPlayer = false,
                BossZone = "ZoneSmuglers",
                Delay = 0,
                ForceSpawn = false,
                IgnoreMaxBots = true,
                IsRandomTimeSpawn = false,
                SpawnMode = new List<string>
            {
                "regular",
                "pve"
            },
                Supports = new List<BossSupport>
            {
                new BossSupport
                {
                    BossEscortAmount = "0",
                    BossEscortDifficulty = new SPTarkov.Server.Core.Utils.Json.ListOrT<string>(new List<string>
                    {
                        "normal"
                    }, null),
                    BossEscortType = "followerBoar"
                },
                new BossSupport
                {
                    BossEscortAmount = "1",
                    BossEscortDifficulty = new SPTarkov.Server.Core.Utils.Json.ListOrT<string>(new List<string>
                    {
                        "normal"
                    }, null),
                    BossEscortType = "followerBoarClose1"
                },
                new BossSupport
                {
                    BossEscortAmount = "1",
                    BossEscortDifficulty = new SPTarkov.Server.Core.Utils.Json.ListOrT<string>(new List<string>
                    {
                        "normal"
                    }, null),
                    BossEscortType = "followerBoarClose2"
                }
            },
                Time = -1,
                TriggerId = "",
                TriggerName = ""
            });
        }
        public static void RemoveBlackAltynLockedCondition(DatabaseService databaseService)
        {
            var recipes = databaseService.GetHideout().Production.Recipes;
            var quests = databaseService.GetQuests();
            var recipesblackalytn = recipes.Find(recipe => recipe.EndProduct == ItemTpl.HEADWEAR_RYST_BULLETPROOF_HELMET_BLACK);
            recipesblackalytn.Requirements.RemoveAll(requirement => requirement.Type == "QuestComplete");
            recipesblackalytn.Locked = false;
            quests["60e71b62a0beca400d69efc4"].Rewards["Success"].RemoveAll(reward => reward.Type == RewardType.ProductionScheme);
        }
        public static void InitOracleQuestData()
        {
            var jsonUtil = ServiceLocator.ServiceProvider.GetService<JsonUtil>();
            var databaseService = ServiceLocator.ServiceProvider.GetService<DatabaseService>();
            var configServer = ServiceLocator.ServiceProvider.GetService<ConfigServer>();
            var modHelper = ServiceLocator.ServiceProvider.GetService<ModHelper>();
            var cloner = ServiceLocator.ServiceProvider.GetService<ICloner>();
            var itemHelper = ServiceLocator.ServiceProvider.GetService<ItemHelper>();
            var imageRouter = ServiceLocator.ServiceProvider.GetService<ImageRouter>();
            var presetHelper = ServiceLocator.ServiceProvider.GetService<PresetHelper>();
            var logger = new ECLogger("PostRagfairLoadEvent", true);
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
            var quests = databaseService.GetQuests();
            var oraclequest = quests["永寂孤芒".ConvertHashID()];
            var conditions = oraclequest.Conditions.AvailableForFinish;
            var foodsanddrinks = conditions[1].Target.List;
            var medicines = conditions[2].Target.List;
            ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.食物, foodsanddrinks, context);
            ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.饮品, foodsanddrinks, context);
            ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.创伤处理, medicines, context);
            ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.急救包, medicines, context);
            ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.注射器, medicines, context);
            ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.药品, medicines, context);
            ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.医疗用品, medicines, context);
        }
    }
}