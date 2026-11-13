using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 在 GameItemElement.middle（不随物品旋转）右下角画出色块和/或数字。
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
            Badge badge = resolved.Value;
            BadgeStyle style = BadgeConfig.Style;
            bool showBlock = style == BadgeStyle.ColorBlock || style == BadgeStyle.Both;
            bool showNumber = (style == BadgeStyle.Number || style == BadgeStyle.Both)
                && !string.IsNullOrEmpty(badge.Text);

            int px = Mathf.Max(1, UISettings.current.gridSize / 16);
            if (showBlock)
            {
                LayoutSwatch(
                    EnsureBorder(nodes, element.middle),
                    EnsureBlock(nodes, element.middle),
                    badge.Color,
                    px);
            }
            else
            {
                HideImage(nodes.Border);
                HideImage(nodes.Block);
            }

            if (showNumber)
            {
                LayoutNumber(
                    EnsureShadow(nodes, element.middle),
                    EnsureText(nodes, element.middle),
                    badge,
                    px,
                    showBlock);
            }
            else
            {
                HideText(nodes.Shadow);
                HideText(nodes.Text);
            }
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

        private static TreeNodeRender EnsureBorder(Nodes nodes, TreeNodeRender parent)
        {
            if (nodes.Border == null)
            {
                nodes.Border = CreateBlockNode(parent, "ItemBadgeBorder");
            }

            return nodes.Border;
        }

        private static TreeNodeRender EnsureBlock(Nodes nodes, TreeNodeRender parent)
        {
            if (nodes.Block == null)
            {
                nodes.Block = CreateBlockNode(parent, "ItemBadgeBlock");
            }

            return nodes.Block;
        }

        private static TreeNodeRender CreateBlockNode(TreeNodeRender parent, string name)
        {
            TreeNodeRender node = TreeNodeRender.Instantiate().SetParent(parent);
            node.gameObject.name = name;
            node.rectTransform.anchorMin = new Vector2(1f, 0f);
            node.rectTransform.anchorMax = new Vector2(1f, 0f);
            node.rectTransform.pivot = new Vector2(1f, 0f);
            node.image.sprite = null;
            node.image.raycastTarget = false;
            return node;
        }

        private static TreeNodeRender EnsureShadow(Nodes nodes, TreeNodeRender parent)
        {
            if (nodes.Shadow == null)
            {
                nodes.Shadow = CreateTextNode(parent, "ItemBadgeTextShadow");
                nodes.Shadow.text.fontStyle = FontStyles.Bold;
                nodes.Shadow.text.color = RenderHandler.GetColorFromPalette(RenderHandler.ColorPalette.VeryDarkGray);
            }

            return nodes.Shadow;
        }

        private static TreeNodeRender EnsureText(Nodes nodes, TreeNodeRender parent)
        {
            if (nodes.Text == null)
            {
                nodes.Text = CreateTextNode(parent, "ItemBadgeText");
                nodes.Text.text.fontStyle = FontStyles.Normal;
            }

            return nodes.Text;
        }

        private static TreeNodeRender CreateTextNode(TreeNodeRender parent, string name)
        {
            TreeNodeRender node = TreeNodeRender.InstantiateText().SetParent(parent);
            node.gameObject.name = name;
            node.rectTransform.anchorMin = new Vector2(1f, 0f);
            node.rectTransform.anchorMax = new Vector2(1f, 0f);
            node.rectTransform.pivot = new Vector2(1f, 0f);
            if (node.image != null)
            {
                node.image.enabled = false;
                node.image.raycastTarget = false;
            }

            node.text.raycastTarget = false;
            node.text.alignment = TextAlignmentOptions.BottomRight;
            node.text.enableWordWrapping = false;
            node.text.overflowMode = TextOverflowModes.Overflow;
            return node;
        }
        #endregion

        #region MARK: 布局
        private static void LayoutSwatch(TreeNodeRender border, TreeNodeRender block, Color color, int px)
        {
            int inner = Mathf.Max(1, BadgeConfig.BlockSize) * px;
            int borderW = px;
            int outer = inner + 2 * borderW;
            int inset = InsetPx * px;
            Vector2 outerPos = new Vector2(-inset, inset);
            Vector2 innerPos = new Vector2(-inset - borderW, inset + borderW);

            PlaceBlock(border, Color.black, outer, outerPos, visible: true);
            PlaceBlock(block, color, inner, innerPos, visible: true);
        }

        private static void PlaceBlock(TreeNodeRender node, Color color, int size, Vector2 pos, bool visible)
        {
            if (node == null)
            {
                return;
            }

            node.rectTransform.SetAsLastSibling();
            node.rectTransform.sizeDelta = new Vector2(size, size);
            node.rectTransform.anchoredPosition = pos;
            node.image.color = color;
            node.image.enabled = visible;
        }

        private static void LayoutNumber(
            TreeNodeRender shadow,
            TreeNodeRender text,
            Badge badge,
            int px,
            bool shiftLeftForBlock)
        {
            int inner = Mathf.Max(1, BadgeConfig.BlockSize) * px;
            int borderW = px;
            float fontSize = Mathf.Max(1f, BadgeConfig.FontSize * px);
            float extraLeft = 0f;
            if (shiftLeftForBlock)
            {
                extraLeft = inner + 2 * borderW + InsetPx * px;
            }

            Vector2 pos = new Vector2(-InsetPx * px - extraLeft, InsetPx * px);
            Vector2 size = new Vector2(fontSize * 2.6f, fontSize * 1.5f);

            PlaceText(shadow, badge.Text, fontSize, size, pos + new Vector2(1f, -1f), visible: true);
            if (shadow != null)
            {
                shadow.rectTransform.SetAsLastSibling();
            }

            PlaceText(text, badge.Text, fontSize, size, pos, visible: true);
            if (text != null)
            {
                text.text.color = badge.Color;
                text.rectTransform.SetAsLastSibling();
            }
        }

        private static void PlaceText(
            TreeNodeRender node,
            string content,
            float fontSize,
            Vector2 size,
            Vector2 pos,
            bool visible)
        {
            if (node == null || node.text == null)
            {
                return;
            }

            node.rectTransform.sizeDelta = size;
            node.rectTransform.anchoredPosition = pos;
            node.text.fontSize = fontSize;
            node.text.text = content;
            node.text.enabled = visible;
        }

        private static void Hide(Nodes nodes)
        {
            HideImage(nodes.Border);
            HideImage(nodes.Block);
            HideText(nodes.Shadow);
            HideText(nodes.Text);
        }

        private static void HideImage(TreeNodeRender node)
        {
            if (node != null && node.image != null)
            {
                node.image.enabled = false;
            }
        }

        private static void HideText(TreeNodeRender node)
        {
            if (node != null && node.text != null)
            {
                node.text.enabled = false;
            }
        }
        #endregion

        private sealed class Nodes
        {
            public TreeNodeRender Border;
            public TreeNodeRender Block;
            public TreeNodeRender Shadow;
            public TreeNodeRender Text;
        }
    }
}
