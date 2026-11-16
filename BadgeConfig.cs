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

    internal enum WineNumberMode
    {
        Grade = 0,
        Score = 1
    }

    internal enum YeastNumberMode
    {
        WineScore = 0,
        FermentDays = 1
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
        public static ConfigEntry<bool> LegendExpandedEntry { get; private set; }
        public static ConfigEntry<WaterNumberMode> WaterNumberEntry { get; private set; }
        public static ConfigEntry<WineNumberMode> WineNumberEntry { get; private set; }
        public static ConfigEntry<YeastNumberMode> YeastNumberEntry { get; private set; }
        public static ConfigEntry<PurityMode> PurityModeEntry { get; private set; }
        public static ConfigEntry<bool> BerryEnabledEntry { get; private set; }
        public static ConfigEntry<bool> YeastEnabledEntry { get; private set; }
        public static ConfigEntry<bool> WaterEnabledEntry { get; private set; }
        public static ConfigEntry<bool> WineEnabledEntry { get; private set; }
        public static ConfigEntry<bool> GunEnabledEntry { get; private set; }
        public static ConfigEntry<bool> IngotEnabledEntry { get; private set; }
        public static ConfigEntry<bool> ChemEnabledEntry { get; private set; }

        public static BadgeStyle Style => StyleEntry != null ? StyleEntry.Value : BadgeStyle.Number;
        public static ColorScheme Scheme =>
            SchemeEntry != null ? SchemeEntry.Value : ColorScheme.YellowBlueGreen;
        public static BadgeCorner Corner => CornerEntry != null ? CornerEntry.Value : BadgeCorner.BottomRight;
        public static bool LegendExpanded =>
            LegendExpandedEntry != null && LegendExpandedEntry.Value;
        public static WaterNumberMode WaterNumbers =>
            WaterNumberEntry != null ? WaterNumberEntry.Value : WaterNumberMode.Quality;
        public static WineNumberMode WineNumbers =>
            WineNumberEntry != null ? WineNumberEntry.Value : WineNumberMode.Grade;
        public static YeastNumberMode YeastNumbers =>
            YeastNumberEntry != null ? YeastNumberEntry.Value : YeastNumberMode.WineScore;
        public static PurityMode PurityDisplay =>
            PurityModeEntry != null ? PurityModeEntry.Value : PurityMode.Label;
        public static bool UseRealPurity => PurityDisplay == PurityMode.Real;
        public static bool BerryEnabled => BerryEnabledEntry == null || BerryEnabledEntry.Value;
        public static bool YeastEnabled => YeastEnabledEntry == null || YeastEnabledEntry.Value;
        public static bool WaterEnabled => WaterEnabledEntry == null || WaterEnabledEntry.Value;
        public static bool WineEnabled => WineEnabledEntry == null || WineEnabledEntry.Value;
        public static bool GunEnabled => GunEnabledEntry == null || GunEnabledEntry.Value;
        public static bool IngotEnabled => IngotEnabledEntry == null || IngotEnabledEntry.Value;
        public static bool ChemEnabled => ChemEnabledEntry == null || ChemEnabledEntry.Value;

        #region MARK: 定义
        public static void Define()
        {
            ModConfig cfg = ModHelper.GetConfig(ModEntry.MOD_ID);

            StyleEntry = Bind(cfg.DefineEnum(
                "style",
                BadgeStyle.Number,
                "appearance",
                () => ModEntry.L("cfg_style"),
                () => ModEntry.L("cfg_style_tip")));

            SchemeEntry = Bind(cfg.DefineEnum(
                "color_scheme",
                ColorScheme.YellowBlueGreen,
                "appearance",
                () => ModEntry.L("cfg_scheme"),
                () => ModEntry.L("cfg_scheme_tip")));

            CornerEntry = Bind(cfg.DefineEnum(
                "badge_corner",
                BadgeCorner.BottomRight,
                "appearance",
                () => ModEntry.L("cfg_corner"),
                () => ModEntry.L("cfg_corner_tip")));

            LegendExpandedEntry = Bind(cfg.Define(
                "legend_expanded",
                false,
                "legend",
                () => ModEntry.L("cfg_legend_expanded"),
                () => ModEntry.L("cfg_legend_expanded_tip")));

            BerryEnabledEntry = Bind(cfg.Define(
                "berry_enabled",
                true,
                "berry",
                () => ModEntry.L("cfg_berry"),
                () => ModEntry.L("cfg_berry_tip")));

            YeastEnabledEntry = Bind(cfg.Define(
                "yeast_enabled",
                true,
                "yeast",
                () => ModEntry.L("cfg_yeast"),
                () => ModEntry.L("cfg_yeast_tip")));

            YeastNumberEntry = Bind(cfg.DefineEnum(
                "yeast_number",
                YeastNumberMode.WineScore,
                "yeast",
                () => ModEntry.L("cfg_yeast_number"),
                () => ModEntry.L("cfg_yeast_number_tip")));

            WaterEnabledEntry = Bind(cfg.Define(
                "water_enabled",
                true,
                "water",
                () => ModEntry.L("cfg_water"),
                () => ModEntry.L("cfg_water_tip")));

            WaterNumberEntry = Bind(cfg.DefineEnum(
                "water_number",
                WaterNumberMode.Quality,
                "water",
                () => ModEntry.L("cfg_water_number"),
                () => ModEntry.L("cfg_water_number_tip")));

            PurityModeEntry = Bind(cfg.DefineEnum(
                "purity_mode",
                PurityMode.Label,
                "water",
                () => ModEntry.L("cfg_purity_mode"),
                () => ModEntry.L("cfg_purity_mode_tip")));

            WineEnabledEntry = Bind(cfg.Define(
                "wine_enabled",
                true,
                "wine",
                () => ModEntry.L("cfg_wine"),
                () => ModEntry.L("cfg_wine_tip")));

            WineNumberEntry = Bind(cfg.DefineEnum(
                "wine_number",
                WineNumberMode.Grade,
                "wine",
                () => ModEntry.L("cfg_wine_number"),
                () => ModEntry.L("cfg_wine_number_tip")));

            GunEnabledEntry = Bind(cfg.Define(
                "gun_enabled",
                true,
                "gun",
                () => ModEntry.L("cfg_gun"),
                () => ModEntry.L("cfg_gun_tip")));

            IngotEnabledEntry = Bind(cfg.Define(
                "ingot_enabled",
                true,
                "ingot",
                () => ModEntry.L("cfg_ingot"),
                () => ModEntry.L("cfg_ingot_tip")));

            ChemEnabledEntry = Bind(cfg.Define(
                "chem_enabled",
                true,
                "chem",
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
