using System;

namespace meerkat.Attributes;

/// <summary>
/// Specifies that the value of a field or property should be stored in uppercase.
/// This attribute can be used to enforce case consistency in MongoDB queries.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class UppercaseAttribute : Attribute { }
