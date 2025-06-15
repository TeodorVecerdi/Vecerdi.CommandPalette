using System.Collections.Generic;
using UnityEditor;
using Vecerdi.CommandPalette.PluginSupport;
using Vecerdi.CommandPalette.Units.Settings;

namespace Vecerdi.CommandPalette.Units;

public partial class UnitsPlugin : IPluginSettingsProvider<UnitConversionSettings> {
    public void AddKeywords(HashSet<string> keywords) {
        keywords.Add("Rem to Pixels");
        keywords.Add("Unit Conversion");
        keywords.Add("Tailwind");
    }

    public void DrawSettings(SerializedObject settings) {
        EditorGUILayout.PropertyField(settings.FindProperty(UnitConversionSettings.RemToPxRatioProperty));
        settings.ApplyModifiedProperties();
    }
}