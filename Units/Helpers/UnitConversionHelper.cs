using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Vecerdi.CommandPalette.Units.Helpers;

public static class UnitConversionHelper {
    // Tailwind spacing scale (matches default spacing scale)
    private static readonly Dictionary<int, float> TailwindSpacing = new() {
        { 0, 0f }, { 1, 0.25f }, { 2, 0.5f }, { 3, 0.75f }, { 4, 1f }, { 5, 1.25f },
        { 6, 1.5f }, { 7, 1.75f }, { 8, 2f }, { 9, 2.25f }, { 10, 2.5f }, { 11, 2.75f },
        { 12, 3f }, { 14, 3.5f }, { 16, 4f }, { 20, 5f }, { 24, 6f }, { 28, 7f },
        { 32, 8f }, { 36, 9f }, { 40, 10f }, { 44, 11f }, { 48, 12f }, { 52, 13f },
        { 56, 14f }, { 60, 15f }, { 64, 16f }, { 72, 18f }, { 80, 20f }, { 96, 24f }
    };

    // Tailwind font sizes
    private static readonly Dictionary<string, float> TailwindFontSizes = new() {
        { "text-xs", 0.75f }, { "text-sm", 0.875f }, { "text-base", 1f }, { "text-lg", 1.125f },
        { "text-xl", 1.25f }, { "text-2xl", 1.5f }, { "text-3xl", 1.875f }, { "text-4xl", 2.25f },
        { "text-5xl", 3f }, { "text-6xl", 3.75f }, { "text-7xl", 4.5f }, { "text-8xl", 6f }, { "text-9xl", 8f }
    };

    private static readonly Regex UnitRegex = new(@"^(\d*\.?\d+)(px|rem|tw)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex TailwindFontRegex = new(@"^text-(xs|sm|base|lg|xl|\d+xl)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static bool IsValidUnit(string input) {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return UnitRegex.IsMatch(input.Trim()) || TailwindFontRegex.IsMatch(input.Trim());
    }

    public static UnitConversion? ParseUnit(string input, float remToPxRatio) {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim();

        // Try parsing tailwind font size first
        var fontMatch = TailwindFontRegex.Match(input);
        if (fontMatch.Success) {
            if (TailwindFontSizes.TryGetValue(input.ToLower(), out var remValue)) {
                return new UnitConversion {
                    OriginalInput = input,
                    RemValue = remValue,
                    PxValue = remValue * remToPxRatio,
                    TwValue = remValue / 0.25f,
                    InputType = UnitType.TailwindFont
                };
            }
        }

        // Try parsing numeric unit
        var match = UnitRegex.Match(input);
        if (!match.Success)
            return null;

        if (!float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return null;

        var unit = match.Groups[2].Value.ToLower();

        return unit switch {
            "px" => new UnitConversion {
                OriginalInput = input,
                RemValue = value / remToPxRatio,
                PxValue = value,
                TwValue = (value / remToPxRatio) / 0.25f,
                InputType = UnitType.Px
            },
            "rem" => new UnitConversion {
                OriginalInput = input,
                RemValue = value,
                PxValue = value * remToPxRatio,
                TwValue = value / 0.25f,
                InputType = UnitType.Rem
            },
            "tw" => new UnitConversion {
                OriginalInput = input,
                RemValue = value * 0.25f,
                PxValue = value * 0.25f * remToPxRatio,
                TwValue = value,
                InputType = UnitType.Tw
            },
            "" => new UnitConversion {
                OriginalInput = input,
                RemValue = value / remToPxRatio,
                PxValue = value,
                TwValue = (value / remToPxRatio) / 0.25f,
                InputType = UnitType.Number
            },
            _ => null
        };
    }

    public static string GetClosestTailwindSpacing(float twValue) {
        var closestKey = 0;
        var minDifference = float.MaxValue;
        var remValue = twValue * 0.25f; // Convert tw to rem

        foreach (var kvp in TailwindSpacing) {
            var difference = Math.Abs(kvp.Value - remValue);
            if (difference < minDifference) {
                minDifference = difference;
                closestKey = kvp.Key;
            }
        }

        return closestKey.ToString();
    }

    public static string GetClosestTailwindFont(float remValue) {
        var closestKey = "text-base";
        var minDifference = float.MaxValue;

        foreach (var kvp in TailwindFontSizes) {
            var difference = Math.Abs(kvp.Value - remValue);
            if (difference < minDifference) {
                minDifference = difference;
                closestKey = kvp.Key;
            }
        }

        return closestKey;
    }
}

public class UnitConversion {
    public string OriginalInput { get; set; } = string.Empty;
    public float RemValue { get; set; }
    public float PxValue { get; set; }
    public float TwValue { get; set; }
    public UnitType InputType { get; set; }
}

public enum UnitType {
    Px,
    Rem,
    Tw,
    Number,
    TailwindFont
}