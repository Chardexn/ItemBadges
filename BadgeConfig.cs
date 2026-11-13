using UnityEngine;

namespace ItemBadges
{
    internal enum BadgeStyle
    {
        ColorBlock = 0,
        Number = 1,
        Both = 2
    }

    /// <summary>
    /// 模组配置。Define 在 Init 调用；标签用延迟 Func，打开面板时才读本地化。
    /// </summary>
    internal static class BadgeConfig
    {
        public const int BlockSizeMin = 2;
        public const int BlockSizeMax = 12;
        public const int FontSizeMin = 6;
        public const int FontSizeMax = 18;

        public static ConfigEntry<BadgeStyle> StyleEntry { get; private set; }
        public static ConfigEntry<ColorPreset> PresetEntry { get; private set; }
        public static ConfigEntry<int> BlockSizeEntry { get; private set; }
        public static ConfigEntry<int> FontSizeEntry { get; private set; }
        public static ConfigEntry<bool> BerryEnabledEntry { get; private set; }
        public static ConfigEntry<bool> YeastEnabledEntry { get; private set; }
        public static ConfigEntry<bool> WaterEnabledEntry { get; private set; }

        public static BadgeStyle Style => StyleEntry != null ? StyleEntry.Value : BadgeStyle.ColorBlock;
        public static ColorPreset Preset => PresetEntry != null ? PresetEntry.Value : ColorPreset.Spectrum;
        public static int BlockSize => Clamp(BlockSizeEntry, 4, BlockSizeMin, BlockSizeMax);
        public static int FontSize => Clamp(FontSizeEntry, 9, FontSizeMin, FontSizeMax);
        public static bool BerryEnabled => BerryEnabledEntry == null || BerryEnabledEntry.Value;
        public static bool YeastEnabled => YeastEnabledEntry == null || YeastEnabledEntry.Value;
        public static bool WaterEnabled => WaterEnabledEntry == null || WaterEnabledEntry.Value;

        #region MARK: 定义
        public static void Define()
        {
            ModConfig cfg = ModHelper.GetConfig(ModEntry.MOD_ID);
            const string section = "常规";

            StyleEntry = Bind(cfg.DefineEnum(
                "style",
                BadgeStyle.ColorBlock,
                section,
                () => ModEntry.L("cfg_style"),
                () => ModEntry.L("cfg_style_tip")));

            PresetEntry = Bind(cfg.DefineEnum(
                "color_preset",
                ColorPreset.Spectrum,
                section,
                () => ModEntry.L("cfg_preset"),
                () => ModEntry.L("cfg_preset_tip")));

            BlockSizeEntry = BindSize(
                cfg.Define(
                    "block_size",
                    4,
                    section,
                    () => ModEntry.L("cfg_block_size"),
                    tooltip: () => ModEntry.L("cfg_block_size_tip")),
                BlockSizeMin,
                BlockSizeMax);

            FontSizeEntry = BindSize(
                cfg.Define(
                    "font_size",
                    9,
                    section,
                    () => ModEntry.L("cfg_font_size"),
                    tooltip: () => ModEntry.L("cfg_font_size_tip")),
                FontSizeMin,
                FontSizeMax);

            BerryEnabledEntry = Bind(cfg.Define(
                "berry_enabled",
                true,
                section,
                () => ModEntry.L("cfg_berry"),
                () => ModEntry.L("cfg_berry_tip")));

            YeastEnabledEntry = Bind(cfg.Define(
                "yeast_enabled",
                true,
                section,
                () => ModEntry.L("cfg_yeast"),
                () => ModEntry.L("cfg_yeast_tip")));

            WaterEnabledEntry = Bind(cfg.Define(
                "water_enabled",
                true,
                section,
                () => ModEntry.L("cfg_water"),
                () => ModEntry.L("cfg_water_tip")));
        }
        #endregion

        #region MARK: 刷新
        private static ConfigEntry<T> Bind<T>(ConfigEntry<T> entry)
        {
            entry.OnChanged += _ => BadgeRenderer.RefreshAll();
            return entry;
        }

        private static ConfigEntry<int> BindSize(ConfigEntry<int> entry, int min, int max)
        {
            entry.OnChanged += value =>
            {
                int clamped = Mathf.Clamp(value, min, max);
                if (clamped != value)
                {
                    entry.Set(clamped);
                    return;
                }

                BadgeRenderer.RefreshAll();
            };
            return entry;
        }

        private static int Clamp(ConfigEntry<int> entry, int fallback, int min, int max)
        {
            int value = entry != null ? entry.Value : fallback;
            return Mathf.Clamp(value, min, max);
        }
        #endregion
    }
}
