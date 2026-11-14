namespace ItemBadges
{
    /// <summary>
    /// 金属锭纯度。平均 (Fair, 售价±0%) 为 0。
    /// 标签档读假标签，真实档读实际纯度。
    /// 极低 -2 / 低 -1 / 平均 0 / 高 +1 / 极高 +2 / 完美 +3。
    /// </summary>
    internal static class IngotBadge
    {
        public const string Key = "ingot";
        private const int BaselineLevel = 7;

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.IngotEnabled || item == null || !item.IsTag(IngotPurityHelper.INGOT_PURITY_TAG))
            {
                return null;
            }

            string purity = BadgeConfig.UseRealPurity
                ? IngotPurityHelper.GetPurity(item)
                : IngotPurityHelper.GetFakePurity(item);
            if (string.IsNullOrEmpty(purity))
            {
                purity = IngotPurityHelper.GetPurity(item);
            }

            if (string.IsNullOrEmpty(purity)
                || !IngotPurityHelper.ingotPurities.TryGetValue(purity, out IngotPurityHelper.IngotPurity data))
            {
                return null;
            }

            return Badge.FromDelta(data.PurityLevel - BaselineLevel);
        }

        public static Badge[] Legend()
        {
            return new[]
            {
                Badge.FromDelta(-2),
                Badge.FromDelta(-1),
                Badge.FromDelta(0),
                Badge.FromDelta(1),
                Badge.FromDelta(2),
                Badge.FromDelta(3)
            };
        }
        #endregion

        #region MARK: 调试生成
        public static void SpawnSamples()
        {
            Spawn(IngotPurityHelper.INGOT_PURITY_VERY_LOW);
            Spawn(IngotPurityHelper.INGOT_PURITY_LOW);
            Spawn(IngotPurityHelper.INGOT_PURITY_FAIR);
            Spawn(IngotPurityHelper.INGOT_PURITY_HIGH);
            Spawn(IngotPurityHelper.INGOT_PURITY_VERY_HIGH);
            Spawn(IngotPurityHelper.INGOT_PURITY_PERFECT);
        }

        private static void Spawn(string purity)
        {
            GameItem item = ModHelper.SpawnItem("metal_ingot");
            if (item == null)
            {
                ModEntry.Instance?.Log.Warning("metal_ingot spawn failed.");
                return;
            }

            IngotPurityHelper.SetPurity(item, purity);
            BadgeSpawn.Place(item);
        }
        #endregion
    }
}
