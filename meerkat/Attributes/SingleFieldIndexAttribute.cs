using System;
using meerkat.Enums;

namespace meerkat.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SingleFieldIndexAttribute : Attribute
{
    public IndexOrder IndexOrder { get; set; } = IndexOrder.Ascending;

    public bool Sparse { get; set; } = false;
}
