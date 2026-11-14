namespace ItemBadges
{
    /// <summary>
    /// 化学品纯度。街头标准 (Street Standard, 售价±0%) 为 0。
    /// 标签档不需工具；真实档需要成分光谱仪。
    /// 淀粉 -3 / 垃圾 -2 / 掺假 -1 / 街头 0 / 近制药 +1 / 制药 +2 / 实验室 +3。
    /// </summary>
    internal static class ChemBadge
    {
        public const string Key = "chem";
        private const int BaselineGrade = 3;

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.ChemEnabled || item == null || !ChemicalProductHelper.HasChemicalPurity(item))
            {
                return null;
            }

            if (BadgeConfig.UseRealPurity && !ToolAccess.HasChemScanner())
            {
                return null;
            }

            ItemFeature feature = ChemicalProductHelper.GetChemicalFeature(item);
            ItemCondition condition = BadgeConfig.UseRealPurity ? feature?.realCondition : feature?.fakeCondition;
            if (condition == null)
            {
                condition = feature?.realCondition;
            }

            if (condition == null)
            {
                return null;
            }

            int grade = ChemicalProductHelper.GetGradeNumber(condition.identifier);
            if (grade < 0)
            {
                return null;
            }

            return Badge.FromDelta(grade - BaselineGrade);
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
                Badge.FromDelta(3)
            };
        }
        #endregion

        #region MARK: 调试生成
        public static void SpawnSamples()
        {
            Spawn(ItemConditionList.CreateStarchGrade());
            Spawn(ItemConditionList.CreateJunkGrade());
            Spawn(ItemConditionList.CreateCutProduct());
            Spawn(ItemConditionList.CreateStreetStandard());
            Spawn(ItemConditionList.CreateNearPharma());
            Spawn(ItemConditionList.CreatePharmaGrade());
            Spawn(ItemConditionList.CreateLabPure());
        }

        private static void Spawn(ItemCondition condition)
        {
            GameItem item = ModHelper.SpawnItem("caffeine_pill");
            if (item == null)
            {
                ModEntry.Instance?.Log.Warning("caffeine_pill spawn failed.");
                return;
            }

            ChemicalProductHelper.SetPurityFeature(item, condition);
            BadgeSpawn.Place(item);
        }
        #endregion
    }
}
