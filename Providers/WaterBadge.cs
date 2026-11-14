namespace ItemBadges
{
    /// <summary>
    /// 容器纯净度。标签档不需工具；真实档需要 AquaScan 或未用试纸。
    /// 数值可按水品质档（基础水=0）或酿酒水质分 W 显示。
    /// </summary>
    internal static class WaterBadge
    {
        public const string Key = "water";
        private const int BasePurityIndex = 2;

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.WaterEnabled || item == null)
            {
                return null;
            }

            if (!item.IsTag(WaterHelper.LIQUID_CONTAINER_TAG) || WaterHelper.IsEmpty(item))
            {
                return null;
            }

            int waterAmount = item.GetTagReadonly(Liquid.GetCurrentPartFromId("water")).GetInt();
            if (waterAmount <= 0)
            {
                return null;
            }

            int purity;
            if (BadgeConfig.UseRealPurity)
            {
                if (!ToolAccess.HasWaterTester())
                {
                    return null;
                }

                purity = WaterHelper.GetWaterPurity(item);
            }
            else
            {
                purity = LabeledPurity(item);
            }

            int index = WaterFeatureHelper.GetPurityArrayIndex(purity);
            return FromIndex(index, BadgeConfig.WaterNumbers);
        }

        public static Badge FromIndex(int index, WaterNumberMode mode)
        {
            return Badge.FromDelta(DeltaForIndex(index, mode));
        }

        public static Badge[] Legend(WaterNumberMode? mode = null)
        {
            WaterNumberMode use = mode ?? BadgeConfig.WaterNumbers;
            Badge[] badges = new Badge[6];
            for (int i = 5; i >= 0; i--)
            {
                badges[5 - i] = FromIndex(i, use);
            }

            return badges;
        }

        private static int LabeledPurity(GameItem item)
        {
            ItemFeature feature = item.FindItemFeatureByCategory("CATEGORY_WATER_PURITY");
            if (feature != null && feature.customIntValue1 > 0)
            {
                return feature.customIntValue1;
            }

            return WaterHelper.GetWaterPurity(item);
        }

        private static readonly int[] WineW = { 1, 1, 0, -1, -2, -3 };

        private static int DeltaForIndex(int index, WaterNumberMode mode)
        {
            int clamped = index < 0 ? 0 : (index > 5 ? 5 : index);
            if (mode == WaterNumberMode.WineScore)
            {
                return WineW[clamped];
            }

            return BasePurityIndex - clamped;
        }
        #endregion

        #region MARK: 调试生成
        public static void SpawnSamples()
        {
            for (int grade = 0; grade <= 5; grade++)
            {
                Spawn(grade);
            }
        }

        private static void Spawn(int grade)
        {
            GameItem jug = ModHelper.SpawnItem("water_jug");
            if (jug == null)
            {
                ModEntry.Instance?.Log.Warning("water_jug spawn failed.");
                return;
            }

            WaterHelper.AddWater(jug, grade);
            BadgeSpawn.Place(jug);
        }
        #endregion
    }
}
