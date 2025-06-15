using System;
using JetBrains.Annotations;

namespace Vecerdi.CommandPalette.Basic.Attributes;

[AttributeUsage(AttributeTargets.Method), MeansImplicitUse]
public class InlineParameterValuesProviderAttribute : Attribute { }
