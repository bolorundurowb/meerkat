using MongoDB.Bson;
using MongoDB.Driver;
using OmniAssert;

namespace meerkat.Tests;

[CollectionDefinition("MeerkatIntegrationTests", DisableParallelization = true)]
public class MeerkatIntegrationTestsCollection;

[Attributes.Collection(Name = "integration_test_counters")]
public class IntegrationCounter : Schema<ObjectId>
{
    public string Name { get; set; }
    public int Value { get; set; }
    public long Total { get; set; }
    public double Score { get; set; }

    public IntegrationCounter()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Collection("MeerkatIntegrationTests")]
[Trait("Category", "Integration")]
public class MeerkatIT
{
    [Fact]
    public void IncrementAndDecrementById_ShouldUpdateFieldValue()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new IntegrationCounter { Name = "test", Value = 10 };
        counter.Save();

        Meerkat.IncrementById<IntegrationCounter, ObjectId, int>(counter.Id, x => x.Value, 5);
        var afterInc = Meerkat.FindById<IntegrationCounter, ObjectId>(counter.Id);
        afterInc.Value.Verify().ToBe(15);

        Meerkat.DecrementById<IntegrationCounter, ObjectId, int>(counter.Id, x => x.Value, 3);
        var afterDec = Meerkat.FindById<IntegrationCounter, ObjectId>(counter.Id);
        afterDec.Value.Verify().ToBe(12);
    }

    [Fact]
    public void IncrementOneAndGetUpdated_ShouldReturnUpdatedDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new IntegrationCounter { Name = "get-updated", Score = 50.5 };
        counter.Save();

        var updated = Meerkat.IncrementOneAndGetUpdated<IntegrationCounter, ObjectId, double>(
            x => x.Name == "get-updated", x => x.Score, 10.5);

        updated.Verify().NotToBeNull();
        updated.Score.Verify().ToBe(61.0);
    }

    [Fact]
    public void IncrementMany_ShouldUpdateAllMatchingDocuments()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var c1 = new IntegrationCounter { Name = "group", Value = 1 };
        var c2 = new IntegrationCounter { Name = "group", Value = 2 };
        var c3 = new IntegrationCounter { Name = "other", Value = 100 };
        c1.Save();
        c2.Save();
        c3.Save();

        Meerkat.IncrementMany<IntegrationCounter, ObjectId, int>(x => x.Name == "group", x => x.Value, 10);

        var groupItems = Meerkat.Find<IntegrationCounter, ObjectId>(x => x.Name == "group");
        groupItems.Verify().ToHaveCount(2);
        foreach (var item in groupItems)
            item.Value.Verify().ToBeGreaterThan(10);

        var other = Meerkat.FindOne<IntegrationCounter, ObjectId>(x => x.Name == "other");
        other.Verify().NotToBeNull();
        other.Value.Verify().ToBe(100);
    }

    [Fact]
    public void DecrementByFilter_ShouldUpdateMatchingDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new IntegrationCounter { Name = "filter-test", Total = 100 };
        counter.Save();

        var filter = Builders<IntegrationCounter>.Filter.Eq(x => x.Name, "filter-test");
        Meerkat.DecrementByFilter<IntegrationCounter, ObjectId, long>(filter, x => x.Total, 25);

        var updated = Meerkat.FindById<IntegrationCounter, ObjectId>(counter.Id);
        updated.Verify().NotToBeNull();
        updated.Total.Verify().ToBe(75);
    }
}
