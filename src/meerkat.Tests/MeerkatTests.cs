using meerkat.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.CollectionDefinition("MeerkatTests", DisableParallelization = true)]
public class MeerkatTestsCollection;

[Xunit.CollectionDefinition("MeerkatUnitTests", DisableParallelization = true)]
public class MeerkatUnitTestsCollection;

[Xunit.Collection("MeerkatTests")]
public class MeerkatTests
{
    [Fact]
    public void Connect_ShouldInitializeDatabase()
    {
        Meerkat.ResetDatabase();
        var connectionString = "mongodb://localhost:27017/testdb";
        Meerkat.Connect(connectionString);
        Meerkat.Database.Verify().NotToBeNull();
    }

    [Fact]
    public void Database_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        var act = () => { _ = Meerkat.Database; };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [meerkat.Attributes.Collection(Name = "test_students")]
    private class TestStudent : Schema<Guid>
    {
        public string FirstName { get; set; }
    }

    private class TestProduct : Schema<ObjectId>
    {
        public string Name { get; set; }
    }

    [Fact]
    public void Collection_ShouldReturnIMongoCollection_WithCustomCollectionName()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");
        var collection = Meerkat.Collection<TestStudent, Guid>();
        collection.Verify().NotToBeNull();
        collection.CollectionNamespace.CollectionName.Verify().ToBe("test_students");
    }

    [Fact]
    public void Collection_ShouldReturnIMongoCollection_WithDefaultCollectionName()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");
        var collection = Meerkat.Collection<TestProduct, ObjectId>();
        collection.Verify().NotToBeNull();
        collection.CollectionNamespace.CollectionName.Verify().ToBe("testproducts");
    }

    [Fact]
    public void Collection_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        var act = () => { _ = Meerkat.Collection<TestStudent, Guid>(); };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
