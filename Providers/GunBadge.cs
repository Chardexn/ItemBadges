namespace ItemBadges
{
    /// <summary>
    /// 枪械成色五档。良好 (Good，属性±0%) 为 0。
    /// 标签档读假标签，真实档读实际成色。
    /// 差 -2 / 一般 -1 / 良好 0 / 极佳 +1 / 新品 +2。
    /// </summary>
    internal static class GunBadge
    {
        public const string Key = "gun";
        public const string Category = "CATEGORY_EQUIPMENT_CONDITION";

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.GunEnabled || item == null || !GunHelper.IsSelfDefenceGun(item))
            {
                return null;
            }

            ItemFeature feature = item.FindItemFeatureByCategory(Category);
            if (feature == null)
            {
                return null;
            }

            ItemCondition condition = BadgeConfig.UseRealPurity ? feature.realCondition : feature.fakeCondition;
            if (condition == null)
            {
                condition = feature.realCondition;
            }

            if (condition == null)
            {
                return null;
            }

            int? delta = DeltaForId(condition.identifier);
            if (!delta.HasValue)
            {
                return null;
            }

            return Badge.FromDelta(delta.Value);
        }

        public static Badge[] Legend()
        {
            return new[]
            {
                Badge.FromDelta(-2),
                Badge.FromDelta(-1),
                Badge.FromDelta(0),
                Badge.FromDelta(1),
                Badge.FromDelta(2)
            };
        }

        internal static int? DeltaForId(string id)
        {
            switch (id)
            {
                case "equipment_condition_poor":
                    return -2;
                case "equipment_condition_fair":
                    return -1;
                case "equipment_condition_good":
                    return 0;
                case "equipment_condition_excellent":
                    return 1;
                case "equipment_condition_new":
                    return 2;
                default:
                    return null;
            }
        }
        #endregion

        #region MARK: 调试生成
        public static void SpawnSamples()
        {
            Spawn(ItemFeatureList.EquipmentCondition.Poor);
            Spawn(ItemFeatureList.EquipmentCondition.Fair);
            Spawn(ItemFeatureList.EquipmentCondition.Good);
            Spawn(ItemFeatureList.EquipmentCondition.Excellent);
            Spawn(ItemFeatureList.EquipmentCondition.New);
        }

        private static void Spawn(ItemFeatureList.EquipmentCondition condition)
        {
            GameItem item = ModHelper.SpawnItem("handmade_pistol");
            if (item == null)
            {
                item = ModHelper.SpawnItem("revolver");
            }

            if (item == null)
            {
                ModEntry.Instance?.Log.Warning("gun spawn failed.");
                return;
            }

            GunHelper.SetEquipmentCondition(item, condition);
            BadgeSpawn.Place(item);
        }
        #endregion
    }
}
