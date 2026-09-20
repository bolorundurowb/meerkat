# Atomic Updates

While `Save` and `SaveAsync` replace the entire document, Meerkat provides fluent update builders and dedicated atomic numeric helpers for field-level modifications. These issue MongoDB update operators (`$set`, `$unset`, `$push`, `$pull`, `$addToSet`, `$inc`) directly without replacing the entire document.

---

## Behaviour & Bypassed Features

When executing partial updates or atomic increment/decrement operations:

- **Timestamps**: If `TrackTimestamps = true` is enabled on the model's `[Collection]` attribute, `UpdatedAt` is automatically set to UTC now as part of the update.
- **Skipped Operations**: Field-level updates **bypass** `CreatedAt`, `[Lowercase]`/`[Uppercase]` case transformations, and `PreSave()` / `PostSave()` lifecycle hooks because the complete entity is not materialised or fully rewritten.

---

## Fluent Partial Updates

Use the fluent update builder to compose multiple update operators in a single atomic database operation:

```csharp
// Update by ID
await Meerkat.Update<Student, ObjectId>(id)
    .Set(x => x.LastName, "Lovelace")
    .Unset(x => x.Nickname)
    .Push(x => x.Tags, "honour")
    .Pull(x => x.Tags, "draft")
    .AddToSet(x => x.Tags, "alumni")
    .Inc(x => x.Age, 1)
    .ExecuteAsync();

// Update first document matching predicate
await Meerkat.UpdateOne<Student, ObjectId>(x => x.FirstName == "Ada")
    .Set(x => x.LastName, "Lovelace")
    .ExecuteAsync();

// Update all documents matching predicate
await Meerkat.UpdateMany<Student, ObjectId>(x => x.Age < 18)
    .Set(x => x.Status, "minor")
    .ExecuteAsync();

// Update using raw MongoDB filter definition
var filter = Builders<Student>.Filter.Eq(x => x.Status, "pending");
await Meerkat.UpdateByFilter<Student, ObjectId>(filter)
    .Set(x => x.Status, "active")
    .ExecuteAsync();
```

### Execution Methods

- **`ExecuteAsync()` / `Execute()`**: Executes the update and returns the standard MongoDB `UpdateResult`.
- **`ExecuteAndGetUpdatedAsync()` / `ExecuteAndGetUpdated()`**: Executes the update and returns the modified document after changes have been applied (cannot be used with `UpdateMany`).

```csharp
var updatedStudent = await Meerkat.Update<Student, ObjectId>(id)
    .Set(x => x.LastName, "Lovelace")
    .ExecuteAndGetUpdatedAsync();
```

---

## Atomic Increment & Decrement

Meerkat provides dedicated atomic helpers for incrementing and decrementing numeric fields (`int`, `long`, `double`, `decimal`, etc.). If the amount is omitted, it defaults to `1`.

### Increment / Decrement by ID

```csharp
// Increment
await Meerkat.IncrementByIdAsync<Student, ObjectId, int>(id, x => x.Age, 5);
Meerkat.IncrementById<Student, ObjectId, int>(id, x => x.Age); // default +1

// Decrement
await Meerkat.DecrementByIdAsync<Student, ObjectId, int>(id, x => x.Age, 3);
Meerkat.DecrementById<Student, ObjectId, int>(id, x => x.Age); // default -1
```

### Increment / Decrement by Filter

```csharp
var filter = Builders<Student>.Filter.Eq(x => x.FirstName, "Ada");

await Meerkat.IncrementByFilterAsync<Student, ObjectId, int>(filter, x => x.Age, 5);
await Meerkat.DecrementByFilterAsync<Student, ObjectId, int>(filter, x => x.Age, 3);
```

### Increment / Decrement One by Predicate

```csharp
await Meerkat.IncrementOneAsync<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age, 5);
await Meerkat.DecrementOneAsync<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age, 3);
```

### Increment / Decrement Many by Predicate

```csharp
await Meerkat.IncrementManyAsync<Student, ObjectId, int>(x => x.Age < 18, x => x.Age, 1);
await Meerkat.DecrementManyAsync<Student, ObjectId, int>(x => x.Age >= 65, x => x.Age, 1);
```

### Returning the Updated Document

Use `*AndGetUpdated` variants to receive the document after the numeric update:

```csharp
var updated = await Meerkat.IncrementByIdAndGetUpdatedAsync<Student, ObjectId, int>(id, x => x.Age, 5);
var updated = await Meerkat.DecrementOneAndGetUpdatedAsync<Student, ObjectId, int>(x => x.FirstName == "Ada", x => x.Age, 3);
```
