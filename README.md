# Meerkat

[![Build, Test & Coverage](https://github.com/bolorundurowb/meerkat/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/bolorundurowb/meerkat/actions/workflows/build-and-test.yml)
[![codecov](https://codecov.io/gh/bolorundurowb/meerkat/graph/badge.svg?token=E35WQFJ7IM)](https://codecov.io/gh/bolorundurowb/meerkat)
![NuGet Version](https://img.shields.io/nuget/v/meerkat)
[![Docs](https://img.shields.io/badge/docs-gh--pages-blue.svg)](https://bolorundurowb.github.io/meerkat/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

**Meerkat** is an ODM (Object Document Mapper) for MongoDB in .NET. It wraps the official MongoDB driver and simplifies common data access patterns, modelling, querying, persistence, and indexing with a clean, strongly-typed API.

---

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
<PackageReference Include="meerkat" Version="3.0.0" />
```

---

## Quickstart

Initialize connection at startup:

```csharp
using meerkat;

Meerkat.Connect("mongodb://user:password@host:port/database-name");
```

Define a model:

```csharp
using meerkat;
using meerkat.Attributes;
using MongoDB.Bson;

[Collection(TrackTimestamps = true)]
public class Student : Schema<ObjectId>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public Student()
    {
        Id = ObjectId.GenerateNewId();
    }
}
```

Save and query:

```csharp
// Save (upsert)
var student = new Student { FirstName = "Ada", LastName = "Lovelace" };
await student.SaveAsync();

// Find by ID
var found = await Meerkat.FindByIdAsync<Student, ObjectId>(student.Id);
```

---

## Documentation

Comprehensive guides, API examples, and architecture documentation are available at **[https://bolorundurowb.github.io/meerkat/](https://bolorundurowb.github.io/meerkat/)**:

- [**Defining Models**](https://bolorundurowb.github.io/meerkat/models/) — Schema base class, ID types, collection names, timestamps, and soft delete.
- [**Querying**](https://bolorundurowb.github.io/meerkat/querying/) — Predicate queries, LINQ queries, counting, and existence checks.
- [**Persistence & Transactions**](https://bolorundurowb.github.io/meerkat/persistence/) — Save, SaveAll, removals, soft delete, and ambient multi-document transactions.
- [**Atomic Updates**](https://bolorundurowb.github.io/meerkat/updates/) — Fluent update builders and atomic numeric increment/decrement helpers.
- [**Indexing**](https://bolorundurowb.github.io/meerkat/indexing/) — Single-field, unique, compound, TTL, geospatial indexes, and startup index verification.
- [**Transforms & Hooks**](https://bolorundurowb.github.io/meerkat/transforms-hooks/) — String casing transforms and lifecycle hooks.
- [**Architecture**](https://bolorundurowb.github.io/meerkat/architecture/) — Design goals, runtime model, and request flows.
- [**Breaking Changes**](https://bolorundurowb.github.io/meerkat/breaking-changes/) — Release notes and upgrade guides.

### Running Docs Locally

To preview documentation locally:

```bash
pip install -r requirements-docs.txt
mkdocs serve
```

---

## Contributing

Issues and pull requests are welcome at [github.com/bolorundurowb/meerkat](https://github.com/bolorundurowb/meerkat).

---

## License

Meerkat is open source software licensed under the [MIT License](LICENSE).
