using System;
using meerkat.Enums;

namespace meerkat.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class CompoundIndexAttribute : Attribute
{
    public IndexOrder IndexOrder { get; set; } = IndexOrder.Ascending;
}
