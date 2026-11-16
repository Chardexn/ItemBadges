using System;
using System.Collections.Generic;

namespace ItemBadges
{
    /// <summary>
    /// F9 工坊截图：原版 / 枪械 / 酿酒拓展分条命令，按种类和档位排到前柜。
    /// </summary>
    internal static class BadgeShowcase
    {
        private enum Kind
        {
            Berry = 0,
            Pulp = 1,
            Yeast = 2,
            Water = 3,
            Wine = 4,
            Gun = 5,
            Ingot = 6,
            Chem = 7,
            Other = 8
        }

        private static readonly string[] ToolIds =
        {
            "magnifier",
            "aquascan",
            "chem_scanner"
        };

        private static readonly string[] ModPlants =
        {
            "be_chili_fruit",
            "be_void_plum_fruit"
        };

        private static readonly string[] ModWines =
        {
            "be_chili_wine",
            "be_void_plum_wine"
        };

        private static readonly ItemFeatureList.EquipmentCondition[] GunGrades =
        {
            ItemFeatureList.EquipmentCondition.Poor,
            ItemFeatureList.EquipmentCondition.Fair,
            ItemFeatureList.EquipmentCondition.Good,
            ItemFeatureList.EquipmentCondition.Excellent,
            ItemFeatureList.EquipmentCondition.New
        };

        private static readonly string[] IngotGrades =
        {
            IngotPurityHelper.INGOT_PURITY_VERY_LOW,
            IngotPurityHelper.INGOT_PURITY_LOW,
            IngotPurityHelper.INGOT_PURITY_FAIR,
            IngotPurityHelper.INGOT_PURITY_HIGH,
            IngotPurityHelper.INGOT_PURITY_VERY_HIGH,
            IngotPurityHelper.INGOT_PURITY_PERFECT
        };

        private static readonly int[] BerryGrades = { -2, -1, 1, 2 };
        private static readonly int[] WineScores = { 0, 1, 2, 3, 5, 6, 7 };
        private static readonly int[] ModWineScores = { 0, 2, 6, 7 };

        #region MARK: 入口
        public static void RunVanilla()
        {
            Run("Vanilla", BadgeSpawn.ShowcaseVanillaTag, AddGradeRows);
        }

        public static void RunGuns()
        {
            Run("Guns", BadgeSpawn.ShowcaseGunTag, AddGunRows);
        }

        public static void RunMod()
        {
            Run("Mod", BadgeSpawn.ShowcaseModTag, AddBrewingRows);
        }

        private static void Run(string label, string tag, Action<List<GameItem>, string> fill)
        {
            if (EmporiumEntry.Instance == null)
            {
                ModEntry.Instance?.Log.Warning("IB Showcase: enter a save first.");
                return;
            }

            try
            {
                Clear(tag);
                PlaceTools();
                List<GameItem> items = new List<GameItem>();
                fill(items, tag);
                items.Sort(Compare);
                int placed = LayOut(items, tag);
                ToolAccess.Invalidate();
                BadgeRenderer.RefreshWorld();
                ModHelper.Delay(ModEntry.MOD_ID, 0.25f, BadgeRenderer.RefreshWorld);
                ModEntry.Instance?.Log.Log(
                    "IB Showcase " + label + ": created " + items.Count + ", placed " + placed + ".");
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Log.Error("IB Showcase " + label + " failed: " + ex);
            }
        }

        public static void Clear()
        {
            Clear(BadgeSpawn.ShowcaseVanillaTag);
            Clear(BadgeSpawn.ShowcaseModTag);
            Clear(BadgeSpawn.ShowcaseGunTag);
        }

        private static void Clear(string tag)
        {
            List<GameItem> stale = new List<GameItem>();
            ToolAccess.ForEachItem(delegate(GameItem item)
            {
                if (item != null && item.IsTag(tag))
                {
                    stale.Add(item);
                }
            });

            for (int i = 0; i < stale.Count; i++)
            {
                BadgeSpawn.Discard(stale[i]);
            }
        }
        #endregion

