using meerkat.Attributes;
using meerkat.Enums;
using MongoDB.Bson;
using MongoDB.Driver;
using OmniAssert;

namespace meerkat.Tests;

[Attributes.Collection(Name = "idx_ensure_entities")]
public class IndexEnsureEntity : Schema<ObjectId>
{
    [UniqueIndex(Name = "idx_unique_name")]
    public string Name { get; set; }

    [CompoundIndex(Name = "idx_cmp", IndexOrder = IndexOrder.Ascending)]
    public string Category { get; set; }

    [CompoundIndex(Name = "idx_cmp", IndexOrder = IndexOrder.Descending)]
    public decimal Amount { get; set; }

    public IndexEnsureEntity()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Xunit.Collection("MeerkatIntegrationTests")]
[Trait("Category", "Integration")]
public class MeerkatIndexingIT
{
    [Fact]
    public async Task EnsureIndexesAsync_CreatesAndVerifiesIndexes()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);

        await Meerkat.EnsureIndexesAsync<IndexEnsureEntity, ObjectId>();
        await Meerkat.EnsureIndexesAsync<IndexEnsureEntity, ObjectId>();

        var indexes = Meerkat.Collection<IndexEnsureEntity, ObjectId>().Indexes.List().ToList();

        indexes.Any(i => i["name"].AsString == "idx_unique_name").Must().BeTrue();
        indexes.Any(i => i["name"].AsString == "idx_cmp").Must().BeTrue();
    }

    [Fact]
    public async Task EnsureIndexesAsync_TypeList_CreatesForEach()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);

        await Meerkat.EnsureIndexesAsync(new[] { typeof(IndexEnsureEntity) });

        var indexes = Meerkat.Collection<IndexEnsureEntity, ObjectId>().Indexes.List().ToList();

        indexes.Any(i => i["name"].AsString == "idx_unique_name").Must().BeTrue();
    }
}
