using System;
using UnityEngine;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.Core;

namespace Vecerdi.CommandPalette.Colors;

public class ColorResultEntry(Color color, ResultDisplaySettings displaySettings, int score, Func<ResultEntry, bool> onSelect) : ResultEntry(displaySettings, score, onSelect, ColorsPlugin.ResourcePathProvider) {
    public override void PostProcessVisualElement(VisualElement element) {
        var resultIcon = element.Q<VisualElement>(null, "result-entry-icon");
        resultIcon.AddToClassList("color-icon");
        resultIcon.style.unityBackgroundImageTintColor = color;
    }
}
