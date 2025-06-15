using System;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.Resource;

namespace Vecerdi.CommandPalette.Core;

public class ResultEntry {
    public ResultDisplaySettings DisplaySettings { get; }
    public int Score { get; }
    public Func<ResultEntry, bool>? OnSelect { get; }
    public object? UserData { get; set; }
    public IResourcePathProvider? ResourcePathProvider { get; }

    public ResultEntry(ResultDisplaySettings displaySettings, int score, Func<ResultEntry, bool>? onSelect, IResourcePathProvider? resourcePathProvider = null) {
        DisplaySettings = displaySettings;
        Score = score;
        OnSelect = onSelect;
        ResourcePathProvider = resourcePathProvider;
    }

    public virtual void PostProcessVisualElement(VisualElement element) { }
}
