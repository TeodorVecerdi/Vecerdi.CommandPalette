using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vecerdi.CommandPalette.PluginSupport;

public interface IPluginSettingsProvider {
    Type SettingsType { get; }
    VisualElement CreateSettingsUI(SerializedObject settings);
    void AddKeywords(HashSet<string> keywords);
}

public interface IPluginSettingsProvider<T> : IPluginSettingsProvider where T : ScriptableObject {
    Type IPluginSettingsProvider.SettingsType => typeof(T);
}
