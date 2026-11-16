namespace ItemBadges
{
    /// <summary>
    /// 酿造中与成品酒：自酿（酒分 2）为 0，琼浆（6）为 +3。假酒用特殊紫。
    /// </summary>
    internal static class WineBadge
    {
        public const string Key = "wine";
        private const int HomebrewScore = 2;

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.WineEnabled || item == null || !WineHelper.IsFermentingOrFinished(item))
            {
                return null;
            }

            if (item.IsTag(WineHelper.WINE_COUNTERFEIT_TAG))
            {
                return Counterfeit();
            }

            int score = item.GetTagReadonly(WineHelper.WINE_SCORE_INT).GetInt();
            return FromScore(score, BadgeConfig.WineNumbers);
        }

        public static Badge FromScore(int score, WineNumberMode mode)
        {
            if (mode == WineNumberMode.Score)
            {
                int colorDelta = score - HomebrewScore;
                return new Badge(
                    BadgePalette.ForDelta(colorDelta),
                    Badge.FormatDelta(score),
                    Badge.ArrowForDelta(colorDelta));
            }

            return Badge.FromDelta(GradeDelta(score));
        }

        public static Badge Counterfeit()
        {
            return new Badge(BadgePalette.Special(), "x", "x");
        }

        public static Badge[] Legend(WineNumberMode? mode = null)
        {
            WineNumberMode use = mode ?? BadgeConfig.WineNumbers;
            if (use == WineNumberMode.Score)
            {
                return new[]
                {
                    FromScore(0, use),
                    FromScore(1, use),
                    FromScore(2, use),
                    FromScore(4, use),
                    FromScore(5, use),
                    FromScore(6, use),
                    FromScore(7, use),
                    Counterfeit()
                };
            }

            return new[]
            {
                Badge.FromDelta(-2),
                Badge.FromDelta(-1),
                Badge.FromDelta(0),
                Badge.FromDelta(1),
                Badge.FromDelta(2),
                Badge.FromDelta(3),
                Badge.FromDelta(4),
                Counterfeit()
            };
        }

        internal static int GradeDelta(int score)
        {
            if (score <= 2)
            {
                return score - HomebrewScore;
            }

            if (score <= 4)
            {
                return 1;
            }

            if (score == 5)
            {
                return 2;
            }

            if (score == 6)
            {
                return 3;
            }

            return score - 3;
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
            Spawn(7, false);
            Spawn(3, true);
            SpawnFermenting(5);
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

        private static void SpawnFermenting(int score)
        {
            GameItem item = ModHelper.SpawnItem("wine_bottle");
            if (item == null)
            {
                ModEntry.Instance?.Log.Warning("wine fermenting spawn failed.");
                return;
            }

            item.ModifyTag(WineHelper.WINE_CONTAINER_STATE, s => s.Enable().SetInt(1));
            item.ModifyTag(WineHelper.WINE_SCORE_INT, s => s.Enable().SetInt(score));
            item.SetSpriteAndShape(WineHelper.WINE_SPRITE_ATLAS, "wine_fermenting");
            BadgeSpawn.Place(item);
        }
        #endregion
    }
}
