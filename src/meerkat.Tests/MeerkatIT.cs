using meerkat.Attributes;
using meerkat.Enums;
using MongoDB.Bson;
using MongoDB.Driver;
using OmniAssert;

namespace meerkat.Tests;

[CollectionDefinition("MeerkatIntegrationTests", DisableParallelization = true)]
public class MeerkatIntegrationTestsCollection;

internal static class TestDatabase
{
    private static string? _connectionString;

    public static string ConnectionString
    {
        get
        {
            if (_connectionString != null)
                return _connectionString;

            var envUri = Environment.GetEnvironmentVariable("MEERKAT_MONGO_URI");
            if (!string.IsNullOrWhiteSpace(envUri))
            {
                _connectionString = envUri;
                return _connectionString;
            }

            // Try authenticated connection first (common in local docker setup)
            try
            {
                var client = new MongoClient("mongodb://user:password@localhost:27017/testdb?authSource=admin");
                client.GetDatabase("testdb").RunCommand((Command<BsonDocument>)"{ping:1}");
                _connectionString = "mongodb://user:password@localhost:27017/testdb?authSource=admin";
                return _connectionString;
            }
            catch
            {
                _connectionString = "mongodb://localhost:27017/testdb";
                return _connectionString;
            }
        }
    }
}

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

[Xunit.Collection("MeerkatIntegrationTests")]
[Trait("Category", "Integration")]
public class MeerkatIT
{
    public MeerkatIT()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);
        Meerkat.Collection<IntegrationCounter, ObjectId>().DeleteMany(Builders<IntegrationCounter>.Filter.Empty);
    }

    [Fact]
    public void IncrementAndDecrementById_ShouldUpdateFieldValue()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);

        var counter = new IntegrationCounter { Name = "test", Value = 10 };
        counter.Save();

        Meerkat.IncrementById<IntegrationCounter, ObjectId, int>(counter.Id, x => x.Value, 5);
        var afterInc = Meerkat.FindById<IntegrationCounter, ObjectId>(counter.Id);
        afterInc.Value.Must().Be(15);

        Meerkat.DecrementById<IntegrationCounter, ObjectId, int>(counter.Id, x => x.Value, 3);
        var afterDec = Meerkat.FindById<IntegrationCounter, ObjectId>(counter.Id);
        afterDec.Value.Must().Be(12);
    }

    [Fact]
    public void IncrementOneAndGetUpdated_ShouldReturnUpdatedDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);

        var counter = new IntegrationCounter { Name = "get-updated", Score = 50.5 };
        counter.Save();

        var updated = Meerkat.IncrementOneAndGetUpdated<IntegrationCounter, ObjectId, double>(
            x => x.Name == "get-updated", x => x.Score, 10.5);

        updated.Must().NotBeNull();
        updated.Score.Must().Be(61.0);
    }

    [Fact]
    public void IncrementMany_ShouldUpdateAllMatchingDocuments()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);

        var c1 = new IntegrationCounter { Name = "group", Value = 1 };
        var c2 = new IntegrationCounter { Name = "group", Value = 2 };
        var c3 = new IntegrationCounter { Name = "other", Value = 100 };
        c1.Save();
        c2.Save();
        c3.Save();

        Meerkat.IncrementMany<IntegrationCounter, ObjectId, int>(x => x.Name == "group", x => x.Value, 10);

        var groupItems = Meerkat.Find<IntegrationCounter, ObjectId>(x => x.Name == "group");
        groupItems.Must().HaveCount(2);
        foreach (var item in groupItems)
            item.Value.Must().BeGreaterThan(10);

        var other = Meerkat.FindOne<IntegrationCounter, ObjectId>(x => x.Name == "other");
        other.Must().NotBeNull();
        other.Value.Must().Be(100);
    }

    [Fact]
    public void DecrementByFilter_ShouldUpdateMatchingDocument()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);

        var counter = new IntegrationCounter { Name = "filter-test", Total = 100 };
        counter.Save();

        var filter = Builders<IntegrationCounter>.Filter.Eq(x => x.Name, "filter-test");
        Meerkat.DecrementByFilter<IntegrationCounter, ObjectId, long>(filter, x => x.Total, 25);

        var updated = Meerkat.FindById<IntegrationCounter, ObjectId>(counter.Id);
        updated.Must().NotBeNull();
        updated.Total.Must().Be(75);
    }

    [Fact]
    public void SequentialPartialSets_ShouldNotClobberIndependentFields()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);

        var counter = new IntegrationCounter { Name = "partial", Value = 1 };
        counter.Save();

        Meerkat.Update<IntegrationCounter, ObjectId>(counter.Id)
            .Set(x => x.Name, "renamed")
            .Execute();

        Meerkat.Update<IntegrationCounter, ObjectId>(counter.Id)
            .Set(x => x.Value, 99)
            .Execute();

        var updated = Meerkat.FindById<IntegrationCounter, ObjectId>(counter.Id);
        updated.Must().NotBeNull();
        updated.Name.Must().Be("renamed");
        updated.Value.Must().Be(99);
    }
}

[Attributes.Collection(Name = "integration_soft_delete", SoftDelete = true, TrackTimestamps = true)]
public class IntegrationSoftDeleteDoc : Schema<ObjectId>
{
    public string Name { get; set; }

    public IntegrationSoftDeleteDoc()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Xunit.Collection("MeerkatIntegrationTests")]
