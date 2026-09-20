# Transforms & Hooks

Meerkat provides built-in attributes for automatic string transformations and lifecycle methods for custom business logic before and after document persistence.

---

## Data Transformations

Apply `[Lowercase]` or `[Uppercase]` attributes to model string properties to transform their values automatically before each document save.

```csharp
using meerkat;
using meerkat.Attributes;

public class User : Schema<Guid>
{
    [Lowercase]
    public string Email { get; set; } = string.Empty;

    [Uppercase]
    public string CountryCode { get; set; } = string.Empty;
}
```

### Constraints & Validation

- Both attributes are strictly restricted to properties of type `string`.
- Applying `[Lowercase]` or `[Uppercase]` to non-string properties throws an `InvalidAttributeException` at runtime when inspected.
- Null string values are safely ignored and remain `null`.

---

## Lifecycle Hooks

Override the `PreSave` and `PostSave` virtual methods on any model inheriting from `Schema<TId>` to run custom logic during document persistence:

```csharp
using meerkat;
using MongoDB.Bson;

public class Student : Schema<ObjectId>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public override void PreSave()
    {
        // Executes after timestamp generation and case transformations,
        // right before the document is written to MongoDB.
        FirstName = FirstName?.Trim() ?? string.Empty;
        LastName = LastName?.Trim() ?? string.Empty;
    }

    public override void PostSave()
    {
        // Executes immediately after the MongoDB ReplaceOne write completes.
    }
}
```

### Execution Order during Save

When calling `Save()` or `SaveAsync()` (and in bulk `SaveAll()` / `SaveAllAsync()`):

1. **Timestamps**: Sets `CreatedAt` (if first save) and `UpdatedAt` (if `TrackTimestamps = true`).
2. **Transforms**: Evaluates `[Lowercase]` and `[Uppercase]` attributes on string properties.
3. **PreSave**: Calls the model's `PreSave()` method.
4. **Database Write**: Executes `ReplaceOne` with upsert enabled on MongoDB.
5. **PostSave**: Calls the model's `PostSave()` method.

!!! warning "Write-Path Difference"
    Partial updates (via `Update<TSchema, TId>()`, `UpdateOne`, `UpdateMany`, `UpdateByFilter`, or atomic increment/decrement helpers) **do not** invoke `PreSave()`, `PostSave()`, or data transformations because documents are modified directly at the field level within MongoDB.
