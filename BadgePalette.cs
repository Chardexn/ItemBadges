using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 三种配色，各 7 色，正中为 0。紫给特殊物品。
    /// </summary>
    internal static class BadgePalette
    {
        public static readonly Color Black = new Color32(20, 18, 24, 255);
        public static readonly Color Red = new Color32(224, 80, 80, 255);
        public static readonly Color DarkGray = new Color32(96, 100, 108, 255);
        public static readonly Color MidGray = new Color32(152, 156, 160, 255);
        public static readonly Color White = new Color32(220, 216, 200, 255);
        public static readonly Color Yellow = new Color32(240, 196, 72, 255);
        public static readonly Color Green = new Color32(80, 208, 120, 255);
        public static readonly Color Blue = new Color32(72, 156, 232, 255);
        public static readonly Color Purple = new Color32(192, 140, 248, 255);
        public static readonly Color Gold = new Color32(232, 180, 48, 255);
        public static readonly Color InkDark = new Color32(24, 20, 28, 255);
        public static readonly Color InkLight = new Color32(236, 232, 220, 255);

        public static ColorScheme? PreviewOverride;

        public static ColorScheme Scheme =>
            PreviewOverride ?? (BadgeConfig.SchemeEntry != null
                ? BadgeConfig.SchemeEntry.Value
                : ColorScheme.YellowBlueGreen);

        #region MARK: 取值
        public static Color ForDelta(int delta, ColorScheme? scheme = null)
        {
            Color[] ramp = Ramp(scheme ?? Scheme);
            int index = 3 + delta;
            if (index < 0)
            {
                index = 0;
            }
            else if (index >= ramp.Length)
            {
                index = ramp.Length - 1;
            }

            return ramp[index];
        }

        public static Color Special()
        {
            return Purple;
        }

        public static int[] ScaleDeltas()
        {
            return new[] { -3, -2, -1, 0, 1, 2, 3 };
        }

        public static Color InkFor(Color fill)
        {
            return Luminance(fill) < 0.38f ? InkLight : InkDark;
        }

        public static Color BorderFor(Color fill)
        {
            return InkDark;
        }

        private static Color[] Ramp(ColorScheme scheme)
        {
            switch (scheme)
            {
                case ColorScheme.GreenBlueYellow:
                    return new[] { Black, Red, DarkGray, White, Green, Blue, Yellow };
                case ColorScheme.Rarity:
                    return new[] { MidGray, White, Green, Blue, Purple, Gold, Red };
                default:
                    return new[] { Black, Red, DarkGray, White, Yellow, Blue, Green };
            }
        }

        private static float Luminance(Color c)
        {
            return 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;
        }
        #endregion
    }
}
