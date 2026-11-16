using System;
using HarmonyLib;
using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 本模组配置面板：中文选项、分块、可折叠品质对照。
    /// </summary>
    [HarmonyPatch]
    internal static class ConfigUi
    {
        private const string PreviewScaleId = "preview_scale";
        private const string PreviewBerryId = "preview_berry";
        private const string PreviewWaterId = "preview_water";
        private const string PreviewYeastId = "preview_yeast";
        private const string PreviewWineId = "preview_wine";
        private const string PreviewGunId = "preview_gun";
        private const string PreviewIngotId = "preview_ingot";
        private const string PreviewChemId = "preview_chem";

        private static readonly string[] LegendIds =
        {
            "legend_header",
            "label_" + PreviewScaleId, PreviewScaleId,
            "label_" + PreviewBerryId, PreviewBerryId,
            "label_" + PreviewWaterId, PreviewWaterId,
            "label_" + PreviewWineId, PreviewWineId,
            "label_" + PreviewYeastId, PreviewYeastId,
            "label_" + PreviewGunId, PreviewGunId,
            "label_" + PreviewIngotId, PreviewIngotId,
            "label_" + PreviewChemId, PreviewChemId
        };

        #region MARK: 补丁
        private static System.Reflection.MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(ModConfigWindow), "BuildEntry");
        }

        private static bool Prefix(CustomUIBuilder b, ModConfig config, ConfigEntry entry)
        {
            if (config == null || config.ModId != ModEntry.MOD_ID || b == null || entry == null)
            {
                return true;
            }

            try
            {
                BuildEntry(b, config, entry);
                return false;
            }
            catch (Exception ex)
            {
                ModEntry.Instance?.Log.Error("ConfigUi.BuildEntry failed: " + ex);
                return true;
            }
        }
        #endregion

        #region MARK: 条目
        private static void BuildEntry(CustomUIBuilder b, ModConfig config, ConfigEntry entry)
        {
            string label = Safe(entry.Label, entry.Key);
            if (entry.Tooltip != null)
            {
                string link = "modcfg:" + config.ModId + ":" + entry.Key;
                TMPIconHover.CustomLinks[link] = entry.Tooltip;
                label = label + " <link=\"" + link + "\"><sprite name=\"info\" tint=1></link>";
            }

            b.BeginRow(4f, TextAnchor.MiddleLeft);
            b.AddLabel(label, "label_" + entry.Key);
            b.AddFlexibleSpace();

            Type valueType = entry.ValueType;
            object boxedValue = entry.BoxedValue;
            if (valueType == typeof(bool))
            {
                Action<bool> onToggle = entry.Key == "legend_expanded" ? (Action<bool>)SetLegendVisible : null;
                b.AddToggle("", (bool)boxedValue, onToggle, entry.Key);
            }
            else if (entry.Choices != null && entry.Choices.Length != 0)
            {
                string[] labels = LocalizeChoices(entry);
                int index = Mathf.Max(0, Array.IndexOf(entry.Choices, ConfigEntry.Format(boxedValue)));
                if (entry.Key == "style")
                {
                    b.AddDropdown(labels, index, OnStyleChanged, entry.Key);
                }
                else if (entry.Key == "color_scheme")
                {
                    b.AddDropdown(labels, index, OnSchemeChanged, entry.Key);
                }
                else if (entry.Key == "water_number")
                {
                    b.AddDropdown(labels, index, OnWaterModeChanged, entry.Key);
                }
                else if (entry.Key == "wine_number")
                {
                    b.AddDropdown(labels, index, OnWineModeChanged, entry.Key);
                }
                else if (entry.Key == "yeast_number")
                {
                    b.AddDropdown(labels, index, OnYeastModeChanged, entry.Key);
                }
                else
                {
                    b.AddDropdown(labels, index, null, entry.Key);
                }

                b.Last.SetFixedWidth(245f);
            }
            else
            {
                b.AddInput("", ConfigEntry.Format(boxedValue), null, entry.Key);
                b.Last.SetFixedWidth(80f);
            }

            b.End();

            if (entry.Key == "legend_expanded")
            {
                AddLegend(
                    b,
                    ParseStyle(DropdownIndex("style")),
                    PendingWaterMode(),
                    PendingWineMode(),
                    PendingYeastMode(),
                    PendingScheme());
                SetLegendVisible((bool)boxedValue);
            }
        }

        private static void AddLegend(
            CustomUIBuilder b,
            BadgeStyle style,
            WaterNumberMode waterMode,
            WineNumberMode wineMode,
            YeastNumberMode yeastMode,
            ColorScheme scheme)
        {
            b.AddSpace(6f);
            b.BeginRow(4f, TextAnchor.MiddleLeft);
            b.AddHeader(ModEntry.L("cfg_legend"), "legend_header");
            b.End();

            using (PaletteScope(scheme))
            {
                AddLegendRow(b, "cfg_legend_scale", "cfg_legend_scale_tip", PreviewScaleId, ScaleLegend(), style);
                AddLegendRow(b, "cfg_legend_berry", "cfg_legend_berry_tip", PreviewBerryId, BerryBadge.Legend(), style);
                AddLegendRow(b, "cfg_legend_water", "cfg_legend_water_tip", PreviewWaterId, WaterBadge.Legend(waterMode), style);
                AddLegendRow(b, "cfg_legend_wine", "cfg_legend_wine_tip", PreviewWineId, WineBadge.Legend(wineMode), style);
                AddLegendRow(b, "cfg_legend_yeast", "cfg_legend_yeast_tip", PreviewYeastId, YeastBadge.Legend(yeastMode), style);
                AddLegendRow(b, "cfg_legend_gun", "cfg_legend_gun_tip", PreviewGunId, GunBadge.Legend(), style);
                AddLegendRow(b, "cfg_legend_ingot", "cfg_legend_ingot_tip", PreviewIngotId, IngotBadge.Legend(), style);
                AddLegendRow(b, "cfg_legend_chem", "cfg_legend_chem_tip", PreviewChemId, ChemBadge.Legend(), style);
            }
        }

        private static void AddLegendRow(
            CustomUIBuilder b,
            string titleKey,
            string tipKey,
            string imageId,
            Badge[] badges,
            BadgeStyle style)
        {
            string label = ModEntry.L(titleKey);
            string link = "modcfg:" + ModEntry.MOD_ID + ":legend_" + imageId;
            TMPIconHover.CustomLinks[link] = () => ModEntry.L(tipKey);
            label = label + " <link=\"" + link + "\"><sprite name=\"info\" tint=1></link>";

            b.BeginRow(4f, TextAnchor.MiddleLeft);
            b.AddLabel(label, "label_" + imageId);
            b.AddFlexibleSpace();
            Sprite sprite = BadgeSprite.Strip(badges, style);
            b.AddImage(sprite, sprite.rect.width, sprite.rect.height, imageId);
            b.End();
        }

        private static Badge[] ScaleLegend()
        {
            int[] deltas = BadgePalette.ScaleDeltas();
            Badge[] badges = new Badge[deltas.Length];
            for (int i = 0; i < deltas.Length; i++)
            {
                badges[i] = Badge.FromDelta(deltas[i]);
            }

            return badges;
        }

        private static void RefreshPreviews(
            BadgeStyle style,
            WaterNumberMode water,
            WineNumberMode wine,
            YeastNumberMode yeast,
            ColorScheme scheme)
        {
            using (PaletteScope(scheme))
            {
                SetPreview(PreviewScaleId, ScaleLegend(), style);
                SetPreview(PreviewBerryId, BerryBadge.Legend(), style);
                SetPreview(PreviewWaterId, WaterBadge.Legend(water), style);
                SetPreview(PreviewWineId, WineBadge.Legend(wine), style);
                SetPreview(PreviewYeastId, YeastBadge.Legend(yeast), style);
                SetPreview(PreviewGunId, GunBadge.Legend(), style);
                SetPreview(PreviewIngotId, IngotBadge.Legend(), style);
                SetPreview(PreviewChemId, ChemBadge.Legend(), style);
            }
        }

        private static void OnStyleChanged(int index)
        {
            RefreshPreviews(ParseStyle(index), PendingWaterMode(), PendingWineMode(), PendingYeastMode(), PendingScheme());
        }

        private static void OnSchemeChanged(int index)
        {
            RefreshPreviews(PendingStyle(), PendingWaterMode(), PendingWineMode(), PendingYeastMode(), ParseScheme(index));
        }

        private static void OnWaterModeChanged(int index)
        {
            RefreshPreviews(
                PendingStyle(),
                index == 1 ? WaterNumberMode.WineScore : WaterNumberMode.Quality,
                PendingWineMode(),
                PendingYeastMode(),
                PendingScheme());
        }

        private static void OnWineModeChanged(int index)
        {
            RefreshPreviews(
                PendingStyle(),
                PendingWaterMode(),
                index == 1 ? WineNumberMode.Score : WineNumberMode.Grade,
                PendingYeastMode(),
                PendingScheme());
        }

        private static void OnYeastModeChanged(int index)
        {
            RefreshPreviews(
                PendingStyle(),
                PendingWaterMode(),
                PendingWineMode(),
                index == 1 ? YeastNumberMode.FermentDays : YeastNumberMode.WineScore,
                PendingScheme());
        }

        internal static void SetLegendVisible(bool visible)
        {
            CustomUIWindow window = GetWindow();
            if (window == null)
            {
                return;
            }

            for (int i = 0; i < LegendIds.Length; i++)
            {
                CustomUIElement element = window.Get(LegendIds[i]);
                if (element == null || element.root == null)
                {
                    continue;
                }

                Transform parent = element.root.transform.parent;
                if (parent != null)
                {
                    parent.gameObject.SetActive(visible);
                }
                else
                {
                    element.SetVisible(visible);
                }
            }
        }

        private static PaletteOverride PaletteScope(ColorScheme scheme)
        {
            return new PaletteOverride(scheme);
        }

        private sealed class PaletteOverride : IDisposable
        {
            public PaletteOverride(ColorScheme scheme)
            {
                BadgePalette.PreviewOverride = scheme;
            }

            public void Dispose()
            {
                BadgePalette.PreviewOverride = null;
            }
        }

        private static BadgeStyle PendingStyle()
        {
            return ParseStyle(DropdownIndex("style"));
        }

        private static WaterNumberMode PendingWaterMode()
        {
            return DropdownIndex("water_number") == 1 ? WaterNumberMode.WineScore : WaterNumberMode.Quality;
        }

        private static WineNumberMode PendingWineMode()
        {
            return DropdownIndex("wine_number") == 1 ? WineNumberMode.Score : WineNumberMode.Grade;
        }

        private static YeastNumberMode PendingYeastMode()
        {
            return DropdownIndex("yeast_number") == 1 ? YeastNumberMode.FermentDays : YeastNumberMode.WineScore;
        }

        private static ColorScheme PendingScheme()
        {
            return ParseScheme(DropdownIndex("color_scheme"));
        }

        private static int DropdownIndex(string id)
        {
            CustomUIWindow window = GetWindow();
            CustomUIElement dropdown = window != null ? window.Get(id) : null;
            if (dropdown == null)
            {
                return 0;
            }

            return Mathf.Max(0, dropdown.GetIndex());
        }

        private static void SetPreview(string id, Badge[] badges, BadgeStyle style)
        {
            CustomUIWindow window = GetWindow();
            CustomUIElement image = window != null ? window.Get(id) : null;
            if (image == null)
            {
                return;
            }

            Sprite sprite = BadgeSprite.Strip(badges, style);
            image.SetSprite(sprite);
            image.SetPreferredSize(sprite.rect.width, sprite.rect.height);
        }

        private static CustomUIWindow GetWindow()
        {
            CustomUIManager manager = CustomUIManager.Instance;
            return manager != null ? manager.GetWindow("mod_config:" + ModEntry.MOD_ID) : null;
        }
        #endregion

        #region MARK: 中文选项
        private static string[] LocalizeChoices(ConfigEntry entry)
        {
            if (entry.Key == "style")
            {
                return new[]
                {
                    ModEntry.L("opt_style_block"),
                    ModEntry.L("opt_style_number"),
                    ModEntry.L("opt_style_arrow")
                };
            }

            if (entry.Key == "color_scheme")
            {
                return new[]
                {
                    ModEntry.L("opt_scheme_ybg"),
                    ModEntry.L("opt_scheme_gby"),
                    ModEntry.L("opt_scheme_rarity")
                };
            }

            if (entry.Key == "badge_corner")
            {
                return new[]
                {
                    ModEntry.L("opt_corner_tl"),
                    ModEntry.L("opt_corner_tr"),
                    ModEntry.L("opt_corner_bl"),
                    ModEntry.L("opt_corner_br")
                };
            }

            if (entry.Key == "water_number")
            {
                return new[]
                {
                    ModEntry.L("opt_water_quality"),
                    ModEntry.L("opt_water_wine")
                };
            }

            if (entry.Key == "wine_number")
            {
                return new[]
                {
                    ModEntry.L("opt_wine_grade"),
                    ModEntry.L("opt_wine_score")
                };
            }

            if (entry.Key == "yeast_number")
            {
                return new[]
                {
                    ModEntry.L("opt_yeast_score"),
                    ModEntry.L("opt_yeast_days")
                };
            }

            if (entry.Key == "purity_mode")
            {
                return new[]
                {
                    ModEntry.L("opt_purity_label"),
                    ModEntry.L("opt_purity_real")
                };
            }

            return entry.Choices;
        }

        private static BadgeStyle ParseStyle(int index)
        {
            if (index == 2)
            {
                return BadgeStyle.Arrow;
            }

            if (index == 1)
            {
                return BadgeStyle.Number;
            }

            return BadgeStyle.ColorBlock;
        }

        private static ColorScheme ParseScheme(int index)
        {
            if (index == 2)
            {
                return ColorScheme.Rarity;
            }

            if (index == 1)
            {
                return ColorScheme.GreenBlueYellow;
            }

            return ColorScheme.YellowBlueGreen;
        }

        private static T Safe<T>(Func<T> getter, T fallback)
        {
            if (getter == null)
            {
                return fallback;
            }

            try
            {
                return getter();
            }
            catch (Exception)
            {
                return fallback;
            }
        }
        #endregion
    }

    [HarmonyPatch(typeof(ModConfigWindow), nameof(ModConfigWindow.Open))]
    internal static class ConfigWindowSectionsPatch
    {
        private static void Postfix(LoadedMod mod, CustomUIWindow __result)
        {
            if (mod?.Manifest == null || __result == null || !__result.IsAlive)
            {
                return;
            }

            if (mod.Manifest.ID != ModEntry.MOD_ID)
            {
                return;
            }

            __result.Get("section_appearance")?.SetText(ModEntry.L("cfg_section_appearance"));
            __result.Get("section_legend")?.SetText(ModEntry.L("cfg_section_legend"));
            __result.Get("section_berry")?.SetText(ModEntry.L("cfg_section_berry"));
            __result.Get("section_yeast")?.SetText(ModEntry.L("cfg_section_yeast"));
            __result.Get("section_water")?.SetText(ModEntry.L("cfg_section_water"));
            __result.Get("section_wine")?.SetText(ModEntry.L("cfg_section_wine"));
            __result.Get("section_gun")?.SetText(ModEntry.L("cfg_section_gun"));
            __result.Get("section_ingot")?.SetText(ModEntry.L("cfg_section_ingot"));
            __result.Get("section_chem")?.SetText(ModEntry.L("cfg_section_chem"));

            CustomUIElement toggle = __result.Get("legend_expanded");
            bool expanded = toggle != null && toggle.GetBool();
            ConfigUi.SetLegendVisible(expanded);
        }
    }
}
