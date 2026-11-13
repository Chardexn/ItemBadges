namespace ItemBadges
{
    /// <summary>
    /// 酵母：非法红、普通绿、可循环紫（Compact 时与普通同色靠 ∞ 区分）、超级金。
    /// </summary>
    internal static class YeastBadge
    {
        public const string Key = "yeast";

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.YeastEnabled || item == null || !item.IsTag(WineHelper.WINE_YEAST_TAG))
            {
                return null;
            }

            int power = item.GetTagReadonly(WineHelper.WINE_YEAST_POWER_INT).GetInt();
            if (power < 1)
            {
                power = 1;
            }

            string powerText = power.ToString();
            if (item.IsTag(WineHelper.WINE_YEAST_ILLEGAL_TAG))
            {
                return new Badge(BadgePalette.ForDelta(-1), powerText);
            }

            if (item.IsTag(WineHelper.WINE_YEAST_REUSABLE_TAG))
            {
                return new Badge(BadgePalette.Special(), "∞");
            }

            if (power >= 4)
            {
                return new Badge(BadgePalette.ForDelta(2), powerText);
            }

            return new Badge(BadgePalette.ForDelta(1), powerText);
        }
        #endregion

        #region MARK: 调试生成
        public static void SpawnSamples()
        {
            Spawn("wine_yeast");
            Spawn("wine_superyeast");
            Spawn("wine_yeast_red");
            Spawn("wine_yeast_infinite");
        }

        private static void Spawn(string id)
        {
            GameItem item = ModHelper.SpawnItem(id);
            if (item == null)
            {
                ModEntry.Instance?.Log.Warning(id + " spawn failed.");
                return;
            }

            ModHelper.MoveItemToInventory(item);
        }
        #endregion
    }
}
