using System;

namespace meerkat.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class LowercaseAttribute : Attribute { }
