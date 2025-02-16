using System;

namespace meerkat.Attributes;

/// <summary>
/// Specifies that the value of a field or property should be stored in lowercase.
/// This attribute can be used to enforce case insensitivity in MongoDB queries.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class LowercaseAttribute : Attribute { }
