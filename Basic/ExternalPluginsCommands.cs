using System;
using System.Reflection;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;
using Vecerdi.CommandPalette.Basic.Attributes;

namespace Vecerdi.CommandPalette.Basic;

[NoAutoStaticsCleanup]
public static class ExternalPluginsCommands {
    private static readonly MethodInfo? s_ConsolePro3WindowCreateWindow = Type.GetType("FlyingWormConsole3.ConsolePro3Window, ConsolePro.Editor")?.GetMethod("CreateWindow", BindingFlags.Static | BindingFlags.Public);

    [CommandValidateMethod] private static bool ShowConsolePro3Commands() => s_ConsolePro3WindowCreateWindow != null;

    [Command(ValidationMethod = nameof(ShowConsolePro3Commands), IconPath = "r:console.infoicon")]
    private static void OpenConsolePro3() {
        if (s_ConsolePro3WindowCreateWindow == null) {
            Debug.LogError("ConsolePro3 is not in project or was not found.");
            return;
        }

        s_ConsolePro3WindowCreateWindow.Invoke(null, null);
    }
}
