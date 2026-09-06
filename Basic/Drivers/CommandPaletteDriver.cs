using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEngine;
using Vecerdi.CommandPalette.Basic.Attributes;
using Vecerdi.CommandPalette.Basic.Data;

namespace Vecerdi.CommandPalette.Basic.Drivers;

[NoAutoStaticsCleanup]
public static class CommandPaletteDriver {
    public static List<CommandEntry> CommandEntries { get; } = [];
    public static Dictionary<string, MethodInfo> ParameterValueProviders { get; private set; } = null!;

    [InitializeOnLoadMethod]
    private static void InitializeDriver() {
        ParameterValueProviders = TypeCache.GetMethodsWithAttribute<InlineParameterValuesProviderAttribute>().ToDictionary(info => info.Name);
        var validateMethods = TypeCache.GetMethodsWithAttribute<CommandValidateMethodAttribute>().ToDictionary(info => info.Name);
        IEnumerable<MethodInfo> methods = TypeCache.GetMethodsWithAttribute<CommandAttribute>();

        foreach (var method in methods) {
            var attribute = method.GetCustomAttribute<CommandAttribute>();
            var displayName = string.IsNullOrWhiteSpace(attribute.DisplayName) ? ObjectNames.NicifyVariableName(method.Name) : attribute.DisplayName!;
            var shortName = string.IsNullOrWhiteSpace(attribute.ShortName) ? GetShortName(ObjectNames.NicifyVariableName(displayName)) : attribute.ShortName!;
            MethodInfo? validationMethod = null;
            if (!string.IsNullOrEmpty(attribute.ValidationMethod)) {
                if (!validateMethods.TryGetValue(attribute.ValidationMethod!, out validationMethod)) {
                    Debug.LogError($"Could not find validation method {attribute.ValidationMethod} for command {displayName}");
                }

                if (validationMethod != null && validationMethod.ReturnType != typeof(bool)) {
                    Debug.LogError($"Validation method {attribute.ValidationMethod} for command {displayName} must return a bool. Located at {validationMethod.DeclaringType!.FullName}.{validationMethod.Name}");
                    validationMethod = null;
                }
            }

            CommandEntries.Add(new CommandEntry(displayName, shortName, attribute.Description, attribute.ShowOnlyWhenSearching, method, validationMethod, attribute.Icon, attribute.Priority));
        }
    }

    private static string GetShortName(string name) {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        StringBuilder builder = new();
        foreach (var part in parts) {
            builder.Append(part[0]);
        }

        return builder.ToString();
    }
}
