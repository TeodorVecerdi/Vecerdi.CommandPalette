using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Vecerdi.CommandPalette.PluginSupport;

namespace Vecerdi.CommandPalette.Settings;

public static class CommandPaletteSettingsProvider {
    // GUIStyles must be lazily initialized — EditorStyles is not available during domain reload
    // when Unity invokes the [SettingsProvider] factory method.
    private static GUIStyle? s_HeaderStyle;
    private static GUIStyle? s_PluginNameStyle;
    private static GUIStyle? s_PluginHeaderStyle;
    private static GUIStyle? s_PluginContentsStyle;
    private static GUIStyle? s_SectionStyle;
    private static GUIStyle? s_BoxStyle;

    private static void EnsureStyles() {
        if (s_HeaderStyle != null) return;

        s_HeaderStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 16,
            margin = { bottom = 4 },
        };
        s_PluginNameStyle = new GUIStyle(EditorStyles.boldLabel) {
            fontSize = 13,
        };
        s_PluginHeaderStyle = new GUIStyle {
            margin = { top = 12 },
        };
        s_PluginContentsStyle = new GUIStyle {
            margin = {
                left = 3,
                right = 3,
            },
        };
        s_SectionStyle = new GUIStyle {
            margin = new RectOffset(8, 8, 8, 0),
        };
        s_BoxStyle = new GUIStyle("box") {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            normal = {
                textColor = new Color(0.752f, 0.752f, 0.752f, 1.0f),
            },
            padding = new RectOffset(8, 8, 8, 8),
        };
    }

    [SettingsProvider]
    public static SettingsProvider CreateCommandPaletteSettingsProvider() {
        PluginSettingsManager.CleanupAssets();
        HashSet<string> keywords = new() {
            "Command Palette",
            "Blur",
            "Down Sample",
            "Size",
            "Passes",
            "Tint",
            "Color",
            "Amount",
            "Clear",
            "Selection",
            "Search",
        };

        foreach ((var provider, _) in PluginSettingsManager.Settings) {
            provider.AddKeywords(keywords);
        }

        return new SettingsProvider("Project/CommandPalette", SettingsScope.Project) {
            label = "Command Palette",
            guiHandler = _ => {
                EnsureStyles();
                var settings = CommandPaletteSettings.GetSerializedSettings();
                DrawBlurSettings(settings);
                settings.ApplyModifiedProperties();

                if (PluginSettingsManager.Settings.Count <= 0) {
                    return;
                }

                GUILayout.Space(8.0f);
                GUILayout.BeginVertical("Plugins", s_BoxStyle);
                GUILayout.Space(24.0f);
                List<(IPluginSettingsProvider, ScriptableObject)> newSettings = new();
                foreach ((var provider, var pluginSettings) in PluginSettingsManager.Settings) {
                    var settingsInstance = pluginSettings;
                    if (settingsInstance == null) {
                        settingsInstance = PluginSettingsManager.GetOrCreateSettings(provider as IPlugin, provider.SettingsType);
                        newSettings.Add((provider, settingsInstance));
                    }

                    DrawPluginHeader(provider, settingsInstance);
                    GUILayout.BeginVertical(s_PluginContentsStyle);
                    provider.DrawSettings(new SerializedObject(settingsInstance));
                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();

                foreach ((var provider, var pluginSettings) in newSettings) {
                    PluginSettingsManager.RegisterSettingsProvider(provider, pluginSettings);
                }
            },
            keywords = keywords,
        };
    }

    private static void DrawBlurSettings(SerializedObject serializedObject) {
        EnsureStyles();
        var settings = CommandPaletteSettings.GetOrCreateSettings();

        GUILayout.BeginVertical("", s_SectionStyle);
        GUILayout.Label("General Settings", s_HeaderStyle);
        EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.ClearSearchOnSelectionProperty));

        GUILayout.Label("Blur Settings", s_HeaderStyle);
        EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.DownSamplePassesProperty));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.PassesProperty));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.BlurSizeProperty));

        EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.EnableTintProperty));
        if (settings.EnableTint) {
            using var _ = new GUILayout.HorizontalScope();
            GUILayout.Space(16.0f);
            using var __ = new GUILayout.VerticalScope();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.TintAmountProperty));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.TintProperty));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.EnableVibrancyProperty));
        if (settings.EnableVibrancy) {
            using var _ = new GUILayout.HorizontalScope();
            GUILayout.Space(16.0f);
            using var __ = new GUILayout.VerticalScope();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.VibrancyProperty));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.EnableNoiseProperty));
        if (settings.EnableNoise) {
            using var _ = new GUILayout.HorizontalScope();
            GUILayout.Space(16.0f);
            using var __ = new GUILayout.VerticalScope();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(CommandPaletteSettings.NoiseTextureProperty));
        }

        GUILayout.EndVertical();
    }

    private static void DrawPluginHeader(IPluginSettingsProvider provider, ScriptableObject pluginSettings) {
        EnsureStyles();
        var pluginName = provider is IPlugin plugin ? plugin.Name : provider.GetType().Name;
        GUILayout.BeginHorizontal(s_PluginHeaderStyle);
        GUILayout.Label(pluginName, s_PluginNameStyle, GUILayout.Width(256.0f));
        GUI.enabled = false;
        EditorGUILayout.ObjectField((string?)null, pluginSettings, pluginSettings.GetType(), true, GUILayout.ExpandWidth(true), GUILayout.MinWidth(256.0f));
        GUI.enabled = true;
        GUILayout.EndHorizontal();
    }
}
