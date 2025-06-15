using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Vecerdi.CommandPalette.PluginSupport;

public interface IPluginSettingsProvider {
    Type SettingsType { get; }
    void DrawSettings(SerializedObject settings);
    void AddKeywords(HashSet<string> keywords);
}

public interface IPluginSettingsProvider<T> : IPluginSettingsProvider where T : ScriptableObject {
    Type IPluginSettingsProvider.SettingsType => typeof(T);
}
