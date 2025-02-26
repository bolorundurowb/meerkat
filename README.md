# 🐾 Meerkat

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE) ![NuGet Version](https://img.shields.io/nuget/v/meerkat) [![Build Status](https://app.travis-ci.com/bolorundurowb/meerkat.svg?branch=master)](https://app.travis-ci.com/bolorundurowb/meerkat)

**Meerkat** is an ODM (Object Document Mapper) library designed to replicate the functionality of NodeJS's [Mongoose](https://www.npmjs.com/package/mongoose) in the .NET ecosystem. 🚀 For those unfamiliar, Mongoose is a JavaScript ODM wrapper library that simplifies data access when working with MongoDB. Similarly, **Meerkat** wraps around the official MongoDB client library for .NET, simplifying common data access patterns.

The name **Meerkat** is a playful homage to Mongoose, as a meerkat is a type of mongoose. 😄 If you find this library cool or useful, don't forget to give it a ⭐️ star!

---

## 🚨 Breaking Changes

With the release of **version 2.0.0**, the underlying MongoDB driver was upgraded to **3.2.0**. The library also transitions the base `Schema` class to  using strongly typed Ids

---

## 🤝 Contributing

There’s still a lot to be done! Feel free to:
- Open new issues to suggest features or report bugs 🐛
- Submit PRs with updates or improvements 🛠️

---

## 📦 Installation

### Manual Installation (for the hardcore devs 💪)
Add the following to your `.csproj` file:

```xml
<PackageReference Include="meerkat" Version="2.0.0"/>
```

### Visual Studio Package Manager Console
Run the following command:

```cmd
Install-Package meerkat
```

### .NET CLI
Run the following in your terminal:

```bash
dotnet add package meerkat
```

---

## 🛠️ Setup

Before using any of Meerkat's functions, you need to initialize it. This only needs to be done once. 🏁

```csharp
using meerkat;
...
Meerkat.Connect("<any valid full MongoDB connection string>"); // e.g., mongodb://user:password@server-address:port/database-name?other-options
```

---

## 🚀 Usage

Ensure you’ve declared the necessary namespace at the top of your class file:

```csharp
using meerkat;
```

**Note:** All async methods support `CancellationToken` for canceling operations. ⏳

---

### 🧩 Modelling

All models must inherit from the abstract `Schema` class. The `Schema` class has a an `Id` property whose type is determined by the `TKey` generic argument. In the example below, the `Id` property is of type `ObjectId`.

```csharp
class Student : Schema<ObjectId>
{  
  public string FirstName { get; set; }
  
  public string LastName { get; set; }
  
  public Student()
  {
    // Example: Generate an ObjectID (you'd likely use a methode better suited to your Id type)
    Id = ObjectId.GenerateNewId();
  }
}
```

To specify a custom collection name or enable timestamp tracking:

```csharp
[Collection(Name = "Persons", TrackTimestamps = true)]
public class Student : Schema
{
  ...
}
```

---

### 💾 Persistence

Meerkat simplifies CRUD operations by combining **create** and **update** into a single API. If an entity doesn’t exist, it’s inserted; if it does, it’s updated. 🔄

```csharp
var student = new Student
{
  FirstName = "Olubakinde",
  LastName = "Chukumerije"
};

await student.SaveAsync(); // or student.Save(); for synchronous calls
```

It’s that simple! 🎉

---

### 🔍 Querying

#### Find by ID
```csharp
var student = await Meerkat.FindByIdAsync<Student>(1234); // or Meerkat.FindById<Student>(1234); for sync calls
```

#### Find by Predicate
```csharp
var student = await Meerkat.FindOneAsync<Student>(x => x.FirstName == "John"); // or Meerkat.FindOne(x => x.LastName == "Jane");
```

#### Complex Queries
For complex queries, you can access the underlying `IQueryable`:

```csharp
var queryable = Meerkat.Query<Student>();

var students = await queryable
  .Where(x => x.FirstName == "Olubakinde")
  .ToListAsync();
```

---

### 🗑️ Removal

#### Remove by ID
```csharp
await Meerkat.RemoveByIdAsync<Student>(1234); // or Meerkat.RemoveById<Student>(1234); for sync calls
```

#### Remove by Predicate
```csharp
await Meerkat.RemoveOneAsync<Student>(x => x.FirstName == "John"); // or Meerkat.RemoveOne(x => x.LastName == "Jane");
```

#### Remove All Matching Entities
```csharp
await Meerkat.RemoveAsync<Student>(x => x.FirstName == "John"); // or Meerkat.Remove(x => x.LastName == "Jane");
```

---

### ✅ Existence Checks

#### Check if Any Entities Exist
```csharp
var exists = await Meerkat.ExistsAsync<Student>(); // or Meerkat.Exists<Student>(); for sync calls
```

#### Check if Entities Match a Predicate
```csharp
var exists = await Meerkat.ExistsAsync<Student>(x => x.FirstName.StartsWith("Ja")); // or Meerkat.Exists<Student>(x => x.FirstName.StartsWith("Ja"));
```

---

### 🔢 Counting

#### Count All Entities
```csharp
var count = await Meerkat.CountAsync<Student>(); // or Meerkat.Count<Student>(); for sync calls
```

#### Count Entities Matching a Predicate
```csharp
var count = await Meerkat.CountAsync<Student>(x => x.FirstName.StartsWith("Ja")); // or Meerkat.Count<Student>(x => x.FirstName.StartsWith("Ja"));
```

---

### 📚 Collections

Meerkat allows for bulk upsert operations on collections of entities, both synchronously and asynchronously. 📦

```csharp
var peter = new Student();
var paul = new Student();
var students = new [] { peter, paul };
await students.SaveAllAsync(); // or students.SaveAll(); for sync calls
```

### 🔍 Indexing

Indexes in MongoDB improve query performance by enabling efficient lookups and enforcing constraints. The following attributes define different types of indexes that can be applied to fields or properties in a MongoDB document model.

## MongoDB Index Attributes

Indexes in MongoDB optimize query performance by allowing efficient lookups and enforcing constraints. The following attributes define different types of indexes that can be applied to fields or properties in a MongoDB document model.

All model classes must inherit from the abstract class `Schema<TKey>`, where `TKey` represents the type of the primary key.

### **1. Compound Index (`CompoundIndexAttribute`)**
A **compound index** is an index on multiple fields, allowing queries to efficiently filter or sort using those fields. The order of fields in a compound index affects query optimization.

#### **Usage Example**
```csharp
public class Product : Schema<Guid>
{
    [CompoundIndex]
    public string Category { get; set; }

    [CompoundIndex]
    public string Brand { get; set; }
}
```  
This creates a compound index on `Category` and `Brand`, making queries that filter or sort by both fields more efficient.

**Example Query Optimized by This Index:**
```csharp
var products = collection.Find(Builders<Product>.Filter.Eq(p => p.Category, "Electronics"))
                         .SortBy(p => p.Brand)
                         .ToList();
```

---

### **2. Geospatial Index (`GeospatialIndexAttribute`)**
A **geospatial index** enables efficient location-based queries, such as finding points within a specific radius or nearest neighbors. MongoDB supports `2dsphere` and `2d` indexes for geospatial data.

#### **Usage Example**
```csharp
public class Location : Schema<string>
{
    [GeospatialIndex]
    public double[] Coordinates { get; set; } = new double[2]; // [Longitude, Latitude]
}
```  
This ensures that MongoDB optimizes spatial queries using a `2dsphere` index.

**Example Query for Finding Nearby Locations:**
```csharp
var filter = Builders<Location>.Filter.NearSphere(
    l => l.Coordinates, 
    longitude: 36.8219, 
    latitude: -1.2921, 
    maxDistance: 5000 // 5km radius
);
var nearbyLocations = collection.Find(filter).ToList();
```

---

### **3. Single-Field Index (`SingleFieldIndexAttribute`)**
A **single-field index** optimizes queries that filter or sort based on a single field.

#### **Usage Example**
```csharp
public class Customer : Schema<ObjectId>
{
    [SingleFieldIndex]
    public string Email { get; set; }
}
```  
This makes queries that filter by `Email` more efficient.

**Example Query Optimized by This Index:**
```csharp
var customer = collection.Find(Builders<Customer>.Filter.Eq(c => c.Email, "user@example.com")).FirstOrDefault();
```

---

### **4. Unique Index (`UniqueIndexAttribute`)**
A **unique index** ensures that a field contains only unique values, preventing duplicates.

#### **Usage Example**
```csharp
public class User : Schema<Guid>
{
    [UniqueIndex]
    public string Username { get; set; }
}
```  
This enforces uniqueness on the `Username` field, ensuring no two users share the same username.

**Example Query Using This Index:**
```csharp
var newUser = new User { Username = "john_doe" };
collection.InsertOne(newUser); // Will throw an error if "john_doe" already exists
```

---

By using these attributes, MongoDB indexes are automatically created and managed, improving query performance, enabling spatial queries, and enforcing data integrity constraints.

---

Enjoy using **Meerkat**! 🎉 If you have any questions or feedback, feel free to reach out or contribute to the project. 🚀