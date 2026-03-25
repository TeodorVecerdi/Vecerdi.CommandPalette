using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.PluginSupport;

namespace Vecerdi.CommandPalette.Settings;

public class CommandPaletteSettingsProvider : SettingsProvider {
    private CommandPaletteSettingsProvider(IEnumerable<string> keywords)
        : base("Project/CommandPalette", SettingsScope.Project, keywords) {
        label = "Command Palette";
    }

    public override void OnActivate(string searchContext, VisualElement rootElement) {
        var settings = CommandPaletteSettings.GetOrCreateSettings();
        var serializedSettings = new SerializedObject(settings);

        var mainContainer = new VisualElement {
            style = {
                paddingLeft = 8,
                paddingRight = 8,
                paddingTop = 4,
            },
        };

        // UITK mode doesn't auto-render the provider label as a title the way IMGUI does.
        mainContainer.Add(new Label("Command Palette") {
            style = {
                fontSize = 19,
                unityFontStyleAndWeight = FontStyle.Bold,
                marginBottom = 10,
            },
        });

        // General Settings
        mainContainer.Add(CreateSectionLabel("General Settings", firstSection: true));
        mainContainer.Add(new PropertyField(serializedSettings.FindProperty(CommandPaletteSettings.ClearSearchOnSelectionProperty)));

        // Blur Settings
        mainContainer.Add(CreateSectionLabel("Blur Settings"));
        mainContainer.Add(new PropertyField(serializedSettings.FindProperty(CommandPaletteSettings.DownSamplePassesProperty)));
        mainContainer.Add(new PropertyField(serializedSettings.FindProperty(CommandPaletteSettings.PassesProperty)));
        mainContainer.Add(new PropertyField(serializedSettings.FindProperty(CommandPaletteSettings.BlurSizeProperty)));

        // Tint
        var enableTintProp = serializedSettings.FindProperty(CommandPaletteSettings.EnableTintProperty);
        mainContainer.Add(new PropertyField(enableTintProp));
        var tintGroup = new VisualElement { style = { marginLeft = 16 } };
        tintGroup.Add(new PropertyField(serializedSettings.FindProperty(CommandPaletteSettings.TintAmountProperty)));
        tintGroup.Add(new PropertyField(serializedSettings.FindProperty(CommandPaletteSettings.TintProperty)));
        tintGroup.style.display = settings.EnableTint ? DisplayStyle.Flex : DisplayStyle.None;
        tintGroup.TrackPropertyValue(enableTintProp, p => tintGroup.style.display = p.boolValue ? DisplayStyle.Flex : DisplayStyle.None);
        mainContainer.Add(tintGroup);

        // Vibrancy
        var enableVibrancyProp = serializedSettings.FindProperty(CommandPaletteSettings.EnableVibrancyProperty);
        mainContainer.Add(new PropertyField(enableVibrancyProp));
        var vibrancyGroup = new VisualElement { style = { marginLeft = 16 } };
        vibrancyGroup.Add(new PropertyField(serializedSettings.FindProperty(CommandPaletteSettings.VibrancyProperty)));
        vibrancyGroup.style.display = settings.EnableVibrancy ? DisplayStyle.Flex : DisplayStyle.None;
        vibrancyGroup.TrackPropertyValue(enableVibrancyProp, p => vibrancyGroup.style.display = p.boolValue ? DisplayStyle.Flex : DisplayStyle.None);
        mainContainer.Add(vibrancyGroup);

        // Noise
        var enableNoiseProp = serializedSettings.FindProperty(CommandPaletteSettings.EnableNoiseProperty);
        mainContainer.Add(new PropertyField(enableNoiseProp));
        var noiseGroup = new VisualElement { style = { marginLeft = 16 } };
        noiseGroup.Add(new PropertyField(serializedSettings.FindProperty(CommandPaletteSettings.NoiseTextureProperty)));
        noiseGroup.style.display = settings.EnableNoise ? DisplayStyle.Flex : DisplayStyle.None;
        noiseGroup.TrackPropertyValue(enableNoiseProp, p => noiseGroup.style.display = p.boolValue ? DisplayStyle.Flex : DisplayStyle.None);
        mainContainer.Add(noiseGroup);

        // Plugin settings — all live inside mainContainer so layout is unified.
        // Each plugin section is re-bound to its own SerializedObject after the main Bind call.
        var pluginBindings = new List<(VisualElement element, SerializedObject so)>();
        if (PluginSettingsManager.Settings.Count > 0) {
            var pluginsBox = new VisualElement {
                style = {
                    marginTop = 16,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = new Color(0.35f, 0.35f, 0.35f),
                    borderBottomColor = new Color(0.35f, 0.35f, 0.35f),
                    borderLeftColor = new Color(0.35f, 0.35f, 0.35f),
                    borderRightColor = new Color(0.35f, 0.35f, 0.35f),
                    backgroundColor = new Color(0f, 0f, 0f, 0.1f),
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 4,
                    paddingBottom = 8,
                },
            };

            pluginsBox.Add(new Label("Plugins") {
                style = {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = new Color(0.752f, 0.752f, 0.752f),
                    marginTop = 4,
                    marginBottom = 8,
                },
            });

            foreach ((var provider, var existingSettings) in PluginSettingsManager.Settings) {
                var settingsInstance = existingSettings;
                if (settingsInstance == null) {
                    settingsInstance = PluginSettingsManager.GetOrCreateSettings(provider as IPlugin, provider.SettingsType);
                    PluginSettingsManager.RegisterSettingsProvider(provider, settingsInstance);
                }

                var pluginSerializedSettings = new SerializedObject(settingsInstance);

                var pluginSection = new VisualElement();
                pluginSection.Add(CreatePluginHeader(provider, settingsInstance));

                var pluginContent = new VisualElement { style = { marginLeft = 3, marginRight = 3 } };
                pluginContent.Add(provider.CreateSettingsUI(pluginSerializedSettings));
                pluginSection.Add(pluginContent);

                pluginsBox.Add(pluginSection);
                pluginBindings.Add((pluginSection, pluginSerializedSettings));
            }

            mainContainer.Add(pluginsBox);
        }

        rootElement.Add(mainContainer);
        mainContainer.Bind(serializedSettings);

        // Re-bind each plugin section to its own SO, overriding the propagated main settings bind.
        foreach ((var element, var so) in pluginBindings) {
            element.Bind(so);
        }
    }

