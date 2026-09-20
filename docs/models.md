# Defining Models

All models in Meerkat must inherit from the abstract base class `Schema<TId>`, where `TId` is the type of the document's unique identifier. The `Id` property is mapped to the MongoDB `_id` field.

---

## Basic Model Definition

```csharp
using meerkat;
using MongoDB.Bson;

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

### Identifier Types

The generic type parameter `TId` requires `IEquatable<TId>`. Common ID types supported in Meerkat include:

- `MongoDB.Bson.ObjectId`
- `System.Guid`
- `string`
- `int` / `long`
- Any custom value type implementing `IEquatable<TId>`

---

## Collection Configuration

Apply the `[Collection]` attribute to customise collection names, timestamp tracking, and soft deletion:

```csharp
using meerkat;
using meerkat.Attributes;
using MongoDB.Bson;

[Collection(Name = "people", TrackTimestamps = true, SoftDelete = true)]
public class Student : Schema<ObjectId>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
```

### Attribute Options

| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Name` | `string` | `null` | Overrides the default collection name. If not specified, Meerkat generates a pluralised, lowercased name from the class name (e.g. `Student` → `students`). |
| `TrackTimestamps` | `bool` | `false` | When `true`, automatically sets `CreatedAt` on first save and updates `UpdatedAt` on every save or partial update. |
| `SoftDelete` | `bool` | `false` | When `true`, enables soft delete. `Remove*` sets `DeletedAt` instead of physically deleting documents, and queries automatically exclude soft-deleted records. |

---

## Timestamps

When `TrackTimestamps = true` is set on the `[Collection]` attribute:

- `CreatedAt` (`DateTime?`): Populated with UTC timestamp when the document is first saved via `Save` or `SaveAll`.
- `UpdatedAt` (`DateTime?`): Updated to the current UTC timestamp on every `Save`, `SaveAll`, and fluent partial `Update` execution.

```csharp
[Collection(TrackTimestamps = true)]
public class Article : Schema<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
```

---

## Soft Delete Overview

When `SoftDelete = true` is set:

- `DeletedAt` (`DateTimeOffset?`): Persisted as a UTC BSON date when the document is soft-deleted.
- `IsDeleted` (`bool`): A computed property returning `DeletedAt != null` (ignored during serialisation).
- An index on `DeletedAt` (`deleted_at_idx`) is created automatically.

For querying, restoring, and permanently deleting soft-deleted entities, see [Persistence & Transactions](persistence.md#soft-delete).
