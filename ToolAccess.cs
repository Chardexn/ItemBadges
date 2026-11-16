using System;
using System.Collections.Generic;

namespace ItemBadges
{
    /// <summary>
    /// 真实纯度：葡萄要放大镜、水要检测仪、化学品要成分光谱仪。
    /// 扫描含工具箱格子和走私者暗格等带内容窗的容器。
    /// </summary>
    internal static class ToolAccess
    {
        private static bool cacheValid;
        private static bool hasMagnifier;
        private static bool hasWaterTester;
        private static bool hasChemScanner;

        #region MARK: 查询
        public static bool HasMagnifier()
        {
            Ensure();
            return hasMagnifier;
        }

        public static bool HasWaterTester()
        {
            Ensure();
            return hasWaterTester;
        }

        public static bool HasChemScanner()
        {
            Ensure();
            return hasChemScanner;
        }

        public static bool IsTool(GameItem item)
        {
            return item != null && IsTool(item.identifier, item);
        }

        public static bool IsTool(string identifier)
        {
            return identifier == "magnifier"
                || identifier == "aquascan"
                || identifier == "water_test_strip"
                || identifier == "chem_scanner";
        }

        public static void Invalidate()
        {
            cacheValid = false;
        }

        public static void ForEachItem(Action<GameItem> visit)
        {
            if (visit == null)
            {
                return;
            }

            try
            {
                VisitAll(visit);
                ItemSelectHandler handler = ItemSelectHandler.current;
                GameItem equipped = handler != null ? handler.GetEquippedItem() : null;
                if (equipped != null)
                {
                    visit(equipped);
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Log.Warning("ToolAccess.ForEachItem failed: " + ex.Message);
            }
        }

        public static string Dump()
        {
            Invalidate();
            Ensure();
            return "magnifier=" + hasMagnifier
                + " water=" + hasWaterTester
                + " chem=" + hasChemScanner
                + " toolboxes=" + CountToolboxes();
        }
        #endregion

        #region MARK: 扫描
        private static void Ensure()
        {
            if (cacheValid)
            {
                return;
            }

            bool magnifier = false;
            bool water = false;
            bool chem = false;
            try
            {
                VisitAll(delegate(GameItem item)
                {
                    if (MatchesMagnifier(item))
                    {
                        magnifier = true;
                    }

                    if (Matches(item, "aquascan", true) || Matches(item, "water_test_strip", false))
                    {
                        water = true;
                    }

                    if (Matches(item, "chem_scanner", true))
                    {
                        chem = true;
                    }
                });

                ItemSelectHandler handler = ItemSelectHandler.current;
                GameItem equipped = handler != null ? handler.GetEquippedItem() : null;
                if (MatchesMagnifier(equipped))
                {
                    magnifier = true;
                }

                if (Matches(equipped, "aquascan", true) || Matches(equipped, "water_test_strip", false))
                {
                    water = true;
                }

                if (Matches(equipped, "chem_scanner", true))
                {
                    chem = true;
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Log.Warning("ToolAccess scan failed: " + ex.Message);
                return;
            }

            hasMagnifier = magnifier;
            hasWaterTester = water;
            hasChemScanner = chem;
            cacheValid = true;
        }

        private static void VisitAll(Action<GameItem> visit)
        {
            HashSet<GameItem> seen = new HashSet<GameItem>();
            TryVisitList(SafeFindAllItem(), visit, seen);
            TryVisitList(SafeOwnedItems(), visit, seen);

            EmporiumEntry shop = EmporiumEntry.Instance;
            if (shop != null)
            {
                VisitStorage(shop.invElement, visit, seen);
                VisitStorage(shop.invWindow, visit, seen);
                VisitStorage(shop.backInvinvElement, visit, seen);
                VisitStorage(shop.backInvWindow, visit, seen);
                VisitStorage(shop.backInvinvElementCounter, visit, seen);
                VisitStorage(shop.frontInvinvElement, visit, seen);
                VisitStorage(shop.frontInvWindow, visit, seen);
                VisitStorage(shop.docInvElement, visit, seen);
                VisitStorage(shop.docInvWindow, visit, seen);
                VisitStorage(shop.showcaseElement, visit, seen);
                VisitStorage(shop.showcaseWindow, visit, seen);
                VisitStorage(shop.hiddenElement, visit, seen);
                VisitStorage(shop.hiddenWindow, visit, seen);
                VisitStorage(shop.afterhourWindow, visit, seen);
                VisitStorage(shop.trashInvWindow, visit, seen);
                VisitStorage(shop.vendingFountainWindow, visit, seen);
                VisitStorage(shop.vendingMachineWindow, visit, seen);
            }

            WindowsHandler windows = WindowsHandler.current;
            if (windows != null && windows.visibleWindows != null)
            {
                List<PixelWindow> visible = windows.visibleWindows;
                for (int i = 0; i < visible.Count; i++)
                {
                    VisitStorage(visible[i], visit, seen);
                }
            }
        }

        private static void TryVisitList(List<GameItem> items, Action<GameItem> visit, HashSet<GameItem> seen)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                VisitItem(items[i], visit, seen);
            }
        }

