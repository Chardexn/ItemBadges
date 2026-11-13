using System.Reflection;
using HarmonyLib;

namespace ItemBadges
{
    /// <summary>
    /// 物品刷新与拖拽显隐后重绘角标。
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
            BadgeRenderer.Apply(__instance);
        }
    }

    [HarmonyPatch(typeof(GameItemElement), nameof(GameItemElement.ToggleBackground))]
    internal static class ToggleBackgroundPatch
    {
        private static void Postfix(GameItemElement __instance)
        {
            BadgeRenderer.Apply(__instance);
        }
    }
}
