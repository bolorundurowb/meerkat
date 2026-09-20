# Indexing

Apply index attributes directly to model properties to configure MongoDB indexes declaratively. Meerkat creates these indexes automatically on first collection access, or explicitly at application startup.

---

## Single-Field Indexes

Apply `[SingleFieldIndex]` to index a single property:

```csharp
public class User : Schema<Guid>
{
    [SingleFieldIndex(Name = "username_idx", Sparse = true, IndexOrder = IndexOrder.Ascending)]
    public string Username { get; set; } = string.Empty;

    [SingleFieldIndex(ExpireAfter = "30d")]
    public DateTime CreatedAtUtc { get; set; }
}
```

### Options

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | Auto | Custom index name in MongoDB. |
| `Sparse` | `bool` | `false` | When `true`, excludes documents that do not contain the indexed field. |
| `IndexOrder` | `IndexOrder` | `Ascending` | Index sorting direction: `Ascending`, `Descending`, or `Hashed`. |
| `ExpireAfter` | `string` | `null` | TTL duration string (e.g. `"30d"`, `"12h"`, `"15m"`, `"1s"`, `"500ms"`, `"2w"`, or ISO-8601). Only valid on `DateTime` or `DateTime?` properties. |

---

## Unique Indexes

Apply `[UniqueIndex]` to enforce uniqueness on a single property:

```csharp
public class User : Schema<Guid>
{
    [UniqueIndex(Name = "email_idx", Sparse = true)]
    public string Email { get; set; } = string.Empty;
}
```

### Options

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | Auto | Custom index name in MongoDB. |
| `Sparse` | `bool` | `false` | When `true`, allows multiple documents with missing or null values. |

---

## Compound Indexes

Properties sharing the same `Name` value are combined into a single compound index:

```csharp
public class Order : Schema<Guid>
{
    [CompoundIndex(Name = "order_idx", IndexOrder = IndexOrder.Ascending, Unique = true)]
    public DateTime OrderDate { get; set; }

    [CompoundIndex(Name = "order_idx", IndexOrder = IndexOrder.Descending, Unique = true)]
    public decimal TotalAmount { get; set; }
}
```

### Options

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | Auto | Index name that groups fields together into a compound index. |
| `IndexOrder` | `IndexOrder` | `Ascending` | Direction: `Ascending`, `Descending`, or `Hashed`. |
| `Unique` | `bool` | `false` | When `true`, enforces compound uniqueness across all fields in the index. All members in the same named group must specify matching `Unique` settings. |

!!! note
    Fields without an explicit `Name` are grouped together into a default unnamed compound index.

---

## Geospatial Indexes

Apply `[GeospatialIndex]` for 2D or 2DSphere spatial queries:

```csharp
public class Location : Schema<Guid>
{
    [GeospatialIndex(Name = "coords_idx", IndexType = GeospatialIndexType.TwoDSphere)]
    public double[] Coordinates { get; set; } = Array.Empty<double>();
}
```

### Options

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | Auto | Custom index name in MongoDB. |
| `IndexType` | `GeospatialIndexType` | `TwoD` | Index type: `TwoD` for flat geometry, or `TwoDSphere` for Earth-surface spherical coordinates. |

---

## Index Summary

| Attribute | Scope | Key Properties |
| :--- | :--- | :--- |
| `[SingleFieldIndex]` | Single property | `Name`, `Sparse`, `IndexOrder`, `ExpireAfter` |
| `[UniqueIndex]` | Single property | `Name`, `Sparse` |
| `[CompoundIndex]` | Multiple properties | `Name` (grouping key), `IndexOrder`, `Unique` |
| `[GeospatialIndex]` | Single property | `Name`, `IndexType` |

---

## Verifying Indexes at Startup

While Meerkat automatically creates indexes when a collection is first accessed at runtime, types that are not accessed immediately will not have their indexes created until accessed.

To guarantee that all indexes exist before serving application traffic, call `EnsureIndexes` or `EnsureIndexesAsync` right after `Connect`:

```csharp
// Verify a single type
await Meerkat.EnsureIndexesAsync<User, Guid>();

// Verify a set of types
await Meerkat.EnsureIndexesAsync(new[] { typeof(User), typeof(Order) });

// Verify all Schema<TId> types in an assembly
await Meerkat.EnsureIndexesAsync(typeof(User).Assembly);

// Synchronous variants
Meerkat.EnsureIndexes<User, Guid>();
Meerkat.EnsureIndexes(typeof(User).Assembly);
```

### Verification Behavior

`EnsureIndexes` creates the expected indexes (idempotently) and checks the collection's indexes in MongoDB to verify that each declared index exists.

If an expected index is missing or could not be created, Meerkat throws an `IndexVerificationException` containing details about the collection and missing indexes.
