using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ItemBadges
{
    /// <summary>
    /// 在物品旋转后的视觉包围盒角落画角标（叠在图标之上）。
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

            bool hide = element.isDebugMenu
                || element.background == null
                || element.background.image == null
                || !element.background.image.enabled;

            Badge? resolved = hide ? null : BadgeRegistry.Resolve(element);
            if (!resolved.HasValue)
            {
                if (NodeTable.TryGetValue(element, out Nodes existing))
                {
                    Hide(existing);
                }

                return;
            }

            Nodes nodes = GetOrCreate(element);
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
            int px = Mathf.Max(1, UISettings.current.gridSize / 16);
            Sprite sprite = BadgeSprite.Create(badge, BadgeConfig.Style);
            RectTransform rect = node.rectTransform;
            rect.localEulerAngles = Vector3.zero;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.SetAsLastSibling();
            rect.sizeDelta = new Vector2(
                sprite.rect.width * px,
                sprite.rect.height * px);
            ApplyVisualCorner(rect, element, px);
            node.image.sprite = sprite;
            node.image.color = Color.white;
            node.image.enabled = true;
        }

        private static void ApplyVisualCorner(RectTransform rect, GameItemElement element, int px)
        {
            int grid = UISettings.current.gridSize;
            GridShape shape = element.modifiedShape;
            float gw = grid;
            float gh = grid;
            if (shape != null)
            {
                gw = Mathf.Max(grid, shape.globalWidth * grid);
                gh = Mathf.Max(grid, shape.globalHeight * grid);
            }

            float s = InsetPx * px;
            Vector2 pivot;
            Vector2 pos;
            switch (BadgeConfig.Corner)
            {
                case BadgeCorner.TopLeft:
                    pivot = new Vector2(0f, 1f);
                    pos = new Vector2(s, -s);
                    break;
                case BadgeCorner.TopRight:
                    pivot = new Vector2(1f, 1f);
                    pos = new Vector2(gw - s, -s);
                    break;
                case BadgeCorner.BottomLeft:
                    pivot = new Vector2(0f, 0f);
                    pos = new Vector2(s, -gh + s);
                    break;
                default:
                    pivot = new Vector2(1f, 0f);
                    pos = new Vector2(gw - s, -gh + s);
                    break;
            }

            rect.pivot = pivot;
            rect.anchoredPosition = pos;
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
