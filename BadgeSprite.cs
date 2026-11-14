using System.Collections.Generic;
using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 把像素字画进带描边的色块，生成点采样 Sprite。
    /// </summary>
    internal static class BadgeSprite
    {
        public const int Border = 1;
        public const int Pad = 1;
        public const int PreviewScale = 2;
        public const int StripGap = 3;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        #region MARK: 单枚
        public static Sprite Create(Badge badge, BadgeStyle style, int scale = 1)
        {
            return Create(badge.Glyph(style), badge.Color, scale);
        }

        public static Sprite Create(string text, Color fill, int scale = 1)
        {
            if (scale < 1)
            {
                scale = 1;
            }

            bool empty = string.IsNullOrEmpty(text);
            string key = scale + ":" + Color32ToHex(fill) + ":" + (empty ? "_" : text);
            if (Cache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

            int glyphW = empty ? 3 : PixelFont.Measure(text);
            int srcW = Border * 2 + Pad * 2 + glyphW;
            int srcH = Border * 2 + Pad * 2 + PixelFont.Height;
            int dstW = srcW * scale;
            int dstH = srcH * scale;

            Texture2D texture = new Texture2D(dstW, dstH, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            Color ink = BadgePalette.InkFor(fill);
            Color border = BadgePalette.BorderFor(fill);
            string glyphs = empty ? "" : text;
            for (int y = 0; y < srcH; y++)
            {
                for (int x = 0; x < srcW; x++)
                {
                    Color pixel = Sample(glyphs, glyphW, x, y, fill, ink, border);
                    Blit(texture, x, y, scale, pixel);
                }
            }

            texture.Apply(false);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, dstW, dstH),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.hideFlags = HideFlags.DontSave;
            Cache[key] = sprite;
            return sprite;
        }
        #endregion

        #region MARK: 色带
        public static Sprite Strip(Badge[] badges, BadgeStyle style, int scale = PreviewScale)
        {
            if (badges == null || badges.Length == 0)
            {
                return Create("", BadgePalette.White, scale);
            }

            string stripKey = "strip:" + scale + ":" + (int)style + ":";
            for (int i = 0; i < badges.Length; i++)
            {
                stripKey += Color32ToHex(badges[i].Color) + badges[i].Glyph(style) + ",";
            }

            if (Cache.TryGetValue(stripKey, out Sprite cachedStrip) && cachedStrip != null)
            {
                return cachedStrip;
            }

            Sprite[] parts = new Sprite[badges.Length];
            int width = 0;
            int height = 0;
            for (int i = 0; i < badges.Length; i++)
            {
                parts[i] = Create(badges[i], style, scale);
                width += (int)parts[i].rect.width;
                if (i > 0)
                {
                    width += StripGap * scale;
                }

                int h = (int)parts[i].rect.height;
                if (h > height)
                {
                    height = h;
                }
            }

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }

            int x0 = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                CopySprite(parts[i], texture, x0, (height - (int)parts[i].rect.height) / 2);
                x0 += (int)parts[i].rect.width + StripGap * scale;
            }

            texture.Apply(false);
            Sprite strip = Sprite.Create(
                texture,
                new Rect(0f, 0f, width, height),
                new Vector2(0.5f, 0.5f),
                100f);
            strip.hideFlags = HideFlags.DontSave;
            Cache[stripKey] = strip;
            return strip;
        }
        #endregion

        #region MARK: 绘制
        private static Color Sample(string text, int glyphW, int x, int y, Color fill, Color ink, Color border)
        {
            if (x < Border || y < Border || x >= Border * 2 + Pad * 2 + glyphW - Border
                || y >= Border * 2 + Pad * 2 + PixelFont.Height - Border)
            {
                return border;
            }

            if (string.IsNullOrEmpty(text))
            {
                return fill;
            }

            int gx = x - Border - Pad;
            int gyTop = (Border + Pad + PixelFont.Height - 1) - y;
            if (gx < 0 || gx >= glyphW || gyTop < 0 || gyTop >= PixelFont.Height)
            {
                return fill;
            }

            int cursor = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0)
                {
                    cursor += PixelFont.Gap;
                }

                int cw = PixelFont.WidthOf(text[i]);
                if (gx < cursor + cw)
                {
                    return PixelFont.Pixel(text[i], gx - cursor, gyTop) ? ink : fill;
                }

                cursor += cw;
            }

            return fill;
        }

        private static void Blit(Texture2D texture, int srcX, int srcY, int scale, Color color)
        {
            int x0 = srcX * scale;
            int y0 = srcY * scale;
            for (int oy = 0; oy < scale; oy++)
            {
                for (int ox = 0; ox < scale; ox++)
                {
                    texture.SetPixel(x0 + ox, y0 + oy, color);
                }
            }
        }

        private static void CopySprite(Sprite sprite, Texture2D dest, int x0, int y0)
        {
            Texture2D src = sprite.texture;
            int w = (int)sprite.rect.width;
            int h = (int)sprite.rect.height;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    dest.SetPixel(x0 + x, y0 + y, src.GetPixel(x, y));
                }
            }
        }

        private static string Color32ToHex(Color color)
        {
            Color32 c = color;
            return c.r.ToString("x2") + c.g.ToString("x2") + c.b.ToString("x2");
        }
        #endregion
    }
}