        #region MARK: 工具
        private static void PlaceTools()
        {
            GraphNodeStorage bag = TargetBag();
            for (int i = 0; i < ToolIds.Length; i++)
            {
                GameItem tool = TryCreate(ToolIds[i], null);
                if (tool == null)
                {
                    continue;
                }

                if (!BadgeSpawn.PlaceInto(bag, tool) && !BadgeSpawn.PlaceInto(TargetCounter(), tool))
                {
                    BadgeSpawn.Place(tool);
                }
            }

            ToolAccess.Invalidate();
        }
        #endregion

        #region MARK: 档位样例
        private static void AddGradeRows(List<GameItem> items, string tag)
        {
            for (int i = 0; i < BerryGrades.Length; i++)
            {
                AddBerry("wine_berry", BerryGrades[i], items, tag);
            }

            AddIfCreated("wine_yeast", items, tag);
            AddIfCreated("wine_superyeast", items, tag);
            AddIfCreated("wine_yeast_red", items, tag);
            AddIfCreated("wine_yeast_infinite", items, tag);

            for (int grade = 0; grade <= 5; grade++)
            {
                GameItem bottle = TryCreate("large_bottled_water", tag) ?? TryCreate("water_jug", tag);
                if (bottle == null)
                {
                    continue;
                }

                WaterHelper.AddWater(bottle, grade);
                items.Add(bottle);
            }

            for (int i = 0; i < WineScores.Length; i++)
            {
                AddWine(WineScores[i], false, items, tag);
            }

            AddWine(3, true, items, tag);
            AddFermenting(5, items, tag);

            for (int i = 0; i < IngotGrades.Length; i++)
            {
                GameItem ingot = TryCreate("metal_ingot", tag);
                if (ingot == null)
                {
                    continue;
                }

                IngotPurityHelper.SetPurity(ingot, IngotGrades[i]);
                items.Add(ingot);
            }

            AddChem(ItemConditionList.CreateStarchGrade(), items, tag);
            AddChem(ItemConditionList.CreateJunkGrade(), items, tag);
            AddChem(ItemConditionList.CreateCutProduct(), items, tag);
            AddChem(ItemConditionList.CreateStreetStandard(), items, tag);
            AddChem(ItemConditionList.CreateNearPharma(), items, tag);
            AddChem(ItemConditionList.CreatePharmaGrade(), items, tag);
            AddChem(ItemConditionList.CreateLabPure(), items, tag);
        }
        #endregion

        #region MARK: 枪械
        private static void AddGunRows(List<GameItem> items, string tag)
        {
            for (int i = 0; i < GunGrades.Length; i++)
            {
                GameItem gun = TryCreate("handmade_pistol", tag) ?? TryCreate("revolver", tag);
                if (gun == null)
                {
                    continue;
                }

                GunHelper.SetEquipmentCondition(gun, GunGrades[i]);
                items.Add(gun);
            }
        }
        #endregion

        #region MARK: 酿酒拓展
        private static void AddBrewingRows(List<GameItem> items, string tag)
        {
            if (!ModHelper.IsModRunning("brewing_expansion"))
            {
                ModEntry.Instance?.Log.Warning("IB Showcase Mod: brewing_expansion is not running.");
            }

            for (int p = 0; p < ModPlants.Length; p++)
            {
                for (int i = 0; i < BerryGrades.Length; i++)
                {
                    AddBerry(ModPlants[p], BerryGrades[i], items, tag);
                }
            }

            for (int w = 0; w < ModWines.Length; w++)
            {
                for (int i = 0; i < ModWineScores.Length; i++)
                {
                    AddModWine(ModWines[w], ModWineScores[i], items, tag);
                }
            }
        }
        #endregion

        #region MARK: 分类排序
        private static Kind Classify(GameItem item)
        {
            if (item == null)
            {
                return Kind.Other;
            }

            if (item.identifier == PulpBadge.ItemId || item.IsTag(PulpBadge.ScoreSumTag))
            {
                return Kind.Pulp;
            }

            if (item.IsTag(WineHelper.BERRY_TAG) || item.IsTag(WineHelper.BERRY_CONDITION_INT))
            {
                return Kind.Berry;
            }

            if (item.IsTag(WineHelper.WINE_YEAST_TAG))
            {
                return Kind.Yeast;
            }

            if (WineHelper.IsFermentingOrFinished(item))
            {
                return Kind.Wine;
            }

            if (GunHelper.IsSelfDefenceGun(item))
            {
                return Kind.Gun;
            }

            if (item.IsTag(IngotPurityHelper.INGOT_PURITY_TAG))
            {
                return Kind.Ingot;
            }

            if (ChemicalProductHelper.HasChemicalPurity(item))
            {
                return Kind.Chem;
            }

            if (item.IsTag(WaterHelper.LIQUID_CONTAINER_TAG)
                && !WaterHelper.IsEmpty(item)
                && item.GetTagReadonly(Liquid.GetCurrentPartFromId("water")).GetInt() > 0)
            {
                return Kind.Water;
            }

            return Kind.Other;
        }

