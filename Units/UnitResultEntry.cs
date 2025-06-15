using System;
using Vecerdi.CommandPalette.Core;
using Vecerdi.CommandPalette.Resource;
using Vecerdi.CommandPalette.Units.Helpers;

namespace Vecerdi.CommandPalette.Units;

public class UnitResultEntry : ResultEntry {
    public UnitConversion Conversion { get; }
    public string UnitValue { get; }

    public UnitResultEntry(UnitConversion conversion, string unitValue, ResultDisplaySettings displaySettings, int priority, Func<ResultEntry, bool>? onSelect) 
        : base(displaySettings, priority, onSelect, UnitsPlugin.ResourcePathProvider) {
        Conversion = conversion;
        UnitValue = unitValue;
    }
}