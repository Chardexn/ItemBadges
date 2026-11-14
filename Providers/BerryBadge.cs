namespace ItemBadges
{
    /// <summary>
    /// 葡萄 BERRY_CONDITION_INT → 酒分实际贡献。需要放大镜才显示。
    /// -2 发霉 → -1；-1 欠/过熟 → 0；1 正常 → +1；2 小颗 → +2。
    /// </summary>
    internal static class BerryBadge
    {
        public const string Key = "berry";

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.BerryEnabled || item == null || !item.IsTag(WineHelper.BERRY_TAG))
            {
                return null;
            }

            if (!ToolAccess.HasMagnifier())
            {
                return null;
            }

            int condition = item.GetTagReadonly(WineHelper.BERRY_CONDITION_INT).GetInt();
            int delta;
            switch (condition)
            {
                case 2:
                case 1:
                    delta = condition;
                    break;
                case -1:
                case -2:
                    delta = condition + 1;
                    break;
                default:
                    return null;
            }

            return Badge.FromDelta(delta);
        }

        public static Badge[] Legend()
        {
            return new[]
            {
                Badge.FromDelta(-1),
                Badge.FromDelta(0),
                Badge.FromDelta(1),
                Badge.FromDelta(2)
            };
        }
        #endregion

        #region MARK: 调试生成
        public static void SpawnSamples()
        {
            SpawnOne(-2);
            SpawnOne(-1);
            SpawnOne(1);
            SpawnOne(2);
        }

        private static void SpawnOne(int condition)
        {
            GameItem item = ModHelper.SpawnItem("wine_berry");
            if (item == null)
            {
                ModEntry.Instance?.Log.Warning("wine_berry spawn failed.");
                return;
            }

            WineHelper.SetBerryCondition(item, condition);
            BadgeSpawn.Place(item);
        }
        #endregion
    }
}
