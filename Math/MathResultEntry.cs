using System;
using UnityEngine.UIElements;
using Vecerdi.CommandPalette.Core;
using Vecerdi.CommandPalette.Resource;

namespace Vecerdi.CommandPalette.Math;

public class MathResultEntry(ResultDisplaySettings displaySettings, int score, Func<ResultEntry, bool> onSelect) : ResultEntry(displaySettings, score, onSelect, MathPlugin.ResourcePathProvider) {
    private static StyleSheet? s_Stylesheet;

    public override void PostProcessVisualElement(VisualElement element) {
        if (s_Stylesheet == null) {
            s_Stylesheet = ResourceLoader.Load<StyleSheet>("StyleSheets/MathResultEntry.uss", MathPlugin.ResourcePathProvider);
        }

        if (s_Stylesheet != null) {
            element.styleSheets.Add(s_Stylesheet);
        }

        element.Q<VisualElement>(null, "result-entry-icon").AddToClassList("math-icon");
        element.AddToClassList("math-result-entry");
    }
}
