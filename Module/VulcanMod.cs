using EternalCycleServer;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using static EternalCycleServer.ContextManager;
namespace VulcanMod
{
    public class VulcanMod
    {
        public static VulcanModConfigClass modConfig = ConfigManager.GetConfig();
        public static void Init(LoadModContext context)
        {
            var globals = context.DB.GetGlobals();
            var items = context.DB.GetItems();
            var prices = context.DB.GetPrices();
            var zhCNLang = context.DB.GetLocales().Global["ch"];
            globals.Configuration.ItemsCommonSettings.MaxBackpackInserting = 99999999;
            InitVanillaItemEdit(context.DB);
            RemoveBlackAltynLockedCondition(context.DB);
            InitModBaseData(context);
            //var botConfig = configServer.GetConfig<BotConfig>();
            //botConfig.BotRolesWithDogTags.Add("assault");
            //new BotGeneratorPatch.AddDogtagToBotPatch().Enable();
            if (modConfig.EnableReshalaEdit)
            {
                InitReshalaEdit(modConfig, context);
            }
            if (modConfig.EnableBlackDivision)
            {
                InitBDReplace(context);
            }
            InitBotEdit(context.DB, context.ModHelper);
            InitWedgeLabFix(context);
            InitHideoutAreaEdit(context.DB);
            if (modConfig.EnableRecipeEdit)
            {
                InitHideoutRecipeEdit(context.DB);
            }
                items[ItemTpl.CONTAINER_STREAMER_ITEM_CASE].Properties.Grids.First().Properties.CellsH = 16;
                items[ItemTpl.CONTAINER_STREAMER_ITEM_CASE].Properties.Grids.First().Properties.CellsV = 16;

            if (modConfig.EnableKabanInShoreline)
            {
                AddKabanToShoreline(modConfig, context.DB);
            }

        }

        public static void InitEquipmentChest(MongoId itemid, ContextManager.LoadModContext context)
        {
            context.DB.GetItems().TryGetValue(itemid, out var targetfilter);
            if (targetfilter != null)
            {
                var filter = targetfilter.Properties.Grids.First().Properties.Filters.First().Filter;
                filter.Clear();
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.头部装备, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.战术胸挂, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.眼部装备, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.耳机, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.背包, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.装备组件, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.防弹衣, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.面部装备, filter, context);
            }
        }

