using System;

namespace ItemBadges
{
    /// <summary>
    /// F9 生成：设归属后塞进店内库存（与原版 DebugPanel 相同）。
    /// </summary>
    internal static class BadgeSpawn
    {
        public const string ShowcaseVanillaTag = "IB_SHOWCASE_VANILLA";
        public const string ShowcaseModTag = "IB_SHOWCASE_MOD";
        public const string ShowcaseGunTag = "IB_SHOWCASE_GUN";

        #region MARK: 放置
        public static void Place(GameItem item)
        {
            if (item == null)
            {
                ModEntry.Instance?.Log.Warning("BadgeSpawn.Place: item is null.");
                return;
            }

            try
            {
                Own(item);
                if (EmporiumEntry.Instance != null)
                {
                    GraphUtils.EmporiumTryAdd(item);
                }
                else
                {
                    ModHelper.MoveItemToInventory(item);
                }

                if (item.parentInventory == null)
                {
                    ModHelper.MoveItemToBackCounter(item);
                }

                if (item.parentInventory == null)
                {
                    ModHelper.MoveItemToInventory(item);
                }

                if (item.parentInventory == null)
                {
                    ModEntry.Instance?.Log.Warning("BadgeSpawn.Place: " + item.identifier + " was not placed.");
                }
                else
                {
                    ToolAccess.Invalidate();
                    BadgeRenderer.RefreshAll();
                }
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Log.Error("BadgeSpawn.Place failed: " + ex);
            }
        }

        public static GameItem Own(GameItem item, string showcaseTag = null)
        {
            if (item == null)
            {
                return null;
            }

            ModHelper.SetItemOwnerShip(item, true);
            if (!string.IsNullOrEmpty(showcaseTag))
            {
                item.EnableTag(showcaseTag);
            }

            return item;
        }

        public static void Detach(GameItem item)
        {
            if (item == null || item.parentInventory == null)
            {
                return;
            }

            try
            {
                item.parentInventory.Expel(item);
            }
            catch (Exception)
            {
            }
        }

        public static bool PlaceInto(GraphNodeStorage target, GameItem item)
        {
            if (item == null)
            {
                return false;
            }

            Own(item);
            Detach(item);
            if (target != null && GraphUtils.TryAcceptAll(target, item) > 0)
            {
                return item.parentInventory != null;
            }

            return false;
        }

        public static void Discard(GameItem item)
        {
            if (item == null)
            {
                return;
            }

            Detach(item);
            try
            {
                item.Destroy();
            }
            catch (Exception)
            {
            }
        }
        #endregion
    }
}