[Trait("Category", "Integration")]
public class MeerkatSoftDeleteIT
{
    public MeerkatSoftDeleteIT()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);
        Meerkat.Collection<IntegrationSoftDeleteDoc, ObjectId>().DeleteMany(Builders<IntegrationSoftDeleteDoc>.Filter.Empty);
    }

    [Fact]
    public void SoftDelete_ShouldHideRestoreAndHardRemoveDocuments()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);

        var doc = new IntegrationSoftDeleteDoc { Name = "keep-me" };
        doc.Save();

        Meerkat.RemoveById<IntegrationSoftDeleteDoc, ObjectId>(doc.Id);

        var hidden = Meerkat.FindById<IntegrationSoftDeleteDoc, ObjectId>(doc.Id);
        hidden.Must().BeNull();

        var foundDeleted = Meerkat.FindById<IntegrationSoftDeleteDoc, ObjectId>(doc.Id, includeDeleted: true);
        foundDeleted.Must().NotBeNull();
        foundDeleted.IsDeleted.Must().BeTrue();
        foundDeleted.DeletedAt.Must().NotBeNull();

        var includingDeleted = Meerkat.Query<IntegrationSoftDeleteDoc, ObjectId>(includeDeleted: true)
            .Where(x => x.Id == doc.Id)
            .ToList();
        includingDeleted.Must().HaveCount(1);

        Meerkat.Count<IntegrationSoftDeleteDoc, ObjectId>(x => x.Name == "keep-me").Must().Be(0);
        Meerkat.Count<IntegrationSoftDeleteDoc, ObjectId>(x => x.Name == "keep-me", includeDeleted: true).Must().Be(1);

        Meerkat.RestoreById<IntegrationSoftDeleteDoc, ObjectId>(doc.Id);
        var restored = Meerkat.FindById<IntegrationSoftDeleteDoc, ObjectId>(doc.Id);
        restored.Must().NotBeNull();
        restored.IsDeleted.Must().BeFalse();
        restored.Name.Must().Be("keep-me");

        Meerkat.HardRemoveById<IntegrationSoftDeleteDoc, ObjectId>(doc.Id);
        Meerkat.FindById<IntegrationSoftDeleteDoc, ObjectId>(doc.Id, includeDeleted: true).Must().BeNull();
    }
}

[Attributes.Collection(Name = "integration_compound_unique")]
public class IntegrationCompoundUniqueDoc : Schema<ObjectId>
{
    [CompoundIndex(Name = "ux", IndexOrder = IndexOrder.Ascending, Unique = true)]
    public string InitiatorId { get; set; }

    [CompoundIndex(Name = "ux", IndexOrder = IndexOrder.Descending, Unique = true)]
    public int Year { get; set; }

    public IntegrationCompoundUniqueDoc()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Xunit.Collection("MeerkatIntegrationTests")]
[Trait("Category", "Integration")]
public class MeerkatCompoundUniqueIndexIT
{
    public MeerkatCompoundUniqueIndexIT()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);
        Meerkat.Collection<IntegrationCompoundUniqueDoc, ObjectId>().DeleteMany(Builders<IntegrationCompoundUniqueDoc>.Filter.Empty);
    }

    [Fact]
    public void CompoundUniqueIndex_ShouldRejectDuplicateEntries()
    {
        var doc1 = new IntegrationCompoundUniqueDoc { InitiatorId = "user_123", Year = 2026 };
        doc1.Save();

        var doc2 = new IntegrationCompoundUniqueDoc { InitiatorId = "user_123", Year = 2026 };
        Action act = () => doc2.Save();
        act.Throws<MongoWriteException>();
    }

    [Fact]
    public void CompoundUniqueIndex_ShouldAllowDifferentCombinations()
    {
        var doc1 = new IntegrationCompoundUniqueDoc { InitiatorId = "user_123", Year = 2025 };
        var doc2 = new IntegrationCompoundUniqueDoc { InitiatorId = "user_123", Year = 2026 };
        var doc3 = new IntegrationCompoundUniqueDoc { InitiatorId = "user_456", Year = 2026 };

        doc1.Save();
        doc2.Save();
        doc3.Save();

        var count = Meerkat.Count<IntegrationCompoundUniqueDoc, ObjectId>();
        count.Must().Be(3);
    }
}

[Attributes.Collection(Name = "integration_ttl")]
public class IntegrationTtlDoc : Schema<ObjectId>
{
    [SingleFieldIndex(Name = "ttl_idx", ExpireAfter = "1s")]
    public DateTime CreatedAtUtc { get; set; }

    public string Payload { get; set; }

    public IntegrationTtlDoc()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Xunit.Collection("MeerkatIntegrationTests")]
[Trait("Category", "Integration")]
public class MeerkatTtlIndexIT
{
    public MeerkatTtlIndexIT()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);
        Meerkat.Collection<IntegrationTtlDoc, ObjectId>().DeleteMany(Builders<IntegrationTtlDoc>.Filter.Empty);
    }

    [Fact]
    public void TtlIndex_ShouldBeCreatedWithOptions()
    {
        var doc = new IntegrationTtlDoc { CreatedAtUtc = DateTime.UtcNow, Payload = "test-ttl" };
        doc.Save();

        var collection = Meerkat.Collection<IntegrationTtlDoc, ObjectId>();
        var indexes = collection.Indexes.List().ToList();
        var ttlIndex = indexes.FirstOrDefault(idx => idx["name"].AsString == "ttl_idx");

        ttlIndex.Must().NotBeNull();
        ttlIndex!["expireAfterSeconds"].ToInt64().Must().Be(1);
    }
}
