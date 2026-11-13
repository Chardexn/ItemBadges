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
        public static ConfigEntry<BadgeStyle> StyleEntry { get; private set; }
        public static ConfigEntry<ColorPreset> PresetEntry { get; private set; }
        public static ConfigEntry<int> BlockSizeEntry { get; private set; }
        public static ConfigEntry<int> FontSizeEntry { get; private set; }
        public static ConfigEntry<bool> BerryEnabledEntry { get; private set; }
        public static ConfigEntry<bool> YeastEnabledEntry { get; private set; }
        public static ConfigEntry<bool> WaterEnabledEntry { get; private set; }

        public static BadgeStyle Style => StyleEntry != null ? StyleEntry.Value : BadgeStyle.ColorBlock;
        public static ColorPreset Preset => PresetEntry != null ? PresetEntry.Value : ColorPreset.Spectrum;
        public static int BlockSize => BlockSizeEntry != null ? BlockSizeEntry.Value : 4;
        public static int FontSize => FontSizeEntry != null ? FontSizeEntry.Value : 9;
        public static bool BerryEnabled => BerryEnabledEntry == null || BerryEnabledEntry.Value;
        public static bool YeastEnabled => YeastEnabledEntry == null || YeastEnabledEntry.Value;
        public static bool WaterEnabled => WaterEnabledEntry == null || WaterEnabledEntry.Value;

        #region MARK: 定义
        public static void Define()
        {
            ModConfig cfg = ModHelper.GetConfig(ModEntry.MOD_ID);
            const string general = "General";

            StyleEntry = Bind(cfg.DefineEnum(
                "style",
                BadgeStyle.ColorBlock,
                general,
                () => ModEntry.L("cfg_style"),
                () => ModEntry.L("cfg_style_tip")));

            PresetEntry = Bind(cfg.DefineEnum(
                "color_preset",
                ColorPreset.Spectrum,
                general,
                () => ModEntry.L("cfg_preset"),
                () => ModEntry.L("cfg_preset_tip")));

            BlockSizeEntry = Bind(cfg.Define(
                "block_size",
                4,
                general,
                () => ModEntry.L("cfg_block_size"),
                2,
                12,
                () => ModEntry.L("cfg_block_size_tip")));

            FontSizeEntry = Bind(cfg.Define(
                "font_size",
                9,
                general,
                () => ModEntry.L("cfg_font_size"),
                6,
                18,
                () => ModEntry.L("cfg_font_size_tip")));

            BerryEnabledEntry = Bind(cfg.Define(
                "berry_enabled",
                true,
                general,
                () => ModEntry.L("cfg_berry"),
                () => ModEntry.L("cfg_berry_tip")));

            YeastEnabledEntry = Bind(cfg.Define(
                "yeast_enabled",
                true,
                general,
                () => ModEntry.L("cfg_yeast"),
                () => ModEntry.L("cfg_yeast_tip")));

            WaterEnabledEntry = Bind(cfg.Define(
                "water_enabled",
                true,
                general,
                () => ModEntry.L("cfg_water"),
                () => ModEntry.L("cfg_water_tip")));
        }
        #endregion

        #region MARK: 刷新
        private static ConfigEntry<T> Bind<T>(ConfigEntry<T> entry)
        {
            entry.OnChanged += OnSettingChanged;
            return entry;
        }

        private static void OnSettingChanged<T>(T _)
        {
            BadgeRenderer.RefreshAll();
        }
        #endregion
    }
}
