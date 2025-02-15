using System;

namespace meerkat.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class UniqueIndexAttribute : Attribute
{
    /// <summary>
    /// Determines how this index handles `null` values. If `true`, the index only references documents that have the
    /// indexed field. If `false`, the index references all documents, including those that are missing the indexed
    /// field. Defaults to `false`.
    /// </summary>
    public bool Sparse { get; set; } = false;
}