    private static Label CreateSectionLabel(string text, bool firstSection = false) {
        return new Label(text) {
            style = {
                fontSize = 16,
                unityFontStyleAndWeight = FontStyle.Bold,
                marginTop = firstSection ? 0 : 16,
                marginBottom = 4,
            },
        };
    }

    private static VisualElement CreatePluginHeader(IPluginSettingsProvider provider, ScriptableObject pluginSettings) {
        var pluginName = provider is IPlugin plugin ? plugin.Name : provider.GetType().Name;
        var header = new VisualElement {
            style = {
                flexDirection = FlexDirection.Row,
                alignItems = Align.Center,
                marginTop = 8,
                marginBottom = 4,
            },
        };

        header.Add(new Label(pluginName) {
            style = {
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = 13,
                width = 256,
            },
        });

        var objectField = new ObjectField { value = pluginSettings, objectType = pluginSettings.GetType() };
        objectField.SetEnabled(false);
        objectField.style.flexGrow = 1;
        objectField.style.minWidth = 256;
        header.Add(objectField);

        return header;
    }

    [SettingsProvider]
    public static SettingsProvider Create() {
        PluginSettingsManager.CleanupAssets();
        HashSet<string> keywords = new() {
            "Command Palette", "Blur", "Down Sample", "Size", "Passes",
            "Tint", "Color", "Amount", "Clear", "Selection", "Search",
        };
        foreach ((var provider, _) in PluginSettingsManager.Settings) {
            provider.AddKeywords(keywords);
        }
        return new CommandPaletteSettingsProvider(keywords);
    }
}
