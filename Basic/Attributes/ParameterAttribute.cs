using System;

namespace Vecerdi.CommandPalette.Basic.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class ParameterAttribute : Attribute {
    public string? Name { get; set; }
    public string? Description { get; set; }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class InlineParameterAttribute(string valuesMethod) : Attribute {
    public string ValuesMethod { get; } = valuesMethod;
}
