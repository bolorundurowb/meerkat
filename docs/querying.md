# Querying

All static query methods on `Meerkat` require two generic type parameters: the schema type (`TSchema`) and the identifier type (`TId`).

---

## Find by ID

Retrieve a single document by its identifier:

```csharp
// Async
var student = await Meerkat.FindByIdAsync<Student, ObjectId>(id);

// Sync
var student = Meerkat.FindById<Student, ObjectId>(id);
```

When soft delete is enabled, soft-deleted documents are excluded by default. Pass `includeDeleted: true` to include them:

```csharp
var student = await Meerkat.FindByIdAsync<Student, ObjectId>(id, includeDeleted: true);
```

---

## Find One by Predicate

Find the first document matching a predicate expression:

```csharp
// Async
var student = await Meerkat.FindOneAsync<Student, ObjectId>(x => x.FirstName == "Ada");

// Sync
var student = Meerkat.FindOne<Student, ObjectId>(x => x.LastName == "Lovelace");
```

Omitting the predicate returns the first document found in the collection:

```csharp
var firstStudent = await Meerkat.FindOneAsync<Student, ObjectId>();
```

---

## Find Many by Predicate

Retrieve all documents matching a predicate expression as a list:

```csharp
// Async
var students = await Meerkat.FindAsync<Student, ObjectId>(x => x.LastName == "Lovelace");

// Sync
var students = Meerkat.Find<Student, ObjectId>(x => x.LastName == "Lovelace");
```

Omitting the predicate returns all active documents in the collection:

```csharp
var allStudents = await Meerkat.FindAsync<Student, ObjectId>();
```

---

## LINQ Queries

For complex filtering, projections, sorting, and pagination, use `Meerkat.Query<TSchema, TId>()` to obtain a MongoDB LINQ `IQueryable`:

```csharp
using MongoDB.Driver.Linq;

var results = await Meerkat.Query<Student, ObjectId>()
    .Where(x => x.FirstName.StartsWith("A"))
    .OrderBy(x => x.LastName)
    .Skip(10)
    .Take(20)
    .ToListAsync();
```

To include soft-deleted documents in a LINQ query:

```csharp
var allResults = await Meerkat.Query<Student, ObjectId>(includeDeleted: true)
    .Where(x => x.Age >= 18)
    .ToListAsync();
```

---

## Counting

Count total documents or documents matching a filter predicate:

```csharp
// Total count
long total = await Meerkat.CountAsync<Student, ObjectId>();

// Conditional count
long adultsCount = await Meerkat.CountAsync<Student, ObjectId>(x => x.Age >= 18);

// Including soft-deleted records
long allCount = await Meerkat.CountAsync<Student, ObjectId>(includeDeleted: true);

// Sync variants
long totalSync = Meerkat.Count<Student, ObjectId>();
long matchingSync = Meerkat.Count<Student, ObjectId>(x => x.FirstName.StartsWith("A"));
```

---

## Existence Checks

Check whether any documents exist or if at least one document matches a predicate:

```csharp
// Any documents exist
bool hasAny = await Meerkat.ExistsAsync<Student, ObjectId>();

// Document matching predicate exists
bool exists = await Meerkat.ExistsAsync<Student, ObjectId>(x => x.FirstName == "Ada");

// Sync variants
bool hasAnySync = Meerkat.Exists<Student, ObjectId>();
bool existsSync = Meerkat.Exists<Student, ObjectId>(x => x.FirstName == "Ada");
```
