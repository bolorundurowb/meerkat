# Meerkat

[![Build, Test & Coverage](https://github.com/bolorundurowb/meerkat/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/bolorundurowb/meerkat/actions/workflows/build-and-test.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE) ![NuGet Version](https://img.shields.io/nuget/v/meerkat) [![codecov](https://codecov.io/gh/bolorundurowb/meerkat/graph/badge.svg?token=E35WQFJ7IM)](https://codecov.io/gh/bolorundurowb/meerkat)

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

Apply the `[Collection]` attribute to control the collection name and timestamp tracking:

```csharp
[Collection(Name = "persons", TrackTimestamps = true)]
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
