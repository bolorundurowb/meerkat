using System;

namespace meerkat.Attributes;

/// <summary>
/// Specifies metadata for a MongoDB collection.
/// This attribute is used to define the collection name, timestamp tracking, and soft-delete behaviour.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CollectionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the MongoDB collection.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether timestamps should be tracked for documents in the collection.
    /// </summary>
    public bool TrackTimestamps { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether documents in the collection use soft delete.
    /// When true, <c>Remove*</c> sets <c>DeletedAt</c> instead of physically deleting, and queries exclude deleted documents by default.
    /// </summary>
    public bool SoftDelete { get; set; }
}