        public static void InitFilePackage(MongoId itemid, ContextManager.LoadModContext context)
        {
            context.DB.GetItems().TryGetValue(itemid, out var targetfilter);
            if (targetfilter != null)
            {
                var filter = targetfilter.Properties.Grids.First().Properties.Filters.First().Filter;
                filter.Clear();
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.其他, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.地图, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.货币, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.情报物品, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.机械钥匙, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.电子钥匙, filter, context);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.特殊物品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.特殊装备, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.次元博物, filter, context, 4);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.贵重物品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.医疗用品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.工具, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.建筑材料, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.日常用品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.易燃物品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.电子产品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.能源物品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.子弹, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.食物, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.饮品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.创伤处理, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.急救包, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.注射器, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.药品, filter, context, 1);
                ItemUtils.AddItemToListByRagfairTag(ERagfairTagsType.容器, filter, context, 1);
            }
        }
        public static void InitModBaseData(LoadModContext context)
        {
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
                    InitEquipmentChest("THICC装备箱".ConvertHashID(), loadContext);
                    InitFilePackage("外勤公文包".ConvertHashID(), loadContext);
                }
                catch (Exception ex)
                {
                }
            };
            EventManager.DataLoadEvent.LoadQuestDataEvent += (loadContext) =>
            {
                try
                {
                    //InitOracleQuestData(loadContext);
                }
                catch (Exception ex)
                {
                }
            }; 
            // 把模组自制的「英雄之证」加进收藏家(3x4)任务的必需物品列表。
            // 注意：原来这里还顺带把复刻的 Mazoni 金色哑铃 / Tigzresq 夹板 也注入了同一份条件，
            // 那段已按需求删除 —— 原版 5.0 的收藏家任务自身就包含这两件物品，无需再注入。
            EventManager.DataLoadEvent.LoadItemEvent += (loadContext) =>
            {
                try
                {
                    var kappa = QuestUtils.GetQuest(QuestTpl.COLLECTOR, loadContext);
                    var conditions = kappa.Conditions.AvailableForFinish;
                    var 英雄之证 = "英雄之证".ConvertHashID();
                    foreach(var condition in conditions)
                    {
                        if (condition?.Target?.IsList == true && condition?.Target?.List!=null && condition.Target.List.Count>0)
                        {
                            if(!condition.Target.List.Contains(英雄之证))
                            {
                                condition.Target.List.Add(英雄之证);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            };
        }
        public static void InitReshalaEdit(VulcanModConfigClass config, LoadModContext context)
        {
            var bots = context.DB.GetBots();
            var reshala = bots.Types["bossbully"];
            var followerreshala = bots.Types["followerbully"];
            var botReshala = context.ModHelper.GetJsonDataFromFile<BotType>(ConfigManager.modPath, "moddata/bots/Reshala.json");
            var botFollowerReshala = context.ModHelper.GetJsonDataFromFile<BotType>(ConfigManager.modPath, "moddata/bots/ReshalaFollower.json");
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
        public static void InitBDReplace(LoadModContext context)
        {
            var getedlocations = context.DB.GetLocations();
            var zhCNLang = context.DB.GetLocales().Global["ch"];
            // 【已废弃】原 BDOperator.json 覆盖已删除：文件保留在 moddata/bots/ 备查，但不再被引用；
            // 伟哥小队小弟 followerWedgeLab 改用原版数据，不再套任何自制兵种。
            // 角色名本地化：原「寻血猎犬/黑色军团」的 ScavRole/ArenaFighterEvent 改为 Wedge 小队。
            // 经核对 SPT_Data/database/locales/global/ch.json，5.0 的全局本地化表只有 16 个通用
            // ScavRole/* 键（其中并没有 ScavRole/BossWedgeLab、ScavRole/FollowerWedgeLab 这类逐
            // Boss 键），逐 Boss 的角色名由客户端内置；这里按 EFT 的 ScavRole/<角色名> 约定补上
            // 小弟键，交客户端本地化表合并覆盖。
            zhCNLang.AddTransformer(lang =>
            {
                lang["ScavRole/FollowerWedgeLab"] = "伟哥小队";
                return lang;
            });
            // 克隆模板改为原版 Wedge 本尊(bossWedgeLab)。它只静态存在于 laboratory_dark
            // （普通 laboratory 原版没有 Wedge 刷点），因此从 LaboratoryDark 的刷点表里取模板。
            // 取不到就整体跳过（不再依赖海关/森林的 arenaFighterEvent 事件刷点）。
            var wedgeTemplate = getedlocations.LaboratoryDark?.Base?.BossLocationSpawn?.Find(x => x.BossName == "bossWedgeLab");
            if (wedgeTemplate == null)
            {
                Utils.commonLogger.Warn("[火神重工] laboratory_dark 中找不到 bossWedgeLab 模板，跳过伟哥小队多地图注入");
                return;
            }
            var bdspawn = context.Cloner.Clone(wedgeTemplate);
            bdspawn.BossName = "bossWedgeLab";
            bdspawn.BossEscortType = "followerWedgeLab";
            bdspawn.BossEscortAmount = "3";
            bdspawn.BossChance = (double)modConfig.BlackDivisionChance;
            bdspawn.ForceSpawn = true;
            bdspawn.IgnoreMaxBots = true;
            bdspawn.Supports = null;
            //VulcanLog.Debug("进入添加流程", logger);
            if (modConfig.BlackDivisionMapConfig["Lighthouse"] == true)
            {
                var lighthouse = context.Cloner.Clone(bdspawn);
                lighthouse.BossZone = "Zone_OldHouse,Zone_Village";
                context.DB.GetLocations().Lighthouse.Base.BossLocationSpawn.Add(lighthouse);
            }
            // 普通 laboratory 原版只有 laboratory_dark 才有 Wedge，这里补一条本尊注入。
            if (modConfig.BlackDivisionMapConfig["Labs"] == true)
            {
                var labs = context.Cloner.Clone(bdspawn);
                labs.BossZone = "BotZoneFloor1";
                context.DB.GetLocations().Laboratory.Base.BossLocationSpawn.Add(labs);
            }
            if (modConfig.BlackDivisionMapConfig["Shoreline"] == true)
            {
                var shoreline = context.Cloner.Clone(bdspawn);
                shoreline.BossZone = "ZoneSanatorium1,ZoneSanatorium2,ZoneSmuglers";
                context.DB.GetLocations().Shoreline.Base.BossLocationSpawn.Add(shoreline);
            }
            if (modConfig.BlackDivisionMapConfig["Streets"] == true)
            {
                var street = context.Cloner.Clone(bdspawn);
                street.BossZone = "ZoneFactory,ZoneConcordiaParking";
                context.DB.GetLocations().TarkovStreets.Base.BossLocationSpawn.Add(street);
            }
            // 海关/森林原先走 InitBDEditForMap 就地改名，但那两张图的 BossLocationSpawn 里根本
            // 没有 arenaFighterEvent（海关只有 bossBully/bossKnight/bossPartisan，森林只有
            // bossKojaniy/bossKnight/bossPartisan），那两条调用从未生效过。现在统一改为与其它
            // 四张图一致的克隆注入，BossZone 取各自本图原版 Boss 的区名。
            if (modConfig.BlackDivisionMapConfig["Woods"] == true)
            {
                var woods = context.Cloner.Clone(bdspawn);
                woods.BossZone = "ZoneWoodCutter";                       // 取自森林本图 bossKojaniy 的区名
                context.DB.GetLocations().Woods.Base.BossLocationSpawn.Add(woods);
            }
            if (modConfig.BlackDivisionMapConfig["Custom"] == true)
            {
                var custom = context.Cloner.Clone(bdspawn);
                custom.BossZone = "ZoneDormitory,ZoneGasStation,ZoneScavBase";   // 取自海关本图 bossBully 的区名
                context.DB.GetLocations().Bigmap.Base.BossLocationSpawn.Add(custom);
            }
            //VulcanLog.Log(jsonUtil.Serialize(context.DB.GetLocations().Shoreline.Base.BossLocationSpawn, true), logger);
        }
        // 【已废弃】海关/森林改为克隆注入后，本方法不再被任何地方调用，保留备查。
        public static void InitBDEditForMap(List<BossLocationSpawn> locationSpawns)
        {
            if (locationSpawns == null) return;
            foreach (var boss in locationSpawns)
            {
                if (boss == null) return;
                if (boss.BossName == "arenaFighterEvent")
                {
                    // 原逻辑把寻血猎犬事件点就地改名成黑色军团本尊(bossKillaAgro)。
                    // 现在整体换装为 Wedge 小队：本尊 bossWedgeLab + 小弟 followerWedgeLab。
                    // 注意：这里没有按字面只把 BossName 改成 followerWedgeLab —— 那样该刷点会
                    // 只剩小弟、本尊缺席；等价处理为「本尊 + 小弟护卫」，与其它地图的克隆体一致。
                    boss.BossName = "bossWedgeLab";
                    boss.BossEscortType = "followerWedgeLab";
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
                    // ⚠ 5.0 把 bots/types.json 拆成了 bots/types/*.json（文件名即键），
                    //   且 PMC 类型由 bear / usec 改名为 pmcbear / pmcusec。
                    //   名字对不上时原来的索引器会抛 KeyNotFoundException，而 InitBotEdit
                    //   只是 Init() 的第 4 步 —— 一炸，后面藏身处/配方/卡班/活动任务
                    //   等全部步骤都不会执行。这里改为容错跳过。
                    if (!bots.Types.TryGetValue(name, out var bot))
                    {
                        Utils.commonLogger.Warn($"[火神重工] bots.Types 缺少 '{name}'，跳过（5.0 的 PMC 类型名为 pmcbear / pmcusec）");
                        continue;
                    }
                    foreach (var equip in data.Data)
                    {
                        bot.BotInventory.Equipment[SPTarkov.Server.Core.Models.Enums.EquipmentSlots.FaceCover].TryAdd(equip[0].ConvertHashID(), double.Parse(equip[1]));
                    }
                }
            }
        }
        /// <summary>
        /// 黑曜石（异格伟哥）与伟哥小弟名字池的两处修补 —— 全部在数据层完成，不碰 SPT_Data 文件
        /// （改 SPT_Data/database 下的 json 会破坏 checks.dat 的完整性校验）。
        ///
        /// ① 把黑曜石的「背包」池并入角色模板 bosswedgelab。
        ///    SPT 的 bot 战利品池是按「角色」缓存的（BotLootCacheService，每局只建一次、
        ///    EndLocalRaid 才清），由该角色这一局第一个跑到「生成战利品」那一步的 bot 建。
        ///    异格和原版共用同一个角色名，所以谁先建缓存谁说了算 ——
        ///    原版 bosswedgelab 的 inventory.items.Backpack 是空对象，而黑曜石的身家
        ///    （实验室钥匙卡 / LEDX / TerraGroup 蓝色文件夹 …）全在背包池，
        ///    于是缓存被原版抢先时黑曜石背包全空。
        ///    只并 Backpack：原版伟哥的 chances.equipment.Backpack 是 0（本尊根本不背背包），
        ///    并进去的池子对它完全无效 —— 对原版零影响，却保证黑曜石无论谁建缓存都拿得到背包战利品。
        ///
        /// ② 删掉 followerwedgelab 名字池里的 "Obsidian"。
        ///    原版伟哥小弟的名字池有 150 个随机名，其中就含 "Obsidian"
        ///    （其它 boss / 小弟的名字池都只有 1 个名字，所以只有这一处会撞）。
        ///    于是普通小弟会随机叫黑曜石，而 BotGeneratorPatch 的 Postfix 是按**昵称**判定的
        ///    （塞实验室管理员钥匙卡、发水晶狗牌_红、塞英雄之证）——
        ///    玩家就会看到一个「一贫如洗的黑曜石」（原版小弟 Backpack 几率 0、Pockets 池只有 7 条）。
        /// </summary>
        public static void InitWedgeLabFix(LoadModContext context)
        {
            var bots = context.DB.GetBots();

            // ① 只把背包池并入角色 bosswedgelab
            if (!bots.Types.TryGetValue("bosswedgelab", out var wedgeLab))
            {
                Utils.commonLogger.Warn("[火神重工] bots.Types 缺少 'bosswedgelab'，跳过黑曜石背包池合并");
            }
            else
            {
                var obsidian = context.ModHelper.GetJsonDataFromFile<BotType>(
                    ConfigManager.modPath, "moddata/bots/Obsidian.json");
                if (obsidian?.BotInventory?.Items?.Backpack == null)
                {
                    Utils.commonLogger.Warn("[火神重工] 读不到 Obsidian.json 的背包池，跳过合并");
                }
                else
                {
                    var added = 0;
                    foreach (var kv in obsidian.BotInventory.Items.Backpack)
                    {
                        if (wedgeLab.BotInventory.Items.Backpack.ContainsKey(kv.Key))
                        {
                            continue;
                        }

                        wedgeLab.BotInventory.Items.Backpack.Add(kv.Key, kv.Value);
                        added++;
                    }

                    Utils.commonLogger.Info($"[火神重工] 黑曜石背包池已并入角色 bosswedgelab：新增 {added} 条（原版伟哥 Backpack 装备几率为 0，不受影响）");
                }
            }

            // ② 小弟名字池里的 "Obsidian"
            if (bots.Types.TryGetValue("followerwedgelab", out var follower) && follower.FirstNames != null)
            {
                var removed = follower.FirstNames.RemoveAll(n => n == "Obsidian");
                if (removed > 0)
                {
                    Utils.commonLogger.Info($"[火神重工] followerwedgelab 名字池已移除 {removed} 个 'Obsidian'（防止普通小弟冒充异格黑曜石）");
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
                IsSpawnedInSession = false,
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
            // ⚠ 5.0 改名：BSG 把这一类物品的 "Bulletproof Helmet" 统一改成 "Assault Helmet"，
            //   4.1 的 HEADWEAR_RYST_BULLETPROOF_HELMET_BLACK → 5.0 的 HEADWEAR_RYST_ASSAULT_HELMET_BLACK
            //   （同厂同色，已对照 5.0 的 ItemTpl 枚举确认：5.0 里已无任何 *_RYST_BULLETPROOF_* 成员）
            var recipesblackalytn = recipes.Find(recipe => recipe.EndProduct == ItemTpl.HEADWEAR_RYST_ASSAULT_HELMET_BLACK);
            recipesblackalytn.Requirements.RemoveAll(requirement => requirement.Type == "QuestComplete");
            recipesblackalytn.Locked = false;
            quests["60e71b62a0beca400d69efc4"].Rewards["Success"].RemoveAll(reward => reward.Type == RewardType.ProductionScheme);
        }
        public static void InitOracleQuestData(LoadModContext context)
        {
            var quests = context.DB.GetQuests();
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