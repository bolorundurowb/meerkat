namespace meerkat.Enums;

/// <summary>
/// Represents the types of geospatial indexes available in MongoDB.
/// </summary>
public enum GeospatialIndexType
{
    /// <summary>
    /// A 2D index used for indexing planar geometry (e.g., latitude and longitude on a flat surface).
    /// </summary>
    TwoD,

    /// <summary>
    /// A 2D sphere index used for indexing spherical geometry.
    /// </summary>
    TwoDSphere,
}
