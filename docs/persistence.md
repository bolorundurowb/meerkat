# Persistence & Transactions

Meerkat provides rich APIs for single-document persistence, bulk persistence, physical and soft removal, and ambient multi-document transactions.

---

## Single Document Persistence

`Save` and `SaveAsync` perform an upsert on the underlying collection: inserting the document if it does not exist, or replacing the entire document if a match on `Id` is found.

```csharp
var student = new Student { FirstName = "Ada", LastName = "Lovelace" };

// Asynchronous save
await student.SaveAsync();

// Synchronous save
student.Save();
```

When saving:
1. `CreatedAt` (if not already set) and `UpdatedAt` are populated if `TrackTimestamps = true`.
2. Property transformations (`[Lowercase]`, `[Uppercase]`) are executed.
3. `PreSave()` lifecycle hook is invoked.
4. The document is replaced / inserted in MongoDB using `ReplaceOne`.
5. `PostSave()` lifecycle hook is invoked.

---

## Bulk Persistence

To persist multiple entities in a single batched operation, use the `SaveAll` / `SaveAllAsync` extension methods from the `meerkat.Collections` namespace:

```csharp
using meerkat.Collections;

var students = new[]
{
    new Student { FirstName = "Ada", LastName = "Lovelace" },
    new Student { FirstName = "Charles", LastName = "Babbage" }
};

// Asynchronous bulk save
await students.SaveAllAsync<Student, ObjectId>();

// Synchronous bulk save
students.SaveAll<Student, ObjectId>();
```

---

## Removal

### Remove by ID

```csharp
// Async
await Meerkat.RemoveByIdAsync<Student, ObjectId>(id);

// Sync
Meerkat.RemoveById<Student, ObjectId>(id);
```

### Remove First Match

```csharp
// Async
await Meerkat.RemoveOneAsync<Student, ObjectId>(x => x.FirstName == "Ada");

// Sync
Meerkat.RemoveOne<Student, ObjectId>(x => x.FirstName == "Ada");
```

### Remove All Matches

```csharp
// Async
await Meerkat.RemoveAsync<Student, ObjectId>(x => x.LastName == "Lovelace");

// Sync
Meerkat.Remove<Student, ObjectId>(x => x.LastName == "Lovelace");
```

### Model Instance Removal

Models inheriting from `Schema<TId>` can remove themselves directly:

```csharp
await student.DeleteAsync();
```

---

## Soft Delete

Opt into soft deletion per collection by setting `[Collection(SoftDelete = true)]`.

Meerkat stores `DeletedAt` (`DateTimeOffset?`) as a UTC BSON date. The `IsDeleted` property is computed dynamically (`DeletedAt != null`) and is not persisted.

```csharp
[Collection(SoftDelete = true, TrackTimestamps = true)]
public class Student : Schema<ObjectId>
{
    public string FirstName { get; set; } = string.Empty;
}
```

### Behaviour When Enabled

- **Queries & Counts**: `Query`, `Find*`, `FindById*`, `Count*`, `Exists*`, increment/decrement, and fluent `Update*` automatically exclude soft-deleted documents.
- **Deletions**: `Remove*`, `RemoveById*`, `RemoveOne*`, and `student.DeleteAsync()` set `DeletedAt` (and `UpdatedAt` if timestamps are tracked) instead of issuing a physical MongoDB delete command.
- **Index**: An index on `DeletedAt` (`deleted_at_idx`) is created automatically.

### Including Soft-Deleted Documents

Pass `includeDeleted: true` to bypass the soft delete filter:

```csharp
var active = await Meerkat.FindByIdAsync<Student, ObjectId>(id);
var includingDeleted = await Meerkat.FindByIdAsync<Student, ObjectId>(id, includeDeleted: true);

var all = await Meerkat.Query<Student, ObjectId>(includeDeleted: true).ToListAsync();
long totalCount = await Meerkat.CountAsync<Student, ObjectId>(includeDeleted: true);
```

### Restoring Soft-Deleted Documents

```csharp
// Restore by ID
await Meerkat.RestoreByIdAsync<Student, ObjectId>(id);

// Restore by predicate
await Meerkat.RestoreAsync<Student, ObjectId>(x => x.LastName == "Lovelace");

// Instance helper
await student.RestoreAsync();
```

### Hard (Permanent) Removal

To bypass soft delete and permanently delete documents from MongoDB:

```csharp
// Hard remove by ID
await Meerkat.HardRemoveByIdAsync<Student, ObjectId>(id);

// Hard remove by predicate
await Meerkat.HardRemoveAsync<Student, ObjectId>(x => x.LastName == "Lovelace");
```

!!! note
    Soft delete does not cascade to related documents (Meerkat does not maintain an association graph). Standard unique indexes still apply to soft-deleted records. If uniqueness is only required among active records, define a partial unique index.

---

## Transactions

Meerkat exposes ambient multi-document transactions. Writes and reads executed within the callback automatically join the transaction without requiring explicit session parameters on `Save`, `Remove*`, `Update*`, `Increment*`, `Count*`, or `Query`/`Find*` APIs.

!!! warning "Replica Set Requirement"
    MongoDB multi-document transactions require a replica set or sharded cluster. Standalone MongoDB instances do not support transactions.

```csharp
await Meerkat.WithTransactionAsync(async _ =>
{
    var order = new Order { Number = "1001" };
    await order.SaveAsync();

    var line = new OrderLine { OrderId = order.Id, Qty = 2 };
    await line.SaveAsync();
});
```

### Transaction Semantics

- **Commit**: If the callback completes successfully, the transaction commits automatically.
- **Rollback**: If the callback throws an exception, the transaction rolls back all modifications.
- **Read-Your-Writes**: Reads inside the callback see uncommitted writes made earlier in the same transaction.
- **Nesting**: Nested `WithTransaction` / `WithTransactionAsync` calls throw `InvalidOperationException`.

### Overloads and Return Values

```csharp
// Synchronous transaction
Meerkat.WithTransaction(() =>
{
    var order = new Order { Number = "1002" };
    order.Save();
});

// Returning a value from transaction
var orderId = await Meerkat.WithTransactionAsync(async _ =>
{
    var order = new Order { Number = "1003" };
    await order.SaveAsync();
    return order.Id;
});
```
