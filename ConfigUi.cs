using System;
using HarmonyLib;
using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 本模组配置面板：中文选项、品质对照预览（属性说明放 tips）。
    /// </summary>
    [HarmonyPatch]
    internal static class ConfigUi
    {
        private const string PreviewScaleId = "preview_scale";
        private const string PreviewBerryId = "preview_berry";
        private const string PreviewWaterId = "preview_water";
        private const string PreviewYeastId = "preview_yeast";
        private const string PreviewWineId = "preview_wine";
        private const string PreviewIngotId = "preview_ingot";
        private const string PreviewChemId = "preview_chem";

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
                b.AddToggle("", (bool)boxedValue, null, entry.Key);
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

            if (entry.Key == "style")
            {
                AddLegend(b, ParseStyle(IndexOf(entry)), BadgeConfig.WaterNumbers, BadgeConfig.Scheme);
            }
        }

        private static int IndexOf(ConfigEntry entry)
        {
            return Mathf.Max(0, Array.IndexOf(entry.Choices, ConfigEntry.Format(entry.BoxedValue)));
        }

        private static void AddLegend(
            CustomUIBuilder b,
            BadgeStyle style,
            WaterNumberMode waterMode,
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
                AddLegendRow(b, "cfg_legend_wine", "cfg_legend_wine_tip", PreviewWineId, WineBadge.Legend(), style);
                AddLegendRow(b, "cfg_legend_yeast", "cfg_legend_yeast_tip", PreviewYeastId, YeastBadge.Legend(), style);
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

        private static void RefreshPreviews(BadgeStyle style, WaterNumberMode water, ColorScheme scheme)
        {
            using (PaletteScope(scheme))
            {
                SetPreview(PreviewScaleId, ScaleLegend(), style);
                SetPreview(PreviewBerryId, BerryBadge.Legend(), style);
                SetPreview(PreviewWaterId, WaterBadge.Legend(water), style);
                SetPreview(PreviewWineId, WineBadge.Legend(), style);
                SetPreview(PreviewYeastId, YeastBadge.Legend(), style);
                SetPreview(PreviewIngotId, IngotBadge.Legend(), style);
                SetPreview(PreviewChemId, ChemBadge.Legend(), style);
            }
        }

        private static void OnStyleChanged(int index)
        {
            RefreshPreviews(ParseStyle(index), PendingWaterMode(), PendingScheme());
        }

        private static void OnSchemeChanged(int index)
        {
            RefreshPreviews(PendingStyle(), PendingWaterMode(), ParseScheme(index));
        }

        private static void OnWaterModeChanged(int index)
        {
            WaterNumberMode mode = index == 1 ? WaterNumberMode.WineScore : WaterNumberMode.Quality;
            RefreshPreviews(PendingStyle(), mode, PendingScheme());
        }

        private static PaletteOverride PaletteScope(ColorScheme scheme)
        {
            return new PaletteOverride(scheme);
        }

        private sealed class PaletteOverride : System.IDisposable
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
}
