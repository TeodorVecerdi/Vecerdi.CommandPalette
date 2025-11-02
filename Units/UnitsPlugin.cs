using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Vecerdi.CommandPalette.Core;
using Vecerdi.CommandPalette.PluginSupport;
using Vecerdi.CommandPalette.Resource;
using Vecerdi.CommandPalette.Units.Helpers;
using Vecerdi.CommandPalette.Units.Settings;

namespace Vecerdi.CommandPalette.Units;

public partial class UnitsPlugin : IPlugin, IResourcePathProvider {
    [InitializeOnLoadMethod]
    private static void InitializePlugin() {
        CommandPalette.RegisterPlugin(s_Plugin);
        Settings = CommandPalette.GetSettings(s_Plugin);
    }

    public static IResourcePathProvider ResourcePathProvider => s_Plugin;

    private static readonly UnitsPlugin s_Plugin = new();

    internal static UnitConversionSettings Settings { get; private set; } = null!;

    public string Name => "Unit Converter";
    public float PriorityMultiplier => 2.0f;
    public CommandPaletteWindow Window { get; set; } = null!;

    public bool IsValid(Query query) {
        return UnitConversionHelper.IsValidUnit(query.Text);
    }

    public IEnumerable<ResultEntry> GetResults(Query query) {
        if (!IsValid(query))
            return [];

        var conversion = UnitConversionHelper.ParseUnit(query.Text, Settings.RemToPxRatio);
        if (conversion == null)
            return [];

        return GenerateConversionResults(conversion);
    }

    private static IEnumerable<ResultEntry> GenerateConversionResults(UnitConversion conversion) {
        var results = new List<ResultEntry>();

        // Pixels
        var pxValue = $"{conversion.PxValue:0.##}px";
        var pxValueUnitless = $"{conversion.PxValue:0.##}";
        if (conversion.InputType != UnitType.Px) {
            results.Add(new UnitResultEntry(
                conversion,
                pxValue,
                pxValueUnitless,
                new ResultDisplaySettings(pxValue, null, "Copy to clipboard", IconResource.FromResource("Textures/UnitIcon.png")),
                100,
                CopyToClipboard
            ));
        }

        // Rem
        var remValue = $"{conversion.RemValue:0.####}rem";
        var remValueUnitless = $"{conversion.RemValue:0.####}";
        if (conversion.InputType != UnitType.Rem) {
            results.Add(new UnitResultEntry(
                conversion,
                remValue,
                remValueUnitless,
                new ResultDisplaySettings(remValue, null, "Copy to clipboard", IconResource.FromResource("Textures/UnitIcon.png")),
                100,
                CopyToClipboard
            ));
        }

        // Tailwind spacing
        var twValue = $"{conversion.TwValue:0.##}";
        var twValueUnitless = $"{conversion.TwValue:0.##}";
        if (conversion.InputType != UnitType.Number) {
            results.Add(new UnitResultEntry(
                conversion,
                twValue,
                twValueUnitless,
                new ResultDisplaySettings(twValue, null, "Copy to clipboard", IconResource.FromResource("Textures/UnitIcon.png")),
                100,
                CopyToClipboard
            ));
        }

        // Closest Tailwind breakpoint
        var closestTwBreakpoint = UnitConversionHelper.GetClosestTailwindBreakpoint(conversion.RemValue, out var isExactMatch);
        if (conversion.InputType != UnitType.TailwindBreakpoint) {
            results.Add(new UnitResultEntry(
                conversion,
                closestTwBreakpoint,
                closestTwBreakpoint,
                new ResultDisplaySettings(closestTwBreakpoint, isExactMatch ? null : "Closest Tailwind breakpoint size", "Copy to clipboard", IconResource.FromResource("Textures/UnitIcon.png")),
                90,
                CopyToClipboard
            ));
        }

        // Closest Tailwind font size
        var closestTwFont = UnitConversionHelper.GetClosestTailwindFont(conversion.RemValue, out isExactMatch);
        if (conversion.InputType != UnitType.TailwindFont) {
            results.Add(new UnitResultEntry(
                conversion,
                closestTwFont,
                closestTwFont,
                new ResultDisplaySettings(closestTwFont, isExactMatch ? null : "Closest Tailwind font size", "Copy to clipboard", IconResource.FromResource("Textures/UnitIcon.png")),
                90,
                CopyToClipboard
            ));
        }

        return results;
    }

    private static bool CopyToClipboard(ResultEntry result) {
        if (result is UnitResultEntry unitResult) {
            GUIUtility.systemCopyBuffer = unitResult.UnitlessValue;
            return true;
        }

        return false;
    }

    public string GetResourcePath(string path) {
        return Path.Combine(Path.GetDirectoryName(PathHelper())!.Replace("\\", "/").Replace(Application.dataPath, "Assets"), "EditorResources", path).Replace("\\", "/");
    }

    private static string PathHelper([CallerFilePath] string path = "") => path;
}
