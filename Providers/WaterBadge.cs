namespace ItemBadges
{
    /// <summary>
    /// 容器真实纯净度，基础水（98%）为 0。
    /// 纯水 +2、优质 +1、基础 0、幽灵 -1、锈水 -2、沟水 -3。
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

            int index = WaterFeatureHelper.GetPurityArrayIndex(WaterHelper.GetWaterPurity(item));
            int delta = BasePurityIndex - index;
            string text = delta > 0 ? "+" + delta : delta.ToString();
            return new Badge(BadgePalette.ForDelta(delta), text);
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
            ModHelper.MoveItemToInventory(jug);
        }
        #endregion
    }
}
