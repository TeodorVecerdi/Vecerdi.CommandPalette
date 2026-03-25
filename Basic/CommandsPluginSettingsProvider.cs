using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.Basic.Settings;
using Vecerdi.CommandPalette.PluginSupport;

namespace Vecerdi.CommandPalette.Basic;

public partial class CommandsPlugin : IPluginSettingsProvider<CommandsPluginSettings> {
    public void AddKeywords(HashSet<string> keywords) {
        keywords.Add("Search Cutoff");
    }

    public VisualElement CreateSettingsUI(SerializedObject settings) {
        var container = new VisualElement();
        container.Add(new PropertyField(settings.FindProperty(CommandsPluginSettings.SearchCutoffProperty)));
        return container;
    }
}
