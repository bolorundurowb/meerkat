using meerkat.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.CollectionDefinition("MeerkatIntegrationTests", DisableParallelization = true)]
public class MeerkatIntegrationTestsCollection;

[meerkat.Attributes.Collection(Name = "update_test_counters")]
public class Counter : Schema<ObjectId>
{
    public string Name { get; set; }
    public int Value { get; set; }
    public long Total { get; set; }
    public double Score { get; set; }

    public Counter()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Xunit.Collection("MeerkatIntegrationTests")]
public class MeerkatUpdateIntegrationTests
{
    [Fact]
    public void IncrementById_ShouldIncreaseFieldByOne_WhenNoAmountProvided()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "test", Value = 10 };
        counter.Save();

        Meerkat.IncrementById<Counter, ObjectId, int>(counter.Id, x => x.Value);

        var updated = Meerkat.FindById<Counter, ObjectId>(counter.Id);
        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(11);
    }

    [Fact]
    public void IncrementById_ShouldIncreaseFieldByAmount_WhenAmountProvided()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "test", Value = 10 };
        counter.Save();

        Meerkat.IncrementById<Counter, ObjectId, int>(counter.Id, x => x.Value, 5);

        var updated = Meerkat.FindById<Counter, ObjectId>(counter.Id);
        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(15);
    }

    [Fact]
    public void DecrementById_ShouldDecreaseFieldByOne_WhenNoAmountProvided()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "test", Value = 10 };
        counter.Save();

        Meerkat.DecrementById<Counter, ObjectId, int>(counter.Id, x => x.Value);

        var updated = Meerkat.FindById<Counter, ObjectId>(counter.Id);
        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(9);
    }

    [Fact]
    public void DecrementById_ShouldDecreaseFieldByAmount_WhenAmountProvided()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "test", Value = 10 };
        counter.Save();

        Meerkat.DecrementById<Counter, ObjectId, int>(counter.Id, x => x.Value, 4);

        var updated = Meerkat.FindById<Counter, ObjectId>(counter.Id);
        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(6);
    }

    [Fact]
    public void IncrementByFilter_ShouldUpdateMatchingDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "filter-test", Value = 20 };
        counter.Save();

        var filter = Builders<Counter>.Filter.Eq(x => x.Name, "filter-test");
        Meerkat.IncrementByFilter<Counter, ObjectId, int>(filter, x => x.Value, 7);

        var updated = Meerkat.FindById<Counter, ObjectId>(counter.Id);
        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(27);
    }

    [Fact]
    public void DecrementByFilter_ShouldUpdateMatchingDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "filter-test-2", Value = 30 };
        counter.Save();

        var filter = Builders<Counter>.Filter.Eq(x => x.Name, "filter-test-2");
        Meerkat.DecrementByFilter<Counter, ObjectId, int>(filter, x => x.Value, 8);

        var updated = Meerkat.FindById<Counter, ObjectId>(counter.Id);
        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(22);
    }

    [Fact]
    public void IncrementOne_ShouldUpdateFirstMatchingDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "alpha", Value = 5 };
        counter.Save();

        Meerkat.IncrementOne<Counter, ObjectId, int>(x => x.Name == "alpha", x => x.Value, 3);

        var updated = Meerkat.FindOne<Counter, ObjectId>(x => x.Name == "alpha");
        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(8);
    }

    [Fact]
    public void DecrementOne_ShouldUpdateFirstMatchingDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "beta", Value = 20 };
        counter.Save();

        Meerkat.DecrementOne<Counter, ObjectId, int>(x => x.Name == "beta", x => x.Value, 7);

        var updated = Meerkat.FindOne<Counter, ObjectId>(x => x.Name == "beta");
        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(13);
    }

    [Fact]
    public void IncrementMany_ShouldUpdateAllMatchingDocuments()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var c1 = new Counter { Name = "group", Value = 1 };
        var c2 = new Counter { Name = "group", Value = 2 };
        var c3 = new Counter { Name = "other", Value = 100 };
        c1.Save();
        c2.Save();
        c3.Save();

        Meerkat.IncrementMany<Counter, ObjectId, int>(x => x.Name == "group", x => x.Value, 10);

        var groupItems = Meerkat.Find<Counter, ObjectId>(x => x.Name == "group");
        groupItems.Verify().ToHaveCount(2);
        foreach (var item in groupItems)
            item.Value.Verify().ToBeGreaterThan(10);

        var other = Meerkat.FindOne<Counter, ObjectId>(x => x.Name == "other");
        other.Verify().NotToBeNull();
        other.Value.Verify().ToBe(100);
    }

    [Fact]
    public void DecrementMany_ShouldUpdateAllMatchingDocuments()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var c1 = new Counter { Name = "decrement-group", Value = 50 };
        var c2 = new Counter { Name = "decrement-group", Value = 30 };
        var c3 = new Counter { Name = "untouched", Value = 999 };
        c1.Save();
        c2.Save();
        c3.Save();

        Meerkat.DecrementMany<Counter, ObjectId, int>(x => x.Name == "decrement-group", x => x.Value, 10);

        var groupItems = Meerkat.Find<Counter, ObjectId>(x => x.Name == "decrement-group");
        groupItems.Verify().ToHaveCount(2);
        groupItems.Verify().AllSatisfy(item => item.Value < 50);

        var untouched = Meerkat.FindOne<Counter, ObjectId>(x => x.Name == "untouched");
        untouched.Verify().NotToBeNull();
        untouched.Value.Verify().ToBe(999);
    }

    [Fact]
    public void IncrementByIdAndGetUpdated_ShouldReturnUpdatedDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "test", Score = 50.5 };
        counter.Save();

        var updated = Meerkat.IncrementByIdAndGetUpdated<Counter, ObjectId, double>(counter.Id, x => x.Score, 10.5);

        updated.Verify().NotToBeNull();
        updated.Id.Verify().ToBe(counter.Id);
        updated.Score.Verify().ToBe(61.0);
    }

    [Fact]
    public void DecrementByIdAndGetUpdated_ShouldReturnUpdatedDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "test", Total = 100 };
        counter.Save();

        var updated = Meerkat.DecrementByIdAndGetUpdated<Counter, ObjectId, long>(counter.Id, x => x.Total, 25);

        updated.Verify().NotToBeNull();
        updated.Id.Verify().ToBe(counter.Id);
        updated.Total.Verify().ToBe(75);
    }

    [Fact]
    public void IncrementOneAndGetUpdated_ShouldReturnUpdatedDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "get-updated-test", Value = 15 };
        counter.Save();

        var updated = Meerkat.IncrementOneAndGetUpdated<Counter, ObjectId, int>(x => x.Name == "get-updated-test", x => x.Value, 5);

        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(20);
    }

    [Fact]
    public void DecrementOneAndGetUpdated_ShouldReturnUpdatedDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");

        var counter = new Counter { Name = "decrement-get-updated", Value = 50 };
        counter.Save();

        var updated = Meerkat.DecrementOneAndGetUpdated<Counter, ObjectId, int>(x => x.Name == "decrement-get-updated", x => x.Value, 12);

        updated.Verify().NotToBeNull();
        updated.Value.Verify().ToBe(38);
    }
}
