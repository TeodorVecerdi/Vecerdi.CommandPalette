using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.PluginSupport;
using Vecerdi.CommandPalette.Units.Settings;

namespace Vecerdi.CommandPalette.Units;

public partial class UnitsPlugin : IPluginSettingsProvider<UnitConversionSettings> {
    public void AddKeywords(HashSet<string> keywords) {
        keywords.Add("Rem to Pixels");
        keywords.Add("Unit Conversion");
        keywords.Add("Tailwind");
    }

    public VisualElement CreateSettingsUI(SerializedObject settings) {
        var container = new VisualElement();
        container.Add(new PropertyField(settings.FindProperty(UnitConversionSettings.RemToPxRatioProperty)));
        return container;
    }
}
