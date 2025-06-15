using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Vecerdi.CommandPalette.Core;
using Vecerdi.CommandPalette.Math.Helpers;
using Vecerdi.CommandPalette.Math.Settings;
using Vecerdi.CommandPalette.PluginSupport;
using Vecerdi.CommandPalette.Resource;

namespace Vecerdi.CommandPalette.Math;

public partial class MathPlugin : IPlugin, IResourcePathProvider {
    [InitializeOnLoadMethod]
    private static void InitializePlugin() {
        CommandPalette.RegisterPlugin(s_Plugin);
        Settings = CommandPalette.GetSettings(s_Plugin);
    }

    public static IResourcePathProvider ResourcePathProvider => s_Plugin;

    private static readonly MathPlugin s_Plugin = new();
    private static readonly MathEngine s_Engine = new();

    internal static MathPluginSettings Settings { get; private set; } = null!;

    public string Name => "Math Engine";
    public float PriorityMultiplier => 2.0f;
    public CommandPaletteWindow Window { get; set; } = null!;

    public IEnumerable<ResultEntry> GetResults(Query query) {
        if (!IsValid(query)) return [];

        var text = query.Text.Trim();
        var priority = 100;

        if (text.StartsWith('=')) {
            text = text.Substring(1).Trim();
            priority = 10000;
        }

        try {
            // Using CurrentUICulture since this is user facing
            var result = s_Engine.Interpret(text, CultureInfo.CurrentUICulture);

            // This could happen for some incorrect queries, like pi(2)
            if (!result.HasValue) {
                return [];
            }

            return new List<ResultEntry> {
                CreateResult(result.Value, priority),
            };
        } // We want to keep the process alive if any the mages library throws any exceptions.
        catch (Exception) {
            // Debug.LogException(new Exception($"Exception in math plugin with query '{query.Text}'", e));
        }

        return new List<ResultEntry>();
    }

    private MathResultEntry CreateResult(CalculateResult calculateResult, int priority) {
        return new MathResultEntry(new ResultDisplaySettings(calculateResult.RoundedResult.ToString(CultureInfo.CurrentCulture), "", "Copy to clipboard", IconResource.FromResource("Textures/CalculatorIcon.png")), priority, CopyToClipboard) { UserData = calculateResult };
    }

    private static bool CopyToClipboard(ResultEntry result) {
        var value = (CalculateResult)result.UserData!;
        GUIUtility.systemCopyBuffer = value.Result.ToString(CultureInfo.CurrentCulture);

        return true;
    }

    public bool IsValid(Query query) {
        var text = query.Text.Trim();
        if (text.StartsWith('=')) {
            text = text.Substring(1).Trim();
        }

        return MathPluginHelper.InputValid(text);
    }

    public string GetResourcePath(string path) {
        return Path.Combine(Path.GetDirectoryName(PathHelper())!.Replace("\\", "/").Replace(Application.dataPath, "Assets"), "EditorResources", path).Replace("\\", "/");
    }

    private static string PathHelper([CallerFilePath] string path = "") => path;
}
