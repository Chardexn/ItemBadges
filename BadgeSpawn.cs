using System;

namespace ItemBadges
{
    /// <summary>
    /// F9 生成：设归属后塞进店内库存（与原版 DebugPanel 相同）。
    /// </summary>
    internal static class BadgeSpawn
    {
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
                ModHelper.SetItemOwnerShip(item, true);
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
        #endregion
    }
}
