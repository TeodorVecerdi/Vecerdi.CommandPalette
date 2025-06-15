using System.Collections.Generic;
using UnityEditor;
using Vecerdi.CommandPalette.Math.Settings;
using Vecerdi.CommandPalette.PluginSupport;

namespace Vecerdi.CommandPalette.Math;

public partial class MathPlugin : IPluginSettingsProvider<MathPluginSettings> {
    public void AddKeywords(HashSet<string> keywords) {
        keywords.Add("Decimal Places");
    }

    public void DrawSettings(SerializedObject settings) {
        EditorGUILayout.PropertyField(settings.FindProperty(MathPluginSettings.DisplayDecimalPlacesProperty));
        EditorGUILayout.PropertyField(settings.FindProperty(MathPluginSettings.CopyDecimalPlacesProperty));
        settings.ApplyModifiedProperties();
    }
}
