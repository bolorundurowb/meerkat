namespace meerkat.Enums;

/// <summary>
/// Represents the order types for indexes in MongoDB.
/// </summary>
public enum IndexOrder
{
    /// <summary>
    /// Specifies an ascending index order (1).
    /// Used for sorting data in increasing order.
    /// </summary>
    Ascending,

    /// <summary>
    /// Specifies a descending index order (-1).
    /// Used for sorting data in decreasing order.
    /// </summary>
    Descending,

    /// <summary>
    /// Specifies a hashed index.
    /// Used for sharding and indexing fields with hashed values.
    /// </summary>
    Hashed
}
