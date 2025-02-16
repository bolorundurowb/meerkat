using System;

namespace meerkat.Attributes;

/// <summary>
/// Specifies metadata for a MongoDB collection.
/// This attribute is used to define the collection name and whether timestamps should be tracked.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CollectionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the MongoDB collection.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether timestamps should be tracked for documents in the collection.
    /// </summary>
    public bool TrackTimestamps { get; set; }
}
