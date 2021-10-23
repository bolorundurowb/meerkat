# meerkat

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE) [![NuGet Badge](https://buildstats.info/nuget/meerkat)](https://www.nuget.org/packages/meerkat)

An ODM (Object Document Mapper) library aiming to replicate as much as is necessary, functionality in NodeJS's [mongoose](https://www.npmjs.com/package/mongoose). For those who may not know, mongoose is a JavaScript ODM wrapper library around the native MongoDB library that simplifies data access by simplifying the API surface. 

This library was named `meerkat` as a homage to `mongoose` because a meerkat is a mongoose. I know, I am hilarious like that. Please be sure to star ⭐️ this project if you think it's cool or useful.

## Contributing

There is a lot still to be done, feel free to open new issues to suggest features or report bugs and feel free to open PRs with updates.

## Installation

If you are hardcore and want to go the manual route. Then add the following to your `csproj` file:

```xml
<PackageReference Include="meerkat" Version="1.0.3"/>
```

If you're using the Visual Studio package manager console, then run the following:

```cmd
Install-Package meerkat
```

If you are making use of the dotnet CLI, then run the following in your terminal:

```bash
dotnet add package meerkat
```

## Setup

Before making use of any of meerkat's functions. Initialization must be done. This only needs to happen once.

```csharp
using meerkat;
...
Meerkat.Connect("<any valid full mongodb connection string>"); // e.g mongodb://user:password@server-address:port/database-name?other-options
```


## Usage

Ensure you have declared the necessary namespace at the head of your class file wherever you want to access meerkat functionality.

### Modelling

All models must inherit from the abstract `Schema` class. The `Schema` class has a `virtual` `Id` property that can be overridden in model. By default the `Id` is an `ObjectId` type.


```csharp
class Student : Schema
{
  public override object Id { get; set; }
  
  public string FirstName { get; set; }
  
  public string LastName { get; set; }
  
  public Student()
  {
    // this is just an example, you'd probably get your entity ids in a saner manner
    Id = (new Random()).Next();
  }
}
```

If you want to specify the collection name to be used or track timestamps for the entity

```csharp
[Collection(Name = "Persons", TrackTimestamps = true)]
public class Student : Schema
{
  ...
}
```

### Persistence

meerkat aims to save developer time by rolling the create and update parts of CRUD into one simple API. If an entity doesn't exist in the database, the entity is inserted into the collection, and if the entity already exists, it is simply updated

```csharp
var student = new Student
{
  FirstName = "Olubakinde",
  LastName = "Chukumerije"
};

await student.SaveAsync(); // or student.Save(); if you like sync calls
```

It's that simple.

### Querying

To find an entity by id

```csharp
var student = await Meerkat.FindByIdAsync<Student>(1234); // or Meerkat.FindById<Student>(1234); if you like sync calls
```

To find an entity by a predictae

```csharp
var student = await Meerkat.FindOneAsync<Student>(x => x.FirstName == "John"); // or Meerkat.FindOne(x => x.LastName == "Jane");
```

If you want to create complex queries and need access to the underlying `IMongoQueryable` then this helps

```csharp
var queryable = Meerkat.Query<Student>();

// do something with your queryable
var students = queryable
  .Where(x => x.FirstName == "Olubakinde")
  .ToListAsync();
```

### Removal

To remove an entity by id

```csharp
await Meerkat.RemoveByIdAsync<Student>(1234); // or Meerkat.RemoveById<Student>(1234); if you like sync calls
```

To remove an entity by a predicate

```csharp
await Meerkat.RemoveOneAsync<Student>(x => x.FirstName == "John"); // or Meerkat.RemoveOne(x => x.LastName == "Jane");
```

To remove all entities that match a predicate

```csharp
await Meerkat.RemoveAsync<Student>(x => x.FirstName == "John"); // or Meerkat.Remove(x => x.LastName == "Jane");
```

### Exists

To check if any entities exist in a collection

```csharp
var exists = await Meerkat.ExistsAsync<Student>(); // or Meerkat.Exists<Student>(); if you like sync calls
```

To check if any entities exits for a predicate

```csharp
var exists = await Meerkat.ExistsAsync<Student>(x => x.FirstName.StartsWith("Ja")); // or Meerkat.Exists<Student>(x => x.FirstName.StartsWith("Ja")); if you like sync calls
```

### Counting

To count all the entities in a collection

```csharp
var count = await Meerkat.CountAsync<Student>(); // or Meerkat.Count<Student>(); if you like sync calls
```

To count all entities exits that match a predicate

```csharp
var count = await Meerkat.CountAsync<Student>(x => x.FirstName.StartsWith("Ja")); // or Meerkat.Count<Student>(x => x.FirstName.StartsWith("Ja")); if you like sync calls
```


## Collections

Meerkat allows for collections of entities to be upserted both synchronously and asynchronously

```csharp
var peter = new Student();
var paul = new Student();
var students = new [] {peter, paul};
await students.SaveAllAsync(); // or students.SaveAll();
```
