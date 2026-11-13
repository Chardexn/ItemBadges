using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 颜色预设。0 永远是灰；负值共用红；正值按档位拉伸到绿→…→金。蓝/紫可缺省。
    /// </summary>
    internal enum ColorPreset
    {
        Spectrum = 0,
        Compact = 1
    }

    internal static class BadgePalette
    {
        private static readonly Color32 Gold = new Color32(204, 164, 40, 255);

        private static readonly Color[] SpectrumPositives =
        {
            Palette(RenderHandler.ColorPalette.Green),
            Palette(RenderHandler.ColorPalette.Blue),
            Palette(RenderHandler.ColorPalette.Purple),
            Gold
        };

        private static readonly Color[] CompactPositives =
        {
            Palette(RenderHandler.ColorPalette.Green),
            Gold
        };

        #region MARK: 取值
        public static Color ForDelta(int delta, int maxPositive = 2)
        {
            if (delta < 0)
            {
                return Palette(RenderHandler.ColorPalette.Red);
            }

            if (delta == 0)
            {
                return Palette(RenderHandler.ColorPalette.Gray);
            }

            Color[] plus = Positives();
            if (plus.Length == 1)
            {
                return plus[0];
            }

            int max = Mathf.Max(1, maxPositive);
            int clamped = Mathf.Clamp(delta, 1, max);
            int index = max == 1 ? 0 : (clamped - 1) * (plus.Length - 1) / (max - 1);
            return plus[index];
        }

        public static Color Special()
        {
            return BadgeConfig.Preset == ColorPreset.Spectrum
                ? Palette(RenderHandler.ColorPalette.Purple)
                : Palette(RenderHandler.ColorPalette.Green);
        }
        #endregion

        #region MARK: 预设表
        private static Color[] Positives()
        {
            return BadgeConfig.Preset == ColorPreset.Compact ? CompactPositives : SpectrumPositives;
        }

        private static Color Palette(RenderHandler.ColorPalette palette)
        {
            return RenderHandler.GetColorFromPalette(palette);
        }
        #endregion
    }
}
