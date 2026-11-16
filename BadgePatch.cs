using System;
using System.Reflection;
using HarmonyLib;

namespace ItemBadges
{
    /// <summary>
    /// 物品刷新、旋转与拖拽显隐后重绘角标。纯度/净化改写后补一次 Apply。
    /// 拿起/放下检测仪、放大镜，或改走私者暗格时刷新全部。
    /// </summary>
    [HarmonyPatch]
    internal static class ValidateSelectLockedPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(GameItemElement), "ValidateSelectLocked");
        }

        private static void Postfix(GameItemElement __instance)
        {
            ApplyWithTools(__instance);
        }

        internal static void ApplyWithTools(GameItemElement element)
        {
            bool tool = ToolAccess.IsTool(element);
            if (tool)
            {
                ToolAccess.Invalidate();
            }

            BadgeRenderer.Apply(element);
            if (tool)
            {
                BadgeRenderer.RefreshAll();
            }
        }
    }

    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.ToggleBackground))]
    internal static class ToggleBackgroundPatch
    {
        private static void Postfix(GameItemElement __instance)
        {
            ValidateSelectLockedPatch.ApplyWithTools(__instance);
        }
    }

    [HarmonyPatch]
    internal static class ItemRotatePatch
    {
        private static System.Collections.Generic.IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(GameItemElement), nameof(GameItemElement.GridRotCCW));
            yield return AccessTools.Method(typeof(GameItemElement), nameof(GameItemElement.GridRotCW));
            yield return AccessTools.Method(typeof(GameItemElement), nameof(GameItemElement.GridRot180));
            yield return AccessTools.Method(typeof(GameItemElement), nameof(GameItemElement.GridFlipX));
            yield return AccessTools.Method(typeof(GameItemElement), nameof(GameItemElement.GridFlipY));
        }

        private static void Postfix(GameItemElement __instance)
        {
            BadgeRenderer.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(GameSlotInventory), nameof(GameSlotInventory.UncheckedAccept))]
    internal static class ToolboxSlotAcceptPatch
    {
        private static void Postfix(GameSlotInventory __instance, GameItem item)
        {
            BadgeRenderer.ApplyItem(item);
            if (IsToolboxSlot(__instance) || ToolAccess.IsTool(item))
            {
                ToolAccess.Invalidate();
                BadgeRenderer.RefreshAll();
            }
        }

        internal static bool IsToolboxSlot(GameSlotInventory slot)
        {
            string id = slot != null ? slot.identifier : null;
            return id != null && id.StartsWith(ToolboxHelper.TOOLBOX_SLOT_ID_PREFIX);
        }
    }

    [HarmonyPatch(typeof(GameSlotInventory), nameof(GameSlotInventory.Expel))]
    internal static class ToolboxSlotExpelPatch
    {
        private static void Postfix(GameSlotInventory __instance, GameItem item)
        {
            if (ToolboxSlotAcceptPatch.IsToolboxSlot(__instance) || ToolAccess.IsTool(item))
            {
                ToolAccess.Invalidate();
                BadgeRenderer.RefreshAll();
            }
        }
    }

    [HarmonyPatch(typeof(GameGridInventory), nameof(GameGridInventory.UncheckedAccept))]
    internal static class GridAcceptPatch
    {
        private static void Postfix(GameGridInventory __instance, GameItem item, bool __result)
        {
            if (!__result)
            {
                return;
            }

            BadgeRenderer.ApplyItem(item);
            if (ShouldRefresh(__instance, item))
            {
                ToolAccess.Invalidate();
                BadgeRenderer.RefreshAll();
            }
        }

        internal static bool ShouldRefresh(GameInventory inventory, GameItem item)
        {
            if (ToolAccess.IsTool(item) || IsSmugglerInventory(inventory))
            {
                return true;
            }

            return item != null && item.contentWindow != null;
        }

        internal static bool IsSmugglerInventory(GameInventory inventory)
        {
            return inventory != null && inventory.identifier == "smuggler_bay";
        }
    }

    [HarmonyPatch(typeof(GameGridInventory), nameof(GameGridInventory.Expel))]
    internal static class GridExpelPatch
    {
        private static void Postfix(GameGridInventory __instance, GameItem item, bool __result)
        {
            if (__result && GridAcceptPatch.ShouldRefresh(__instance, item))
            {
                ToolAccess.Invalidate();
                BadgeRenderer.RefreshAll();
            }
        }
    }

    [HarmonyPatch(typeof(ChemicalProductHelper), nameof(ChemicalProductHelper.GradeProduct))]
    internal static class GradeProductPatch
    {
        private static void Postfix(GameItem item)
        {
            BadgeRenderer.ApplyItem(item);
        }
    }

    [HarmonyPatch(typeof(ChemicalProductHelper), nameof(ChemicalProductHelper.SetPurityFeature), new Type[] { typeof(GameItem), typeof(ItemCondition), typeof(ItemCondition) })]
    internal static class SetPurityFeaturePatch
    {
        private static void Postfix(GameItem item)
        {
            BadgeRenderer.ApplyItem(item);
        }
    }

    [HarmonyPatch(typeof(MachinePurifier), nameof(MachinePurifier.PurifyContainer))]
    internal static class PurifyContainerPatch
    {
        private static void Postfix(GameItem waterContainer)
        {
            BadgeRenderer.ApplyItem(waterContainer);
        }
    }

    [HarmonyPatch(typeof(WaterFeatureHelper), nameof(WaterFeatureHelper.SetPresumedPurity))]
    internal static class SetPresumedPurityPatch
    {
        private static void Postfix(GameItem gameItem)
        {
            BadgeRenderer.ApplyItem(gameItem);
        }
    }

    [HarmonyPatch(typeof(GunHelper), nameof(GunHelper.SetEquipmentCondition))]
    internal static class SetEquipmentConditionPatch
    {
        private static void Postfix(GameItem item)
        {
            BadgeRenderer.ApplyItem(item);
        }
    }
}
