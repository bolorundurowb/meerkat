# 🐾 Meerkat

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE) ![NuGet Version](https://img.shields.io/nuget/v/meerkat) [![Build Status](https://app.travis-ci.com/bolorundurowb/meerkat.svg?branch=master)](https://app.travis-ci.com/bolorundurowb/meerkat)

**Meerkat** is an ODM (Object Document Mapper) library designed to replicate the functionality of NodeJS's [Mongoose](https://www.npmjs.com/package/mongoose) in the .NET ecosystem. 🚀 For those unfamiliar, Mongoose is a JavaScript ODM wrapper library that simplifies data access when working with MongoDB. Similarly, **Meerkat** wraps around the official MongoDB client library for .NET, simplifying common data access patterns. It also adds support for the `DateOnly` and `TimeOnly` types introduced in .NET 6. 📅⏰

The name **Meerkat** is a playful homage to Mongoose, as a meerkat is a type of mongoose. 😄 If you find this library cool or useful, don't forget to give it a ⭐️ star!

---

## 🚨 Breaking Changes

With the release of **version 1.1.0**, the underlying MongoDB driver was upgraded to **3.1.0**. This removes the `IMongoQueryable<T>` interface returned by the `Query<TSchema>()` method, replacing it with the built-in `IQueryable<T>` interface. 🔄

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
<PackageReference Include="meerkat" Version="1.1.0"/>
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

All models must inherit from the abstract `Schema` class. The `Schema` class has a `virtual` `Id` property that can be overridden in your model. By default, the `Id` is of type `ObjectId`.

```csharp
class Student : Schema
{
  public override object Id { get; set; }
  
  public string FirstName { get; set; }
  
  public string LastName { get; set; }
  
  public Student()
  {
    // Example: Generate a random ID (you'd likely use a better method in production)
    Id = (new Random()).Next();
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

---

Enjoy using **Meerkat**! 🎉 If you have any questions or feedback, feel free to reach out or contribute to the project. 🚀