        private static int SortDelta(GameItem item)
        {
            Kind kind = Classify(item);
            switch (kind)
            {
                case Kind.Berry:
                    return BerryBadge.Contribution(
                        item.GetTagReadonly(WineHelper.BERRY_CONDITION_INT).GetInt()) ?? 0;
                case Kind.Pulp:
                    int sum = item.IsTag(PulpBadge.ScoreSumTag)
                        ? item.GetTagReadonly(PulpBadge.ScoreSumTag).GetInt()
                        : 0;
                    return sum;
                case Kind.Yeast:
                    if (item.IsTag(WineHelper.WINE_YEAST_ILLEGAL_TAG))
                    {
                        return -2;
                    }

                    if (item.IsTag(WineHelper.WINE_YEAST_REUSABLE_TAG))
                    {
                        return 3;
                    }

                    if (item.identifier == "wine_superyeast"
                        || item.GetTagReadonly(WineHelper.WINE_YEAST_POWER_INT).GetInt() >= 4)
                    {
                        return 2;
                    }

                    return 1;
                case Kind.Water:
                    return WaterFeatureHelper.GetPurityArrayIndex(WaterHelper.GetWaterPurity(item));
                case Kind.Wine:
                    if (item.IsTag(WineHelper.WINE_COUNTERFEIT_TAG))
                    {
                        return 99;
                    }

                    return WineBadge.GradeDelta(item.GetTagReadonly(WineHelper.WINE_SCORE_INT).GetInt());
                case Kind.Gun:
                    ItemFeature gun = item.FindItemFeatureByCategory(GunBadge.Category);
                    string gunId = gun != null && gun.realCondition != null
                        ? gun.realCondition.identifier
                        : null;
                    return GunBadge.DeltaForId(gunId) ?? 0;
                case Kind.Ingot:
                    string purity = IngotPurityHelper.GetPurity(item);
                    if (!string.IsNullOrEmpty(purity)
                        && IngotPurityHelper.ingotPurities.TryGetValue(
                            purity,
                            out IngotPurityHelper.IngotPurity data))
                    {
                        return data.PurityLevel;
                    }

                    return 0;
                case Kind.Chem:
                    ItemFeature chem = ChemicalProductHelper.GetChemicalFeature(item);
                    string gradeId = chem != null && chem.realCondition != null
                        ? chem.realCondition.identifier
                        : null;
                    return string.IsNullOrEmpty(gradeId)
                        ? 0
                        : ChemicalProductHelper.GetGradeNumber(gradeId);
                default:
                    return 0;
            }
        }

        private static int Compare(GameItem a, GameItem b)
        {
            int kind = Classify(a).CompareTo(Classify(b));
            if (kind != 0)
            {
                return kind;
            }

            int delta = SortDelta(a).CompareTo(SortDelta(b));
            if (delta != 0)
            {
                return delta;
            }

            string idA = a != null ? a.identifier : "";
            string idB = b != null ? b.identifier : "";
            return string.CompareOrdinal(idA, idB);
        }
        #endregion

        #region MARK: 放置
        private static int LayOut(List<GameItem> items, string tag)
        {
            GraphNodeStorage[] targets =
            {
                TargetCounter(),
                TargetBack(),
                TargetBag()
            };

            int placed = 0;
            for (int i = 0; i < items.Count; i++)
            {
                GameItem item = items[i];
                if (item == null)
                {
                    continue;
                }

                BadgeSpawn.Own(item, tag);
                bool ok = false;
                for (int t = 0; t < targets.Length; t++)
                {
                    if (BadgeSpawn.PlaceInto(targets[t], item))
                    {
                        ok = true;
                        break;
                    }
                }

                if (!ok)
                {
                    BadgeSpawn.Place(item);
                    ok = item.parentInventory != null;
                }

                if (ok)
                {
                    placed++;
                }
                else
                {
                    ModEntry.Instance?.Log.Warning("IB Showcase could not place " + item.identifier);
                }
            }

            return placed;
        }

