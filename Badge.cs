using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 角标数据。provider 返回 null 表示该物品不显示角标。
    /// </summary>
    internal readonly struct Badge
    {
        public readonly Color Color;
        public readonly string Number;
        public readonly string Arrow;

        public Badge(Color color, string number, string arrow)
        {
            Color = color;
            Number = number ?? "";
            Arrow = string.IsNullOrEmpty(arrow) ? "-" : arrow;
        }

        public string Glyph(BadgeStyle style)
        {
            switch (style)
            {
                case BadgeStyle.Number:
                    return Number;
                case BadgeStyle.Arrow:
                    return Arrow;
                default:
                    return "";
            }
        }

        public static string FormatDelta(int delta)
        {
            if (delta > 0)
            {
                return "+" + delta;
            }

            if (delta == 0)
            {
                return "+0";
            }

            return delta.ToString();
        }

        public static string ArrowForDelta(int delta)
        {
            if (delta > 0)
            {
                return "^";
            }

            if (delta < 0)
            {
                return "v";
            }

            return "-";
        }

        public static Badge FromDelta(int delta, Color? color = null)
        {
            Color fill = color ?? BadgePalette.ForDelta(delta);
            return new Badge(fill, FormatDelta(delta), ArrowForDelta(delta));
        }
    }
}
