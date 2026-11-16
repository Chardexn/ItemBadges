using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 酵母：数字为酿酒值 +1 或发酵天数；颜色按品级。非法红色。不需要工具。
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

            return Make(GradeColor(item), YeastPower(item), BadgeConfig.YeastNumbers);
        }

        public static Badge[] Legend(YeastNumberMode? mode = null)
        {
            YeastNumberMode use = mode ?? BadgeConfig.YeastNumbers;
            return new[]
            {
                Make(BadgePalette.ForDelta(1), 2, use),
                Make(BadgePalette.ForDelta(2), 4, use),
                Make(BadgePalette.Special(), 4, use),
                Make(BadgePalette.Red, 2, use)
            };
        }

        private static Badge Make(Color color, int power, YeastNumberMode mode)
        {
            if (mode == YeastNumberMode.FermentDays)
            {
                int days = FermentDays(power);
                return new Badge(color, days.ToString(), Badge.ArrowForDelta(2 - days));
            }

            return new Badge(color, Badge.FormatDelta(1), Badge.ArrowForDelta(1));
        }

        private static Color GradeColor(GameItem item)
        {
            if (item.IsTag(WineHelper.WINE_YEAST_ILLEGAL_TAG))
            {
                return BadgePalette.Red;
            }

            if (item.IsTag(WineHelper.WINE_YEAST_REUSABLE_TAG))
            {
                return BadgePalette.Special();
            }

            if (item.identifier == "wine_superyeast" || YeastPower(item) >= 4)
            {
                return BadgePalette.ForDelta(2);
            }

            return BadgePalette.ForDelta(1);
        }

        private static int YeastPower(GameItem item)
        {
            int power = item.GetTagReadonly(WineHelper.WINE_YEAST_POWER_INT).GetInt();
            if (power < 1 || power > 8)
            {
                return 2;
            }

            return power;
        }

        private static int FermentDays(int power)
        {
            int use = power < 1 ? 2 : power;
            int days = (WineHelper.BASE_FERMENTATION_PERIOD + use - 1) / use;
            return days < 1 ? 1 : days;
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

            BadgeSpawn.Place(item);
        }
        #endregion
    }
}
