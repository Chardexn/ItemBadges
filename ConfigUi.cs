using System;
using System.Globalization;
using HarmonyLib;
using UnityEngine;

namespace ItemBadges
{
    /// <summary>
    /// 本模组配置面板：中文选项、颜色预设色带预览。只拦截自己的条目。
    /// </summary>
    [HarmonyPatch]
    internal static class ConfigUi
    {
        private const string PreviewId = "preset_preview";

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
            else if (IsNumber(valueType) && entry.HasRange)
            {
                b.AddSlider(entry.Min, entry.Max, Convert.ToSingle(boxedValue, CultureInfo.InvariantCulture), null, valueType == typeof(int), entry.Key);
                b.Last.SetFixedWidth(224f);
            }
            else if (entry.Choices != null && entry.Choices.Length != 0)
            {
                string[] labels = LocalizeChoices(entry);
                int index = Mathf.Max(0, Array.IndexOf(entry.Choices, ConfigEntry.Format(boxedValue)));
                if (entry.Key == "color_preset")
                {
                    b.AddDropdown(labels, index, OnPresetChanged, entry.Key);
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

            if (entry.Key == "color_preset")
            {
                AddPresetPreview(b, ParsePresetIndex(Array.IndexOf(entry.Choices, ConfigEntry.Format(boxedValue))));
            }
        }

        private static void AddPresetPreview(CustomUIBuilder b, ColorPreset preset)
        {
            b.BeginRow(4f, TextAnchor.MiddleLeft);
            b.AddLabel(ModEntry.L("cfg_preset_preview"), "label_" + PreviewId);
            b.AddFlexibleSpace();
            Sprite sprite = BadgePalette.PreviewSprite(preset);
            b.AddImage(sprite, 132f, 16f, PreviewId);
            b.End();
        }

        private static void OnPresetChanged(int index)
        {
            CustomUIManager manager = CustomUIManager.Instance;
            if (manager == null)
            {
                return;
            }

            CustomUIWindow window = manager.GetWindow("mod_config:" + ModEntry.MOD_ID);
            CustomUIElement image = window != null ? window.Get(PreviewId) : null;
            image?.SetSprite(BadgePalette.PreviewSprite(ParsePresetIndex(index)));
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
                    ModEntry.L("opt_style_both")
                };
            }

            if (entry.Key == "color_preset")
            {
                return new[]
                {
                    ModEntry.L("opt_preset_spectrum"),
                    ModEntry.L("opt_preset_compact")
                };
            }

            return entry.Choices;
        }

        private static ColorPreset ParsePresetIndex(int index)
        {
            return index == 1 ? ColorPreset.Compact : ColorPreset.Spectrum;
        }

        private static bool IsNumber(Type type)
        {
            return type == typeof(int) || type == typeof(float);
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
