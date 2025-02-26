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

## 📄 Index Attributes  

This section provides an overview of the custom attributes used to define MongoDB indexes in your model classes. These attributes are applied to fields or properties and are used to generate appropriate indexes in MongoDB. All model classes must inherit from the abstract class `Schema<TKey>`.

---

### 🗂️ Index Attributes Overview 

#### 🎯 **`SingleFieldIndexAttribute`** 
- **Purpose**: Defines a single-field index on a specific field or property.
- **Usage**: Apply this attribute to a field or property to create an index on that single field.
- **Optional Properties**:
    - **`Name`**: Specifies the name of the index. If not provided, MongoDB generates a default name.
    - **`Sparse`**: A boolean value indicating whether the index should be sparse. Default is `false`.
    - **`IndexOrder`**: Specifies the order of the index. Options are `Ascending`, `Descending`, or `Hashed`.

##### Example:
```csharp
public class User : Schema<Guid>
{
    [SingleFieldIndex(Name = "username_index", Sparse = true, IndexOrder = IndexOrder.Ascending)]
    public string Username { get; set; }
}
```
- **Explanation**: This creates a single-field index on the `Username` property with an ascending order. The index is sparse, meaning it will only include documents where the `Username` field exists.

##### What is a Sparse Index? 🤔
A sparse index only includes documents that have the indexed field. If a document does not contain the indexed field, it is excluded from the index. This can save space and improve performance for fields that are not present in all documents.

##### What is a Hashed Index? 🔍
A hashed index in MongoDB uses a hash function to compute the value of the indexed field. This is particularly useful for sharding and equality queries but does not support range queries.

---

#### 🔑 **`UniqueIndexAttribute`** 
- **Purpose**: Defines a unique index on a specific field or property.
- **Usage**: Apply this attribute to enforce uniqueness on a field or property.
- **Optional Properties**:
    - **`Name`**: Specifies the name of the index. If not provided, MongoDB generates a default name.
    - **`Sparse`**: A boolean value indicating whether the index should be sparse. Default is `false`.

##### Example:
```csharp
public class User : Schema<Guid>
{
    [UniqueIndex(Name = "email_unique_index", Sparse = true)]
    public string Email { get; set; }
}
```
- **Explanation**: This creates a unique index on the `Email` property. The index is sparse, meaning it will only include documents where the `Email` field exists.

---

#### 🧩 **`CompoundIndexAttribute`** 
- **Purpose**: Defines a compound index on multiple fields or properties.
- **Usage**: Apply this attribute to multiple fields or properties to create a compound index.
- **Optional Properties**:
    - **`Name`**: Specifies the name of the index. If two or more fields have the same `Name`, they are grouped into a single compound index. Unnamed indexes are grouped into one compound index.
    - **`IndexOrder`**: Specifies the order of the index. Options are `Ascending`, `Descending`, or `Hashed`.

##### Example:
```csharp
public class Order : Schema<Guid>
{
    [CompoundIndex(Name = "order_index", IndexOrder = IndexOrder.Ascending)]
    public DateTime OrderDate { get; set; }

    [CompoundIndex(Name = "order_index", IndexOrder = IndexOrder.Descending)]
    public decimal TotalAmount { get; set; }
}
```
- **Explanation**: This creates a compound index on the `OrderDate` and `TotalAmount` properties. The `OrderDate` is indexed in ascending order, while the `TotalAmount` is indexed in descending order.

##### Note on Compound Indexes 📌
If multiple fields have the same `Name` in the `CompoundIndexAttribute`, they are grouped into a single compound index. Unnamed indexes are grouped into one compound index automatically.

---

#### 🌍 **`GeospatialIndexAttribute`** 
- **Purpose**: Defines a geospatial index on a field or property.
- **Usage**: Apply this attribute to fields or properties that store geospatial data (e.g., coordinates).
- **Optional Properties**:
    - **`Name`**: Specifies the name of the index. If not provided, MongoDB generates a default name.
    - **`IndexType`**: Specifies the type of geospatial index. Options are `TwoD` (default) or `TwoDSphere`.

##### Example:
```csharp
public class Location : Schema<Guid>
{
    [GeospatialIndex(Name = "location_geo_index", IndexType = IndexType.TwoDSphere)]
    public double[] Coordinates { get; set; }
}
```
- **Explanation**: This creates a geospatial index on the `Coordinates` property, using the `TwoDSphere` index type, which is useful for querying geospatial data on a spherical surface.
  What is the Difference Between TwoD and TwoDSphere? 🌐

  - **`TwoD`**: This index type is used for flat, 2D geospatial data. It is suitable for simple 2D coordinate systems.

  - **`TwoDSphere`**: This index type is used for geospatial data on a spherical surface (e.g., Earth). It supports more complex queries involving distances, intersections, and other spherical calculations.

---

### Summary of Index Types 📊

| Attribute                | Purpose                          | Optional Properties            |
|--------------------------|----------------------------------|--------------------------------|
| `SingleFieldIndex`       | Single-field index              | `Name`, `Sparse`, `IndexOrder` |
| `UniqueIndex`            | Unique index                    | `Name`, `Sparse`               |
| `CompoundIndex`           | Compound index on multiple fields | `Name`, `IndexOrder`           |
| `GeospatialIndex`         | Geospatial index                | `Name`, `IndexType`             |

---

### Example Model Class 🧑‍💻

Here’s an example of a model class using all the attributes:

```csharp
public class Product : Schema<Guid>
{
    [SingleFieldIndex(Name = "name_index", IndexOrder = IndexOrder.Ascending)]
    public string Name { get; set; }

    [UniqueIndex(Name = "sku_unique_index", Sparse = true)]
    public string SKU { get; set; }

    [CompoundIndex(Name = "price_category_index", IndexOrder = IndexOrder.Descending)]
    public decimal Price { get; set; }

    [CompoundIndex(Name = "price_category_index", IndexOrder = IndexOrder.Ascending)]
    public string Category { get; set; }

    [GeospatialIndex(Name = "location_geo_index")]
    public double[] Location { get; set; }
}
```
- **Explanation**: This example demonstrates the use of multiple index types within a single model class. It includes a single-field index, a unique index, a compound index, and a geospatial index.

---

Enjoy using **Meerkat**! 🎉 If you have any questions or feedback, feel free to reach out or contribute to the project. 🚀