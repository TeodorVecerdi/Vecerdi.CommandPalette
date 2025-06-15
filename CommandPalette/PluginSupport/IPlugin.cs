using System.Collections.Generic;
using Vecerdi.CommandPalette.Core;

namespace Vecerdi.CommandPalette.PluginSupport;

public interface IPlugin {
    string Name { get; }
    float PriorityMultiplier { get; }
    CommandPaletteWindow Window { get; set; }

    bool IsValid(Query query);
    IEnumerable<ResultEntry> GetResults(Query query);
}
