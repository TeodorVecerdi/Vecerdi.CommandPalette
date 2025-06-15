using System;
using Vecerdi.CommandPalette.Core;
using Vecerdi.CommandPalette.Units.Helpers;

namespace Vecerdi.CommandPalette.Units;

public class UnitResultEntry(UnitConversion conversion, string unitValue, string unitlessValue, ResultDisplaySettings displaySettings, int priority, Func<ResultEntry, bool>? onSelect) : ResultEntry(displaySettings, priority, onSelect, UnitsPlugin.ResourcePathProvider) {
    public UnitConversion Conversion { get; } = conversion;
    public string UnitValue { get; } = unitValue;
    public string UnitlessValue { get; } = unitlessValue;
}
