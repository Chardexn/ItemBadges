using System;

namespace ItemBadges
{
    /// <summary>
    /// 酿酒拓展原浆：BE_PULP_BERRY_SCORE_SUM_INT。跟葡萄开关，需要放大镜。
    /// 档位按四颗均分后走葡萄贡献；评分显示总分。
    /// </summary>
    internal static class PulpBadge
    {
        public const string Key = "pulp";
        public const string ItemId = "be_brewing_fruit_pulp";
        public const string ScoreSumTag = "BE_PULP_BERRY_SCORE_SUM_INT";

        #region MARK: 注册
        public static void Register()
        {
            BadgeRegistry.Register(Key, TryGet);
        }
        #endregion

        #region MARK: 映射
        public static Badge? TryGet(GameItem item)
        {
            if (!BadgeConfig.BerryEnabled || item == null || item.identifier != ItemId)
            {
                return null;
            }

            if (!item.IsTag(ScoreSumTag) || !ToolAccess.HasMagnifier())
            {
                return null;
            }

            int sum = item.GetTagReadonly(ScoreSumTag).GetInt();
            if (BadgeConfig.WineNumbers == WineNumberMode.Score)
            {
                return new Badge(
                    BadgePalette.ForDelta(ColorDeltaForSum(sum)),
                    Badge.FormatDelta(sum),
                    Badge.ArrowForDelta(ColorDeltaForSum(sum)));
            }

            int avg = (int)Math.Round(sum / 4.0, MidpointRounding.AwayFromZero);
            int? delta = BerryBadge.Contribution(avg);
            if (!delta.HasValue)
            {
                return Badge.FromDelta(avg);
            }

            return Badge.FromDelta(delta.Value);
        }

        private static int ColorDeltaForSum(int sum)
        {
            int avg = (int)Math.Round(sum / 4.0, MidpointRounding.AwayFromZero);
            int? delta = BerryBadge.Contribution(avg);
            return delta ?? avg;
        }
        #endregion
    }
}
