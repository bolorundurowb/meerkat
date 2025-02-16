using System;
using meerkat.Enums;

namespace meerkat.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class GeospatialIndexAttribute : Attribute
{
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the geospatial index. Available options: TwoD, TwoDSphere.
    /// </summary>
    public GeospatialIndexType IndexType { get; set; } = GeospatialIndexType.TwoD;
}
