# Meerkat

[![Build, Test & Coverage](https://github.com/bolorundurowb/meerkat/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/bolorundurowb/meerkat/actions/workflows/build-and-test.yml) [![codecov](https://codecov.io/gh/bolorundurowb/meerkat/graph/badge.svg?token=E35WQFJ7IM)](https://codecov.io/gh/bolorundurowb/meerkat) ![NuGet Version](https://img.shields.io/nuget/v/meerkat) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Meerkat** is an ODM (Object Document Mapper) for MongoDB in .NET. It wraps the official MongoDB driver and simplifies
common data access patterns, modelling, querying, persistence, and indexing with a clean, strongly-typed API.

## Installation

**.NET CLI**

```bash
dotnet add package meerkat
```

**Package Manager Console**

```powershell
Install-Package meerkat
```

**.csproj**

```xml

<PackageReference Include="meerkat" Version="2.0.1"/>
```

## Setup

Call `Connect` once at application startup, before any other Meerkat operation:

```csharp
using meerkat;

Meerkat.Connect("mongodb://user:password@host:port/database-name");
```

To create and verify attribute indexes at startup, call `EnsureIndexesAsync` right after `Connect`:

```csharp
await Meerkat.EnsureIndexesAsync(typeof(MyApp).Assembly);
```

This scans the assembly for concrete `Schema<TId>` types, creates any indexes declared via the
index attributes, and throws `IndexVerificationException` if an expected index is missing. See
[Indexing](#indexing).

## Defining Models

All models must inherit from `Schema<TId>`, where `TId` is the type of the document's unique identifier. The `Id`
property is automatically mapped as the MongoDB `_id` field.

```csharp
using meerkat;
using MongoDB.Bson;

public class Student : Schema<ObjectId>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public Student()
    {
        Id = ObjectId.GenerateNewId();
    }
}
```

Common ID types include `ObjectId`, `Guid`, `string`, and `int` any type that implements `IEquatable<T>`.

### Collection configuration

Apply the `[Collection]` attribute to control the collection name, timestamp tracking, and soft delete:

```csharp
[Collection(Name = "persons", TrackTimestamps = true, SoftDelete = true)]
public class Student : Schema<ObjectId>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
```

- **`Name`** - overrides the default collection name. Without this attribute, Meerkat uses a pluralized, lowercased
  version of the class name (e.g., `Student` → `students`).
- **`TrackTimestamps`** - when `true`, Meerkat automatically sets `CreatedAt` on first save and updates `UpdatedAt` on
  every subsequent save.
- **`SoftDelete`** - when `true`, `Remove*` sets `DeletedAt` instead of physically deleting, and queries, counts, and
  updates exclude those documents by default. See [Soft delete](#soft-delete).

## Querying

All static query methods on `Meerkat` require two type parameters: the schema type and the ID type.

### Find by ID

```csharp
var student = await Meerkat.FindByIdAsync<Student, ObjectId>(id);
// sync:
var student = Meerkat.FindById<Student, ObjectId>(id);
```

### Find one by predicate

```csharp
var student = await Meerkat.FindOneAsync<Student, ObjectId>(x => x.FirstName == "Ada");
// sync:
var student = Meerkat.FindOne<Student, ObjectId>(x => x.LastName == "Lovelace");
```

Omitting the predicate returns the first document in the collection.

### Find many by predicate

```csharp
var students = await Meerkat.FindAsync<Student, ObjectId>(x => x.LastName == "Lovelace");
// sync:
var students = Meerkat.Find<Student, ObjectId>(x => x.LastName == "Lovelace");
```

Omitting the predicate returns all documents.

### LINQ queries

For complex queries, use `Query<TSchema, TId>()` to get a LINQ-compatible `IQueryable`:

```csharp
var results = await Meerkat.Query<Student, ObjectId>()
    .Where(x => x.FirstName.StartsWith("A"))
    .OrderBy(x => x.LastName)
    .ToListAsync();
```

## Persistence

`Save` and `SaveAsync` perform an upsert: inserting the document if it does not exist, or replacing it if it does. The
match is done on `Id`.

```csharp
var student = new Student { FirstName = "Ada", LastName = "Lovelace" };

await student.SaveAsync();
// sync:
student.Save();
```

## Bulk Persistence

To save multiple entities in a single batched operation, use the `SaveAll` / `SaveAllAsync` extension methods from the
`meerkat.Collections` namespace:

```csharp
using meerkat.Collections;

var students = new[] { new Student(), new Student() };

await students.SaveAllAsync<Student, ObjectId>();
// sync:
students.SaveAll<Student, ObjectId>();
```

## Removal

### Remove by ID

```csharp
await Meerkat.RemoveByIdAsync<Student, ObjectId>(id);
// sync:
Meerkat.RemoveById<Student, ObjectId>(id);
```

### Remove first match

```csharp
await Meerkat.RemoveOneAsync<Student, ObjectId>(x => x.FirstName == "Ada");
// sync:
Meerkat.RemoveOne<Student, ObjectId>(x => x.FirstName == "Ada");
```

### Remove all matches

```csharp
await Meerkat.RemoveAsync<Student, ObjectId>(x => x.LastName == "Lovelace");
// sync:
Meerkat.Remove<Student, ObjectId>(x => x.LastName == "Lovelace");
```

## Soft delete

Opt in per collection with `[Collection(SoftDelete = true)]`. Meerkat stores `DeletedAt` (`DateTimeOffset?`) as a UTC
BSON date. `IsDeleted` is computed (`DeletedAt != null`) and is not persisted.

When enabled:

- `Query`, `Find*`, `FindById*`, `Count*`, `Exists*`, increment/decrement, and fluent `Update*` exclude documents with
  `DeletedAt` set.
- `Remove*` / `RemoveById*` / `RemoveOne*` set `DeletedAt` (and `UpdatedAt` when timestamps are tracked) instead of
  calling MongoDB `DeleteOne` / `DeleteMany`.
- A `DeletedAt` index (`deleted_at_idx`) is created automatically.

Pass `includeDeleted: true` to include soft-deleted documents. `Collection<TSchema, TId>()` is always unfiltered.

```csharp
var active = await Meerkat.FindByIdAsync<Student, ObjectId>(id);
var includingDeleted = await Meerkat.FindByIdAsync<Student, ObjectId>(id, includeDeleted: true);

var all = await Meerkat.Query<Student, ObjectId>(includeDeleted: true).ToListAsync();
long includingDeletedCount = await Meerkat.CountAsync<Student, ObjectId>(includeDeleted: true);
```

Restore a document, or permanently delete it:

```csharp
await Meerkat.RestoreByIdAsync<Student, ObjectId>(id);
await Meerkat.RestoreAsync<Student, ObjectId>(x => x.LastName == "Lovelace");

await Meerkat.HardRemoveByIdAsync<Student, ObjectId>(id);
await Meerkat.HardRemoveAsync<Student, ObjectId>(x => x.LastName == "Lovelace");

// instance helpers (same semantics as the static APIs)
await student.DeleteAsync();
await student.RestoreAsync();
```

`Restore*` is a no-op on types that do not set `SoftDelete = true`. Soft delete does **not** cascade to related
documents; Meerkat has no association graph.

Unique indexes still apply to soft-deleted rows. If you need uniqueness only among active documents, create a partial
unique index yourself.

## Atomic Updates

`Save` / `SaveAsync` replace the entire document. For concurrent or field-level writes, use the fluent updater or the increment/decrement helpers below. These issue MongoDB update operators (`$set`, `$unset`, `$push`, `$pull`, `$addToSet`, `$inc`) instead of a full replace.

### Fluent partial updates

Compose one or more field operations and execute them as a single atomic update. If the schema uses `[Collection(TrackTimestamps = true)]`, `UpdatedAt` is set to UTC now as part of the same write. `CreatedAt`, `PreSave`, `PostSave`, and case transforms are not applied.

```csharp
await Meerkat.Update<Student, ObjectId>(id)
    .Set(x => x.LastName, "Lovelace")
    .Unset(x => x.Nickname)
    .Push(x => x.Tags, "honour")
    .Pull(x => x.Tags, "draft")
    .AddToSet(x => x.Tags, "alumni")
    .Inc(x => x.Age, 1)
    .ExecuteAsync();

var updated = await Meerkat.Update<Student, ObjectId>(id)
    .Set(x => x.LastName, "Lovelace")
    .ExecuteAndGetUpdatedAsync();

await Meerkat.UpdateOne<Student, ObjectId>(x => x.FirstName == "Ada")
    .Set(x => x.LastName, "Lovelace")
    .ExecuteAsync();

await Meerkat.UpdateMany<Student, ObjectId>(x => x.Age < 18)
    .Set(x => x.Status, "minor")
    .ExecuteAsync();

var filter = Builders<Student>.Filter.Eq(x => x.Status, "pending");
await Meerkat.UpdateByFilter<Student, ObjectId>(filter)
    .Set(x => x.Status, "active")
    .ExecuteAsync();

await Meerkat.UpdateByFilter<Student, ObjectId>(filter, many: true)
    .Set(x => x.Status, "active")
    .ExecuteAsync();
```

`Execute` / `ExecuteAsync` return `UpdateResult`. `ExecuteAndGetUpdated` / `ExecuteAndGetUpdatedAsync` return the document after the update and cannot be used with `UpdateMany`.

### Increment/Decrement

Meerkat also provides dedicated atomic increment and decrement operations for numeric fields. Each method defaults the amount to `1` when not specified. Generic `TField` type parameter supports `int`, `long`, `double`, `decimal`, and any other numeric type accepted by MongoDB.

### Increment/Decrement by ID

```csharp
await Meerkat.IncrementByIdAsync<Student, ObjectId, int>(id, x => x.Age, 5);
Meerkat.IncrementById<Student, ObjectId, int>(id, x => x.Age);      // default amount = 1

await Meerkat.DecrementByIdAsync<Student, ObjectId, int>(id, x => x.Age, 3);
Meerkat.DecrementById<Student, ObjectId, int>(id, x => x.Age);      // default amount = 1
```

### Increment/Decrement by filter

```csharp
var filter = Builders<Student>.Filter.Eq(x => x.FirstName, "Ada");

await Meerkat.IncrementByFilterAsync<Student, ObjectId, int>(filter, x => x.Age, 5);
Meerkat.IncrementByFilter<Student, ObjectId, int>(filter, x => x.Age);

await Meerkat.DecrementByFilterAsync<Student, ObjectId, int>(filter, x => x.Age, 3);
Meerkat.DecrementByFilter<Student, ObjectId, int>(filter, x => x.Age);
```

### Increment/Decrement one by predicate

```csharp
await Meerkat.IncrementOneAsync<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age, 5);
Meerkat.IncrementOne<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age);

await Meerkat.DecrementOneAsync<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age, 3);
Meerkat.DecrementOne<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age);
```

### Increment/Decrement many by predicate

```csharp
await Meerkat.IncrementManyAsync<Student, ObjectId, int>(x => x.Age < 18, x => x.Age, 1);
Meerkat.IncrementMany<Student, ObjectId, int>(x => x.Age < 18, x => x.Age, 1);

await Meerkat.DecrementManyAsync<Student, ObjectId, int>(x => x.Age >= 65, x => x.Age, 1);
Meerkat.DecrementMany<Student, ObjectId, int>(x => x.Age >= 65, x => x.Age, 1);
```

### Increment/Decrement and get updated document

The `*AndGetUpdated` variants return the document after the update is applied:

```csharp
var updated = await Meerkat.IncrementByIdAndGetUpdatedAsync<Student, ObjectId, int>(id, x => x.Age, 5);
var updated = Meerkat.IncrementByIdAndGetUpdated<Student, ObjectId, int>(id, x => x.Age);

var updated = await Meerkat.DecrementByIdAndGetUpdatedAsync<Student, ObjectId, int>(id, x => x.Age, 3);
var updated = Meerkat.DecrementByIdAndGetUpdated<Student, ObjectId, int>(id, x => x.Age);

var updated = await Meerkat.IncrementOneAndGetUpdatedAsync<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age, 5);
var updated = Meerkat.IncrementOneAndGetUpdated<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age);

var updated = await Meerkat.DecrementOneAndGetUpdatedAsync<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age, 3);
var updated = Meerkat.DecrementOneAndGetUpdated<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age);
```

## Counting

```csharp
// total count
long total = await Meerkat.CountAsync<Student, ObjectId>();

// conditional count
long count = await Meerkat.CountAsync<Student, ObjectId>(x => x.FirstName.StartsWith("A"));

// sync variants
long total = Meerkat.Count<Student, ObjectId>();
long count = Meerkat.Count<Student, ObjectId>(x => x.FirstName.StartsWith("A"));
```

## Existence Checks

```csharp
bool any = await Meerkat.ExistsAsync<Student, ObjectId>();
bool match = await Meerkat.ExistsAsync<Student, ObjectId>(x => x.FirstName == "Ada");

// sync variants
bool any = Meerkat.Exists<Student, ObjectId>();
bool match = Meerkat.Exists<Student, ObjectId>(x => x.FirstName == "Ada");
```

## Indexing

Apply index attributes to model properties to have Meerkat automatically create the appropriate MongoDB indexes.

### Single-field index

```csharp
public class User : Schema<Guid>
{
    [SingleFieldIndex(Name = "username_idx", Sparse = true, IndexOrder = IndexOrder.Ascending)]
    public string Username { get; set; }
}
```

| Property     | Default     | Description                            |
|--------------|-------------|----------------------------------------|
| `Name`       | auto        | Custom index name                      |
| `Sparse`     | `false`     | Excludes documents missing the field   |
| `IndexOrder` | `Ascending` | `Ascending`, `Descending`, or `Hashed` |

### Unique index

```csharp
public class User : Schema<Guid>
{
    [UniqueIndex(Name = "email_idx", Sparse = true)]
    public string Email { get; set; }
}
```

| Property | Default | Description                          |
|----------|---------|--------------------------------------|
| `Name`   | auto    | Custom index name                    |
| `Sparse` | `false` | Excludes documents missing the field |

### Compound index

Fields sharing the same `Name` value are grouped into a single compound index:

```csharp
public class Order : Schema<Guid>
{
    [CompoundIndex(Name = "order_idx", IndexOrder = IndexOrder.Ascending)]
    public DateTime OrderDate { get; set; }

    [CompoundIndex(Name = "order_idx", IndexOrder = IndexOrder.Descending)]
    public decimal TotalAmount { get; set; }
}
```

Fields without a `Name` are grouped together into one unnamed compound index.

### Geospatial index

```csharp
public class Location : Schema<Guid>
{
    [GeospatialIndex(Name = "coords_idx", IndexType = GeospatialIndexType.TwoDSphere)]
    public double[] Coordinates { get; set; }
}
```

| Property    | Default | Description                                                                  |
|-------------|---------|------------------------------------------------------------------------------|
| `Name`      | auto    | Custom index name                                                            |
| `IndexType` | `TwoD`  | `TwoD` for flat geometry, `TwoDSphere` for spherical (Earth-surface) queries |

### Index summary

| Attribute          | Scope               | Key properties                       |
|--------------------|---------------------|--------------------------------------|
| `SingleFieldIndex` | Single property     | `Name`, `Sparse`, `IndexOrder`       |
| `UniqueIndex`      | Single property     | `Name`, `Sparse`                     |
| `CompoundIndex`    | Multiple properties | `Name` (groups fields), `IndexOrder` |
| `GeospatialIndex`  | Single property     | `Name`, `IndexType`                  |

### Verifying indexes at startup

Indexes are also created lazily on first collection access, but a type that is never queried never
gets its indexes. To guarantee indexes exist before the app serves traffic, call one of the
`EnsureIndexes` overloads after `Connect`:

```csharp
// a single type
await Meerkat.EnsureIndexesAsync<User, Guid>();

// a set of types
await Meerkat.EnsureIndexesAsync(new[] { typeof(User), typeof(Order) });

// every concrete Schema<TId> in an assembly
await Meerkat.EnsureIndexesAsync(typeof(User).Assembly);

// sync variants
Meerkat.EnsureIndexes<User, Guid>();
Meerkat.EnsureIndexes(typeof(User).Assembly);
```

`EnsureIndexes` creates the expected indexes (idempotently) and then lists the collection's indexes
to confirm each one is present. If an index is missing or could not be created, it throws
`IndexVerificationException` with the collection name and the missing indexes. Named indexes are
matched by name; unnamed indexes are matched by key pattern (and uniqueness/sparse where set).

## Transactions

Meerkat exposes ambient multi-document transactions. Writes and reads issued inside the callback
automatically join the transaction — no session parameter is required on the existing `Save`,
`Remove*`, `Update*`, `Increment*`, `Count*`, and `Query`/`Find*` APIs.

> **Note:** transactions require a replica set or sharded cluster. A standalone MongoDB instance
> cannot run transactions.

```csharp
await Meerkat.WithTransactionAsync(async _ =>
{
    var order = new Order { Number = "1001" };
    await order.SaveAsync();

    var line = new OrderLine { OrderId = order.Id, Qty = 2 };
    await line.SaveAsync();
});
```

- If the callback completes, the transaction commits.
- If the callback throws, the transaction rolls back all of its writes.
- Reads inside the callback see uncommitted writes made earlier in the same transaction.
- Nested `WithTransaction`/`WithTransactionAsync` calls throw `InvalidOperationException`.

Sync and returning overloads are also available:

```csharp
Meerkat.WithTransaction(() => { /* ... */ });

var result = await Meerkat.WithTransactionAsync(async _ =>
{
    // ...
    return 42;
});
```

## Data Transformations

Apply `[Lowercase]` or `[Uppercase]` to string properties to have their values automatically transformed before each
save. Both attributes are restricted to `string` properties and throw `InvalidAttributeException` if applied to any
other type.

```csharp
public class User : Schema<Guid>
{
    [Lowercase]
    public string Email { get; set; }

    [Uppercase]
    public string CountryCode { get; set; }
}
```

## Lifecycle Hooks

Override `PreSave` and `PostSave` on any model to run custom logic before or after persistence:

```csharp
public class Student : Schema<ObjectId>
{
    public string FirstName { get; set; }

    public override void PreSave()
    {
        // runs after timestamp updates and transformations, before the database write
        FirstName = FirstName?.Trim();
    }

    public override void PostSave()
    {
        // runs after the database write completes
    }
}
```

## Breaking Changes

### v2.0.0

- The MongoDB driver was upgraded to **3.x**.
- The base class `Schema` was replaced by the generic `Schema<TId>`, requiring an explicit ID type parameter on all
  models and on all static `Meerkat` methods.

## Contributing

Issues and pull requests are welcome at [github.com/bolorundurowb/meerkat](https://github.com/bolorundurowb/meerkat).
