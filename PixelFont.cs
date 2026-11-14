using System.Collections.Generic;

namespace ItemBadges
{
    /// <summary>
    /// 3×5（∞ 为 5×5）点阵字，用于角标数字和箭头。
    /// </summary>
    internal static class PixelFont
    {
        public const int Height = 5;
        public const int Gap = 1;

        private static readonly Dictionary<char, byte[]> Glyphs = Build();

        #region MARK: 度量
        public static int WidthOf(char c)
        {
            byte[] rows = Find(c);
            return rows == null ? 3 : RowWidth(rows[0]);
        }

        public static int Measure(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int width = 0;
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0)
                {
                    width += Gap;
                }

                width += WidthOf(text[i]);
            }

            return width;
        }

        public static bool Pixel(char c, int x, int y)
        {
            byte[] rows = Find(c);
            if (rows == null || y < 0 || y >= Height)
            {
                return false;
            }

            int width = RowWidth(rows[y]);
            if (x < 0 || x >= width)
            {
                return false;
            }

            return (rows[y] & (1 << (width - 1 - x))) != 0;
        }
        #endregion

        #region MARK: 字库
        private static byte[] Find(char c)
        {
            if (Glyphs.TryGetValue(c, out byte[] rows))
            {
                return rows;
            }

            if (c == '\u221e')
            {
                return Glyphs['i'];
            }

            return Glyphs.TryGetValue('?', out rows) ? rows : null;
        }

        private static int RowWidth(byte row)
        {
            return (row & 0x80) != 0 ? 5 : 3;
        }

        private static Dictionary<char, byte[]> Build()
        {
            Dictionary<char, byte[]> map = new Dictionary<char, byte[]>();
            Add(map, '0', 0x07, 0x05, 0x05, 0x05, 0x07);
            Add(map, '1', 0x02, 0x06, 0x02, 0x02, 0x07);
            Add(map, '2', 0x07, 0x01, 0x07, 0x04, 0x07);
            Add(map, '3', 0x07, 0x01, 0x07, 0x01, 0x07);
            Add(map, '4', 0x05, 0x05, 0x07, 0x01, 0x01);
            Add(map, '5', 0x07, 0x04, 0x07, 0x01, 0x07);
            Add(map, '6', 0x07, 0x04, 0x07, 0x05, 0x07);
            Add(map, '7', 0x07, 0x01, 0x01, 0x01, 0x01);
            Add(map, '8', 0x07, 0x05, 0x07, 0x05, 0x07);
            Add(map, '9', 0x07, 0x05, 0x07, 0x01, 0x07);
            Add(map, '+', 0x00, 0x02, 0x07, 0x02, 0x00);
            Add(map, '-', 0x00, 0x00, 0x07, 0x00, 0x00);
            AddWide(map, '^', 0x00, 0x04, 0x0A, 0x11, 0x00);
            AddWide(map, 'v', 0x00, 0x11, 0x0A, 0x04, 0x00);
            Add(map, '!', 0x02, 0x02, 0x02, 0x00, 0x02);
            Add(map, 'x', 0x05, 0x02, 0x07, 0x02, 0x05);
            Add(map, '?', 0x07, 0x01, 0x02, 0x00, 0x02);
            AddWide(map, 'i', 0x0A, 0x15, 0x15, 0x0A, 0x00);
            return map;
        }

        private static void Add(Dictionary<char, byte[]> map, char c, byte r0, byte r1, byte r2, byte r3, byte r4)
        {
            map[c] = new[] { r0, r1, r2, r3, r4 };
        }

        private static void AddWide(Dictionary<char, byte[]> map, char c, byte r0, byte r1, byte r2, byte r3, byte r4)
        {
            map[c] = new[]
            {
                (byte)(r0 | 0x80),
                (byte)(r1 | 0x80),
                (byte)(r2 | 0x80),
                (byte)(r3 | 0x80),
                (byte)(r4 | 0x80)
            };
        }
        #endregion
    }
}
