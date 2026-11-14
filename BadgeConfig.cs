namespace ItemBadges
{
    internal enum BadgeStyle
    {
        ColorBlock = 0,
        Number = 1,
        Arrow = 2
    }

    internal enum BadgeCorner
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3
    }

    internal enum WaterNumberMode
    {
        Quality = 0,
        WineScore = 1
    }

    internal enum PurityMode
    {
        Label = 0,
        Real = 1
    }

    internal enum ColorScheme
    {
        YellowBlueGreen = 0,
        GreenBlueYellow = 1,
        Rarity = 2
    }

    /// <summary>
    /// 模组配置。Define 在 Init 调用；标签用延迟 Func，打开面板时才读本地化。
    /// </summary>
    internal static class BadgeConfig
    {
        public static ConfigEntry<BadgeStyle> StyleEntry { get; private set; }
        public static ConfigEntry<ColorScheme> SchemeEntry { get; private set; }
        public static ConfigEntry<BadgeCorner> CornerEntry { get; private set; }
        public static ConfigEntry<WaterNumberMode> WaterNumberEntry { get; private set; }
        public static ConfigEntry<PurityMode> PurityModeEntry { get; private set; }
        public static ConfigEntry<bool> BerryEnabledEntry { get; private set; }
        public static ConfigEntry<bool> YeastEnabledEntry { get; private set; }
        public static ConfigEntry<bool> WaterEnabledEntry { get; private set; }
        public static ConfigEntry<bool> WineEnabledEntry { get; private set; }
        public static ConfigEntry<bool> IngotEnabledEntry { get; private set; }
        public static ConfigEntry<bool> ChemEnabledEntry { get; private set; }

        public static BadgeStyle Style => StyleEntry != null ? StyleEntry.Value : BadgeStyle.Number;
        public static ColorScheme Scheme =>
            SchemeEntry != null ? SchemeEntry.Value : ColorScheme.YellowBlueGreen;
        public static BadgeCorner Corner => CornerEntry != null ? CornerEntry.Value : BadgeCorner.BottomRight;
        public static WaterNumberMode WaterNumbers =>
            WaterNumberEntry != null ? WaterNumberEntry.Value : WaterNumberMode.Quality;
        public static PurityMode PurityDisplay =>
            PurityModeEntry != null ? PurityModeEntry.Value : PurityMode.Label;
        public static bool UseRealPurity => PurityDisplay == PurityMode.Real;
        public static bool BerryEnabled => BerryEnabledEntry == null || BerryEnabledEntry.Value;
        public static bool YeastEnabled => YeastEnabledEntry == null || YeastEnabledEntry.Value;
        public static bool WaterEnabled => WaterEnabledEntry == null || WaterEnabledEntry.Value;
        public static bool WineEnabled => WineEnabledEntry == null || WineEnabledEntry.Value;
        public static bool IngotEnabled => IngotEnabledEntry == null || IngotEnabledEntry.Value;
        public static bool ChemEnabled => ChemEnabledEntry == null || ChemEnabledEntry.Value;

        #region MARK: 定义
        public static void Define()
        {
            ModConfig cfg = ModHelper.GetConfig(ModEntry.MOD_ID);
            const string section = "常规";

            StyleEntry = Bind(cfg.DefineEnum(
                "style",
                BadgeStyle.Number,
                section,
                () => ModEntry.L("cfg_style"),
                () => ModEntry.L("cfg_style_tip")));

            SchemeEntry = Bind(cfg.DefineEnum(
                "color_scheme",
                ColorScheme.YellowBlueGreen,
                section,
                () => ModEntry.L("cfg_scheme"),
                () => ModEntry.L("cfg_scheme_tip")));

            CornerEntry = Bind(cfg.DefineEnum(
                "badge_corner",
                BadgeCorner.BottomRight,
                section,
                () => ModEntry.L("cfg_corner"),
                () => ModEntry.L("cfg_corner_tip")));

            WaterNumberEntry = Bind(cfg.DefineEnum(
                "water_number",
                WaterNumberMode.Quality,
                section,
                () => ModEntry.L("cfg_water_number"),
                () => ModEntry.L("cfg_water_number_tip")));

            PurityModeEntry = Bind(cfg.DefineEnum(
                "purity_mode",
                PurityMode.Label,
                section,
                () => ModEntry.L("cfg_purity_mode"),
                () => ModEntry.L("cfg_purity_mode_tip")));

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

            WineEnabledEntry = Bind(cfg.Define(
                "wine_enabled",
                true,
                section,
                () => ModEntry.L("cfg_wine"),
                () => ModEntry.L("cfg_wine_tip")));

            IngotEnabledEntry = Bind(cfg.Define(
                "ingot_enabled",
                true,
                section,
                () => ModEntry.L("cfg_ingot"),
                () => ModEntry.L("cfg_ingot_tip")));

            ChemEnabledEntry = Bind(cfg.Define(
                "chem_enabled",
                true,
                section,
                () => ModEntry.L("cfg_chem"),
                () => ModEntry.L("cfg_chem_tip")));
        }
        #endregion

        #region MARK: 刷新
        private static ConfigEntry<T> Bind<T>(ConfigEntry<T> entry)
        {
            entry.OnChanged += _ => BadgeRenderer.RefreshAll();
            return entry;
        }
        #endregion
    }
}
