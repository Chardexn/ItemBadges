using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ItemBadges
{
    /// <summary>
    /// 在物品实际占用格的角落画角标（叠在图标之上）。
    /// 不规则形状（枪械）贴最近的实心格，不落到包围盒缺口里。
    /// middle 仍是未旋转本地尺寸，要用 globalWidth/Height。
    /// </summary>
    internal static class BadgeRenderer
    {
        private const int InsetPx = 1;

        private static readonly ConditionalWeakTable<GameItemElement, Nodes> NodeTable =
            new ConditionalWeakTable<GameItemElement, Nodes>();

        private static readonly List<WeakReference<GameItemElement>> Tracked =
            new List<WeakReference<GameItemElement>>();

        private static bool loggedError;

        #region MARK: 应用
        public static void Apply(GameItemElement element)
        {
            try
            {
                ApplyUnchecked(element);
            }
            catch (Exception ex)
            {
                if (!loggedError)
                {
                    loggedError = true;
                    ModEntry.Instance?.Log.Error("BadgeRenderer.Apply failed: " + ex);
                }
            }
        }

        public static void RefreshAll()
        {
            ToolAccess.Invalidate();
            PruneTracked();
            for (int i = 0; i < Tracked.Count; i++)
            {
                if (Tracked[i].TryGetTarget(out GameItemElement element) && !element.IsDestroyed())
                {
                    Apply(element);
                }
            }
        }

        public static void RefreshWorld()
        {
            ToolAccess.Invalidate();
            ToolAccess.ForEachItem(ApplyItem);
        }

        public static void ApplyItem(GameItem item)
        {
            if (item is GameItemElement element)
            {
                Apply(element);
            }
        }

        private static void PruneTracked()
        {
            for (int i = Tracked.Count - 1; i >= 0; i--)
            {
                if (!Tracked[i].TryGetTarget(out GameItemElement element) || element.IsDestroyed())
                {
                    Tracked.RemoveAt(i);
                }
            }
        }

        private static void ApplyUnchecked(GameItemElement element)
        {
            if (element == null || element.IsDestroyed() || element.middle == null)
            {
                return;
            }

            Nodes nodes = GetOrCreate(element);
            bool hide = element.isDebugMenu
                || element.background == null
                || element.background.image == null
                || !element.background.image.enabled;

            Badge? resolved = hide ? null : BadgeRegistry.Resolve(element);
            if (!resolved.HasValue)
            {
                Hide(nodes);
                return;
            }

            TreeNodeRender image = EnsureImage(nodes, element.middle);
            LayoutBadge(image, element, resolved.Value);
        }
        #endregion

        #region MARK: 节点
        private static Nodes GetOrCreate(GameItemElement element)
        {
            if (NodeTable.TryGetValue(element, out Nodes existing))
            {
                return existing;
            }

            Nodes created = new Nodes();
            NodeTable.Add(element, created);
            PruneTracked();
            Tracked.Add(new WeakReference<GameItemElement>(element));
            return created;
        }

        private static TreeNodeRender EnsureImage(Nodes nodes, TreeNodeRender parent)
        {
            if (nodes.Image == null)
            {
                TreeNodeRender node = TreeNodeRender.Instantiate().SetParent(parent);
                node.gameObject.name = "ItemBadge";
                node.image.raycastTarget = false;
                node.image.type = Image.Type.Simple;
                node.image.preserveAspect = true;
                nodes.Image = node;
            }
            else if (parent != null && nodes.Image.rectTransform.parent != parent.rectTransform)
            {
                nodes.Image.SetParent(parent);
            }

            return nodes.Image;
        }
        #endregion

        #region MARK: 布局
        private static void LayoutBadge(TreeNodeRender node, GameItemElement element, Badge badge)
        {
            int grid = UISettings.current.gridSize;
            int px = Mathf.Max(1, grid / 16);
            float scale = IsSingleCell(element.modifiedShape) ? 0.6f : 1f;
            Sprite sprite = BadgeSprite.Create(badge, BadgeConfig.Style);
            RectTransform rect = node.rectTransform;
            rect.localEulerAngles = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.SetAsLastSibling();
            rect.sizeDelta = new Vector2(
                sprite.rect.width * px * scale,
                sprite.rect.height * px * scale);
            ApplyVisualCorner(rect, element, px, scale);
            node.image.sprite = sprite;
            node.image.color = Color.white;
            node.image.enabled = true;
        }

        private static void ApplyVisualCorner(RectTransform rect, GameItemElement element, int px, float scale)
        {
            int grid = UISettings.current.gridSize;
            BadgeCorner corner = BadgeConfig.Corner;
            float s = InsetPx * px * scale;
            float cellL = 0f;
            float cellT = 0f;
            GridShape shape = element.modifiedShape;
            if (TryOccupiedCell(shape, corner, out int col, out int row))
            {
                cellL = col * grid;
                cellT = -row * grid;
            }
            else
            {
                float gw = grid;
                float gh = grid;
                if (shape != null)
                {
                    gw = Mathf.Max(grid, shape.globalWidth * grid);
                    gh = Mathf.Max(grid, shape.globalHeight * grid);
                }

                if (corner == BadgeCorner.TopRight || corner == BadgeCorner.BottomRight)
                {
                    cellL = gw - grid;
                }

                if (corner == BadgeCorner.BottomLeft || corner == BadgeCorner.BottomRight)
                {
                    cellT = -gh + grid;
                }
            }

            Vector2 pivot;
            Vector2 pos;
            switch (corner)
            {
                case BadgeCorner.TopLeft:
                    pivot = new Vector2(0f, 1f);
                    pos = new Vector2(cellL + s, cellT - s);
                    break;
                case BadgeCorner.TopRight:
                    pivot = new Vector2(1f, 1f);
                    pos = new Vector2(cellL + grid - s, cellT - s);
                    break;
                case BadgeCorner.BottomLeft:
                    pivot = new Vector2(0f, 0f);
                    pos = new Vector2(cellL + s, cellT - grid + s);
                    break;
                default:
                    pivot = new Vector2(1f, 0f);
                    pos = new Vector2(cellL + grid - s, cellT - grid + s);
                    break;
            }

            rect.pivot = pivot;
            rect.anchoredPosition = pos;
        }

        private static bool TryOccupiedCell(GridShape shape, BadgeCorner corner, out int col, out int row)
        {
            col = 0;
            row = 0;
            if (shape == null)
            {
                return false;
            }

            int minX = shape.minX;
            int minY = shape.minY;
            int maxX = shape.maxX;
            int maxY = shape.maxY;
            bool preferRight = corner == BadgeCorner.TopRight || corner == BadgeCorner.BottomRight;
            bool preferBottom = corner == BadgeCorner.BottomLeft || corner == BadgeCorner.BottomRight;
            int targetX = preferRight ? maxX : minX;
            int targetY = preferBottom ? maxY : minY;

            bool found = false;
            int bestDist = 0;
            int bestTie = 0;
            int bestGx = minX;
            int bestGy = minY;
            for (int gy = minY; gy <= maxY; gy++)
            {
                for (int gx = minX; gx <= maxX; gx++)
                {
                    if (!shape.Intersects(gx, gy))
                    {
                        continue;
                    }

                    int dist = Math.Abs(gx - targetX) + Math.Abs(gy - targetY);
                    int tie = (preferRight ? gx : -gx) + (preferBottom ? gy : -gy);
                    if (!found || dist < bestDist || (dist == bestDist && tie > bestTie))
                    {
                        found = true;
                        bestDist = dist;
                        bestTie = tie;
                        bestGx = gx;
                        bestGy = gy;
                    }
                }
            }

            if (!found)
            {
                return false;
            }

            col = bestGx - minX;
            row = bestGy - minY;
            return true;
        }

        private static bool IsSingleCell(GridShape shape)
        {
            if (shape == null)
            {
                return true;
            }

            if (shape.globalWidth <= 1 && shape.globalHeight <= 1)
            {
                return true;
            }

            int count = 0;
            for (int gy = shape.minY; gy <= shape.maxY; gy++)
            {
                for (int gx = shape.minX; gx <= shape.maxX; gx++)
                {
                    if (!shape.Intersects(gx, gy))
                    {
                        continue;
                    }

                    count++;
                    if (count > 1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void Hide(Nodes nodes)
        {
            if (nodes.Image != null && nodes.Image.image != null)
            {
                nodes.Image.image.enabled = false;
            }
        }
        #endregion

        private sealed class Nodes
        {
            public TreeNodeRender Image;
        }
    }
}
