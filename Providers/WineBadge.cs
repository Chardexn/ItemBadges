namespace ItemBadges
{
    /// <summary>
    /// 成品酒：量产（酒分 3–4）为 0。不需要工具。假酒用特殊紫。
    /// ≤0 -3，1 -2，2 -1，3–4 0，5 +1，≥6 +2。
    /// </summary>
    internal static class WineBadge
    {
        public const string Key = "wine";
        private const int BaselineTier = 3;

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.WineEnabled || item == null || !WineHelper.IsFinishedWine(item))
            {
                return null;
            }

            if (item.IsTag(WineHelper.WINE_COUNTERFEIT_TAG))
            {
                return Counterfeit();
            }

            int delta = WineHelper.GetWineQualityTier(item) - BaselineTier;
            return Badge.FromDelta(delta);
        }

        public static Badge Counterfeit()
        {
            return new Badge(BadgePalette.Special(), "x", "x");
        }

        public static Badge[] Legend()
        {
            return new[]
            {
                Badge.FromDelta(-3),
                Badge.FromDelta(-2),
                Badge.FromDelta(-1),
                Badge.FromDelta(0),
                Badge.FromDelta(1),
                Badge.FromDelta(2),
                Counterfeit()
            };
        }
        #endregion

        #region MARK: 调试生成
        public static void SpawnSamples()
        {
            Spawn(0, false);
            Spawn(1, false);
            Spawn(2, false);
            Spawn(3, false);
            Spawn(5, false);
            Spawn(6, false);
            Spawn(3, true);
        }

        private static void Spawn(int score, bool counterfeit)
        {
            GameItem item = WineHelper.CreateFinishedWineForDebug(score, counterfeit);
            if (item == null)
            {
                ModEntry.Instance?.Log.Warning("wine spawn failed.");
                return;
            }

            BadgeSpawn.Place(item);
        }
        #endregion
    }
}
