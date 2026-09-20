# Meerkat

[![Build, Test & Coverage](https://github.com/bolorundurowb/meerkat/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/bolorundurowb/meerkat/actions/workflows/build-and-test.yml)
[![codecov](https://codecov.io/gh/bolorundurowb/meerkat/graph/badge.svg?token=E35WQFJ7IM)](https://codecov.io/gh/bolorundurowb/meerkat)
![NuGet Version](https://img.shields.io/nuget/v/meerkat)
[![Licence: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/bolorundurowb/meerkat/blob/master/LICENSE)

**Meerkat** is an Object Document Mapper (ODM) for MongoDB in .NET. It wraps the official MongoDB driver and simplifies common data access patterns, modelling, querying, persistence, and indexing with a clean, strongly-typed API.

---

## Installation

=== ".NET CLI"

    ```bash
    dotnet add package meerkat
    ```

=== "Package Manager Console"

    ```powershell
    Install-Package meerkat
    ```

=== ".csproj PackageReference"

    ```xml
    <PackageReference Include="meerkat" Version="3.0.0" />
    ```

---

## Quick Setup

Call `Connect` once at application startup, before executing any Meerkat operations:

```csharp
using meerkat;

Meerkat.Connect("mongodb://user:password@host:port/database-name");
```

### Verifying Indexes at Startup

To create and verify attribute-declared indexes at startup, call `EnsureIndexesAsync` right after `Connect`:

```csharp
await Meerkat.EnsureIndexesAsync(typeof(MyApp).Assembly);
```

This scans the assembly for concrete `Schema<TId>` types, creates any indexes declared via index attributes idempotently, and throws `IndexVerificationException` if an expected index is missing.

---

## Quick Example

Define a model inheriting from `Schema<TId>`:

```csharp
using meerkat;
using meerkat.Attributes;
using MongoDB.Bson;

[Collection(TrackTimestamps = true)]
public class Student : Schema<ObjectId>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }

    public Student()
    {
        Id = ObjectId.GenerateNewId();
    }
}
```

Save and query documents:

```csharp
// Persist or update
var student = new Student { FirstName = "Ada", LastName = "Lovelace", Age = 36 };
await student.SaveAsync();

// Query by ID
var result = await Meerkat.FindByIdAsync<Student, ObjectId>(student.Id);

// Query with LINQ
var students = await Meerkat.Query<Student, ObjectId>()
    .Where(s => s.LastName == "Lovelace")
    .ToListAsync();
```

---

## Next Steps

Explore the detailed documentation topics:

- [**Defining Models**](models.md) — Base schema, identifiers, collection configuration, timestamps, and soft delete.
- [**Querying**](querying.md) — Find by ID, predicates, LINQ queries, counts, and existence checks.
- [**Persistence & Transactions**](persistence.md) — Document upserts, bulk persistence, deletions, and ambient multi-document transactions.
- [**Atomic Updates**](updates.md) — Fluent update builder and atomic numeric increment/decrement helpers.
- [**Indexing**](indexing.md) — Single-field, unique, compound, TTL, geospatial indexes, and startup verification.
- [**Transforms & Hooks**](transforms-hooks.md) — String casing transforms and `PreSave`/`PostSave` lifecycle hooks.
- [**Architecture**](architecture.md) — Deep dive into library design, runtime model, and request flows.
- [**Breaking Changes**](breaking-changes.md) — Upgrade notes and breaking change history across major versions.