        private static void VisitStorage(GraphNodeStorage root, Action<GameItem> visit, HashSet<GameItem> seen)
        {
            if (root == null)
            {
                return;
            }

            try
            {
                List<GameItem> items = GraphUtils.FindAllChildrenType<GameItem>(root);
                TryVisitList(items, visit, seen);
            }
            catch (Exception)
            {
            }
        }

        private static void VisitItem(GameItem item, Action<GameItem> visit, HashSet<GameItem> seen)
        {
            if (item == null || !seen.Add(item))
            {
                return;
            }

            visit(item);
            if (IsToolbox(item))
            {
                VisitToolbox(item, visit, seen);
                return;
            }

            VisitContent(item, visit, seen);
        }

        private static void VisitContent(GameItem item, Action<GameItem> visit, HashSet<GameItem> seen)
        {
            if (item == null || item.contentWindow == null)
            {
                return;
            }

            try
            {
                TryVisitList(item.FindAllChildItems(), visit, seen);
            }
            catch (Exception)
            {
            }
        }

        private static void VisitToolbox(GameItem box, Action<GameItem> visit, HashSet<GameItem> seen)
        {
            try
            {
                List<GameSlotInventory> slots = ToolboxHelper.GetToolboxSlots(box);
                if (slots != null)
                {
                    for (int i = 0; i < slots.Count; i++)
                    {
                        GameSlotInventory slot = slots[i];
                        if (slot == null)
                        {
                            continue;
                        }

                        VisitItem(slot.childItem, visit, seen);
                        List<GameItem> children = slot.childItems;
                        if (children == null)
                        {
                            continue;
                        }

                        for (int c = 0; c < children.Count; c++)
                        {
                            VisitItem(children[c], visit, seen);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Log.Warning("toolbox slots failed: " + BoxName(box) + " " + ex.Message);
            }

            try
            {
                PixelWindow window = box.contentWindow;
                if (window != null)
                {
                    List<GameItem> nested = GraphUtils.FindAllChildrenType<GameItem>(window);
                    TryVisitList(nested, visit, seen);
                    List<GameSlotInventory> windowSlots =
                        GraphUtils.FindAllChildrenType(window, (GameSlotInventory s) => true);
                    if (windowSlots != null)
                    {
                        for (int i = 0; i < windowSlots.Count; i++)
                        {
                            GameSlotInventory slot = windowSlots[i];
                            if (slot != null)
                            {
                                VisitItem(slot.childItem, visit, seen);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Log.Warning("toolbox window failed: " + BoxName(box) + " " + ex.Message);
            }
        }

        private static bool IsToolbox(GameItem item)
        {
            if (item == null)
            {
                return false;
            }

            return item.identifier == "toolbox" || item.IsTag(ToolboxHelper.TOOLBOX_TAG);
        }

        private static bool MatchesMagnifier(GameItem item)
        {
            if (item == null)
            {
                return false;
            }

            return item.identifier == "magnifier" || item.IsTag(ToolDirectory.MAGNIFYING_GLASS);
        }

        private static bool Matches(GameItem item, string id, bool spentOk)
        {
            if (item == null || item.identifier != id)
            {
                return false;
            }

            return spentOk || !item.IsTag("IS_SPENT");
        }

        private static bool IsTool(string identifier, GameItem item)
        {
            return IsTool(identifier) || MatchesMagnifier(item);
        }

        private static List<GameItem> SafeFindAllItem()
        {
            try
            {
                return PlayerStore.Instance != null ? PlayerStore.Instance.FindAllItem() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static List<GameItem> SafeOwnedItems()
        {
            try
            {
                return EmporiumEntry.Instance != null ? EmporiumEntry.Instance.GetAllOwnedItems() : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static int CountToolboxes()
        {
            int count = 0;
            HashSet<GameItem> seen = new HashSet<GameItem>();
            TryVisitList(SafeFindAllItem(), item =>
            {
                if (IsToolbox(item))
                {
                    count++;
                }
            }, seen);
            TryVisitList(SafeOwnedItems(), item =>
            {
                if (IsToolbox(item))
                {
                    count++;
                }
            }, seen);
            return count;
        }

        private static string BoxName(GameItem box)
        {
            if (box == null)
            {
                return "?";
            }

            return string.IsNullOrEmpty(box.name) ? box.identifier : box.name;
        }
        #endregion
    }
}
