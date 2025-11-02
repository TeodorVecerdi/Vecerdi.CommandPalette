using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Vecerdi.CommandPalette.Units.Helpers;

public static class UnitConversionHelper {
    // Tailwind font sizes to rem values
    private static readonly Dictionary<string, float> s_TailwindFontSizes = new() {
        { "text-xs", 0.75f },
        { "text-sm", 0.875f },
        { "text-base", 1f },
        { "text-lg", 1.125f },
        { "text-xl", 1.25f },
        { "text-2xl", 1.5f },
        { "text-3xl", 1.875f },
        { "text-4xl", 2.25f },
        { "text-5xl", 3f },
        { "text-6xl", 3.75f },
        { "text-7xl", 4.5f },
        { "text-8xl", 6f },
        { "text-9xl", 8f },
    };

    // Tailwind breakpoint sizes to rem values
    private static readonly Dictionary<string, float> s_TailwindSizes = new() {
        { "xs", 20.0f },
        { "sm", 24.0f },
        { "md", 28.0f },
        { "lg", 32.0f },
        { "xl", 36.0f },
        { "2xl", 42.0f },
        { "3xl", 48.0f },
        { "4xl", 56.0f },
        { "5xl", 64.0f },
        { "6xl", 72.0f },
        { "7xl", 80.0f },
    };

    private static readonly Regex s_UnitRegex = new(@"^(\d*\.?\d+)(px|rem)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static bool IsValidUnit(string input) {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var trimmedInput = input.Trim().ToLowerInvariant();
        return s_TailwindFontSizes.ContainsKey(trimmedInput) || s_TailwindSizes.ContainsKey(trimmedInput) || s_UnitRegex.IsMatch(trimmedInput);
    }

    public static UnitConversion? ParseUnit(string input, float remToPxRatio) {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        input = input.Trim().ToLowerInvariant();

        // Try parsing tailwind breakpoint size
        if (s_TailwindSizes.TryGetValue(input, out var remSizeValue)) {
            return new UnitConversion {
                RemValue = remSizeValue,
                PxValue = remSizeValue * remToPxRatio,
                TwValue = remSizeValue / 0.25f,
                InputType = UnitType.TailwindBreakpoint,
            };
        }

        // Try parsing tailwind font size
        if (s_TailwindFontSizes.TryGetValue(input, out var remValue)) {
            return new UnitConversion {
                RemValue = remValue,
                PxValue = remValue * remToPxRatio,
                TwValue = remValue / 0.25f,
                InputType = UnitType.TailwindFont,
            };
        }

        // Try parsing numeric unit
        var match = s_UnitRegex.Match(input);
        if (!match.Success)
            return null;

        if (!float.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return null;

        var unit = match.Groups[2].Value.ToLower();
        return unit switch {
            "px" => new UnitConversion {
                RemValue = value / remToPxRatio,
                PxValue = value,
                TwValue = value / remToPxRatio / 0.25f,
                InputType = UnitType.Px,
            },
            "rem" => new UnitConversion {
                RemValue = value,
                PxValue = value * remToPxRatio,
                TwValue = value / 0.25f,
                InputType = UnitType.Rem,
            },
            "" => new UnitConversion {
                RemValue = value * 0.25f,
                PxValue = value * 0.25f * remToPxRatio,
                TwValue = value,
                InputType = UnitType.Number,
            },
            _ => null,
        };
    }

    public static string GetClosestTailwindBreakpoint(float remValue, out bool isExactMatch) {
        var closestKey = "";
        var minDifference = float.MaxValue;

        foreach (var kvp in s_TailwindSizes) {
            var difference = Math.Abs(kvp.Value - remValue);
            if (difference < minDifference) {
                minDifference = difference;
                closestKey = kvp.Key;
            }
        }

        isExactMatch = Mathf.Approximately(minDifference, 0.0f);
        return closestKey;
    }

    public static string GetClosestTailwindFont(float remValue, out bool isExactMatch) {
        var closestKey = "text-base";
        var minDifference = float.MaxValue;

        foreach (var kvp in s_TailwindFontSizes) {
            var difference = Math.Abs(kvp.Value - remValue);
            if (difference < minDifference) {
                minDifference = difference;
                closestKey = kvp.Key;
            }
        }

        isExactMatch = Mathf.Approximately(minDifference, 0.0f);
        return closestKey;
    }
}

public class UnitConversion {
    public float RemValue { get; set; }
    public float PxValue { get; set; }
    public float TwValue { get; set; }
    public UnitType InputType { get; set; }
}

public enum UnitType {
    Px,
    Rem,
    Number,
    TailwindBreakpoint,
    TailwindFont,
}
