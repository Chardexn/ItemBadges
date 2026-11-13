using System.Collections.Generic;
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

        private static readonly Dictionary<ColorPreset, Sprite> PreviewCache = new Dictionary<ColorPreset, Sprite>();

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

        #region MARK: 预览
        public static Sprite PreviewSprite(ColorPreset preset)
        {
            if (PreviewCache.TryGetValue(preset, out Sprite cached) && cached != null)
            {
                return cached;
            }

            Color[] swatches = Swatches(preset);
            const int cell = 12;
            const int gap = 3;
            const int border = 1;
            int width = swatches.Length * cell + (swatches.Length - 1) * gap;
            Texture2D texture = new Texture2D(width, cell, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color black = Color.black;
            for (int y = 0; y < cell; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            for (int i = 0; i < swatches.Length; i++)
            {
                int x0 = i * (cell + gap);
                for (int y = 0; y < cell; y++)
                {
                    for (int x = 0; x < cell; x++)
                    {
                        bool edge = x < border || y < border || x >= cell - border || y >= cell - border;
                        texture.SetPixel(x0 + x, y, edge ? black : swatches[i]);
                    }
                }
            }

            texture.Apply(false);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, cell), new Vector2(0.5f, 0.5f), 100f);
            PreviewCache[preset] = sprite;
            return sprite;
        }

        private static Color[] Swatches(ColorPreset preset)
        {
            Color[] plus = preset == ColorPreset.Compact ? CompactPositives : SpectrumPositives;
            Color[] all = new Color[plus.Length + 2];
            all[0] = Palette(RenderHandler.ColorPalette.Red);
            all[1] = Palette(RenderHandler.ColorPalette.Gray);
            for (int i = 0; i < plus.Length; i++)
            {
                all[i + 2] = plus[i];
            }

            return all;
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
