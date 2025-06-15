using System.Collections.Generic;
using UnityEditor;
using Vecerdi.CommandPalette.Basic.Settings;
using Vecerdi.CommandPalette.PluginSupport;

namespace Vecerdi.CommandPalette.Basic;

public partial class CommandsPlugin : IPluginSettingsProvider<CommandsPluginSettings> {
    public void AddKeywords(HashSet<string> keywords) {
        keywords.Add("Search Cutoff");
    }

    public void DrawSettings(SerializedObject settings) {
        EditorGUILayout.PropertyField(settings.FindProperty(CommandsPluginSettings.SearchCutoffProperty));
        settings.ApplyModifiedProperties();
    }
}
