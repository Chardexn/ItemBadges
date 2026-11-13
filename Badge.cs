using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 角标数据。provider 返回 null 表示该物品不显示角标。
    /// </summary>
    internal readonly struct Badge
    {
        public readonly Color Color;
        public readonly string Text;

        public Badge(Color color, string text)
        {
            Color = color;
            Text = text ?? "";
        }
    }
}
