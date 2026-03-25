using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.Math.Settings;
using Vecerdi.CommandPalette.PluginSupport;

namespace Vecerdi.CommandPalette.Math;

public partial class MathPlugin : IPluginSettingsProvider<MathPluginSettings> {
    public void AddKeywords(HashSet<string> keywords) {
        keywords.Add("Decimal Places");
    }

    public VisualElement CreateSettingsUI(SerializedObject settings) {
        var container = new VisualElement();
        container.Add(new PropertyField(settings.FindProperty(MathPluginSettings.DisplayDecimalPlacesProperty)));
        container.Add(new PropertyField(settings.FindProperty(MathPluginSettings.CopyDecimalPlacesProperty)));
        return container;
    }
}
