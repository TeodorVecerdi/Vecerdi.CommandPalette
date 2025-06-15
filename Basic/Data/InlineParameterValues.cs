using System.Collections.Generic;

namespace Vecerdi.CommandPalette.Basic.Data;

public class InlineParameterValues : List<InlineParameterResultEntry> {
    public InlineParameterValues() { }

    public InlineParameterValues(IEnumerable<InlineParameterResultEntry> collection) : base(collection) { }
}

public sealed class InlineParameterValues<T> : InlineParameterValues {
    public InlineParameterValues() { }

    public InlineParameterValues(IEnumerable<InlineParameterResultEntry<T>> collection) : base(collection) { }
}