        private static GraphNodeStorage TargetCounter()
        {
            EmporiumEntry shop = EmporiumEntry.Instance;
            if (shop == null)
            {
                return null;
            }

            return shop.frontInvinvElement != null ? shop.frontInvinvElement : (GraphNodeStorage)shop.frontInvWindow;
        }

        private static GraphNodeStorage TargetBack()
        {
            EmporiumEntry shop = EmporiumEntry.Instance;
            if (shop == null)
            {
                return null;
            }

            return shop.backInvinvElement != null ? shop.backInvinvElement : (GraphNodeStorage)shop.backInvWindow;
        }

        private static GraphNodeStorage TargetBag()
        {
            EmporiumEntry shop = EmporiumEntry.Instance;
            if (shop == null)
            {
                return null;
            }

            return shop.invElement != null ? shop.invElement : (GraphNodeStorage)shop.invWindow;
        }
        #endregion

        #region MARK: 生成辅助
        private static GameItem TryCreate(string id, string tag)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            try
            {
                if (!DirectoryMaster.Has<GameItem>(id))
                {
                    return null;
                }

                GameItem item = ModHelper.SpawnItem(id);
                if (item == null)
                {
                    return null;
                }

                BadgeSpawn.Own(item, tag);
                BadgeSpawn.Detach(item);
                return item;
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Log.Warning("IB Showcase spawn " + id + ": " + ex.Message);
                return null;
            }
        }

        private static void AddIfCreated(string id, List<GameItem> items, string tag)
        {
            GameItem item = TryCreate(id, tag);
            if (item != null)
            {
                items.Add(item);
            }
        }

        private static void AddBerry(string id, int condition, List<GameItem> items, string tag)
        {
            GameItem item = TryCreate(id, tag);
            if (item == null)
            {
                return;
            }

            WineHelper.SetBerryCondition(item, condition);
            items.Add(item);
        }

        private static void AddWine(int score, bool counterfeit, List<GameItem> items, string tag)
        {
            GameItem item = WineHelper.CreateFinishedWineForDebug(score, counterfeit);
            if (item == null)
            {
                return;
            }

            BadgeSpawn.Own(item, tag);
            BadgeSpawn.Detach(item);
            items.Add(item);
        }

        private static void AddModWine(string id, int score, List<GameItem> items, string tag)
        {
            GameItem item = TryCreate(id, tag);
            if (item == null)
            {
                item = WineHelper.CreateFinishedWineForDebug(score, false);
                if (item == null)
                {
                    return;
                }

                item.identifier = id;
                BadgeSpawn.Own(item, tag);
                BadgeSpawn.Detach(item);
            }

            item.EnableTag(WineHelper.WINE_CONTAINER_TAG);
            item.ModifyTag(WineHelper.WINE_CONTAINER_STATE, s => s.Enable().SetInt(2));
            item.ModifyTag(WineHelper.WINE_SCORE_INT, s => s.Enable().SetInt(score));
            items.Add(item);
        }

        private static void AddFermenting(int score, List<GameItem> items, string tag)
        {
            GameItem item = TryCreate("wine_bottle", tag);
            if (item == null)
            {
                return;
            }

            item.ModifyTag(WineHelper.WINE_CONTAINER_STATE, s => s.Enable().SetInt(1));
            item.ModifyTag(WineHelper.WINE_SCORE_INT, s => s.Enable().SetInt(score));
            item.SetSpriteAndShape(WineHelper.WINE_SPRITE_ATLAS, "wine_fermenting");
            items.Add(item);
        }

        private static void AddChem(ItemCondition condition, List<GameItem> items, string tag)
        {
            GameItem item = TryCreate("caffeine_pill", tag);
            if (item == null)
            {
                return;
            }

            ChemicalProductHelper.SetPurityFeature(item, condition);
            items.Add(item);
        }

        #endregion
    }
}
