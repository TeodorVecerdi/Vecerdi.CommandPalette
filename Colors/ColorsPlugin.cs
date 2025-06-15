using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Vecerdi.CommandPalette.Core;
using Vecerdi.CommandPalette.PluginSupport;
using Vecerdi.CommandPalette.Resource;

namespace Vecerdi.CommandPalette.Colors;

public class ColorsPlugin : IPlugin, IResourcePathProvider {
    [InitializeOnLoadMethod]
    private static void InitializePlugin() {
        CommandPalette.RegisterPlugin(s_Plugin);
    }

    public static IResourcePathProvider ResourcePathProvider => s_Plugin;

    private static readonly ColorsPlugin s_Plugin = new();

    public string Name => "Color Converter";
    public float PriorityMultiplier => 2.0f;
    public CommandPaletteWindow Window { get; set; } = null!;

    public bool IsValid(Query query) {
        return ColorHelper.IsValid(query.Text);
    }

    public IEnumerable<ResultEntry> GetResults(Query query) {
        if (!ColorHelper.IsValid(query.Text)) {
            return [];
        }

        var color = ColorHelper.Extract(query.Text);
        if (!color.HasValue) {
            return [];
        }

        if (Math.Abs(color.Value.a - 1.0f) > 0.001f) {
            return GenerateAlphaResults(color.Value);
        }

        return GenerateColorResults(color.Value);
    }

    private static IEnumerable<ResultEntry> GenerateColorResults(Color color) {
        var r = (byte)Math.Round(color.r * 255.0f);
        var g = (byte)Math.Round(color.g * 255.0f);
        var b = (byte)Math.Round(color.b * 255.0f);
        Color.RGBToHSV(color, out var hsvH, out var hsvS, out var hsvV);
        ColorHelper.RgbToHsl(color, out var hslH, out var hslS, out var hslL);

        var hex = $"#{r:X2}{g:X2}{b:X2}";
        var rgb = $"rgb({r}, {g}, {b})";
        var hsv = $"hsv({hsvH * 360.0f}, {hsvS}, {hsvV})";
        var hsl = $"hsl({hslH * 360.0f}, {hslS}, {hslL})";

        yield return new ColorResultEntry(color, new ResultDisplaySettings(hex, null, "Copy to clipboard", IconResource.FromResource("Textures/Square.png")), 100, _ => {
            GUIUtility.systemCopyBuffer = hex;
            return true;
        });

        yield return new ColorResultEntry(color, new ResultDisplaySettings(rgb, null, "Copy to clipboard", IconResource.FromResource("Textures/Square.png")), 100, _ => {
            GUIUtility.systemCopyBuffer = rgb;
            return true;
        });

        yield return new ColorResultEntry(color, new ResultDisplaySettings(hsv, null, "Copy to clipboard", IconResource.FromResource("Textures/Square.png")), 100, _ => {
            GUIUtility.systemCopyBuffer = hsv;
            return true;
        });

        yield return new ColorResultEntry(color, new ResultDisplaySettings(hsl, null, "Copy to clipboard", IconResource.FromResource("Textures/Square.png")), 100, _ => {
            GUIUtility.systemCopyBuffer = hsl;
            return true;
        });
    }

    private static IEnumerable<ResultEntry> GenerateAlphaResults(Color color) {
        var r = (byte)Math.Round(color.r * 255.0f);
        var g = (byte)Math.Round(color.g * 255.0f);
        var b = (byte)Math.Round(color.b * 255.0f);
        var a = (byte)Math.Round(color.a * 255.0f);
        Color.RGBToHSV(color, out var hsvH, out var hsvS, out var hsvV);
        ColorHelper.RgbToHsl(color, out var hslH, out var hslS, out var hslL);

        var hex = $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        var rgba = $"rgba({r}, {g}, {b}, {color.a})";
        var hsva = $"hsv({hsvH * 360.0f}, {hsvS}, {hsvV}, {color.a})";
        var hsla = $"hsl({hslH * 360.0f}, {hslS}, {hslL}, {color.a})";

        yield return new ColorResultEntry(color, new ResultDisplaySettings(hex, null, "Copy to clipboard", IconResource.FromResource("Textures/Square.png")), 100, _ => {
            GUIUtility.systemCopyBuffer = hex;
            return true;
        });

        yield return new ColorResultEntry(color, new ResultDisplaySettings(rgba, null, "Copy to clipboard", IconResource.FromResource("Textures/Square.png")), 100, _ => {
            GUIUtility.systemCopyBuffer = rgba;
            return true;
        });

        yield return new ColorResultEntry(color, new ResultDisplaySettings(hsva, null, "Copy to clipboard", IconResource.FromResource("Textures/Square.png")), 100, _ => {
            GUIUtility.systemCopyBuffer = hsva;
            return true;
        });

        yield return new ColorResultEntry(color, new ResultDisplaySettings(hsla, null, "Copy to clipboard", IconResource.FromResource("Textures/Square.png")), 100, _ => {
            GUIUtility.systemCopyBuffer = hsla;
            return true;
        });
    }

    public string GetResourcePath(string path) {
        return Path.Combine(Path.GetDirectoryName(PathHelper())!.Replace("\\", "/").Replace(Application.dataPath, "Assets"), "EditorResources", path).Replace("\\", "/");
    }

    private static string PathHelper([CallerFilePath] string path = "") => path;
}
