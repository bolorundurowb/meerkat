using meerkat.Attributes;
using meerkat.Enums;
using meerkat.Exceptions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatIndexingTests
{
    public class IndexedEntity : Schema<Guid>
    {
        [UniqueIndex(Name = "unique_name", Sparse = true)]
        public string Name { get; set; }

        [SingleFieldIndex(Name = "single_age", IndexOrder = IndexOrder.Descending)]
        public int Age { get; set; }

        [GeospatialIndex(Name = "geo_location", IndexType = GeospatialIndexType.TwoDSphere)]
        public double[] Location { get; set; }

        [CompoundIndex(Name = "compound_idx", IndexOrder = IndexOrder.Ascending)]
        public string Category { get; set; }

        [CompoundIndex(Name = "compound_idx", IndexOrder = IndexOrder.Descending)]
        public decimal TotalAmount { get; set; }
    }

    public class NoIndexEntity : Schema<Guid>
    {
        public string NoName { get; set; }
    }

    public class SingleFieldAscEntity : Schema<Guid>
    {
        [SingleFieldIndex(IndexOrder = IndexOrder.Ascending)]
        public string Name { get; set; }
    }

    public class SingleFieldHashedEntity : Schema<Guid>
    {
        [SingleFieldIndex(IndexOrder = IndexOrder.Hashed)]
        public string Name { get; set; }
    }

    public class Geospatial2DEntity : Schema<Guid>
    {
        [GeospatialIndex(IndexType = GeospatialIndexType.TwoD)]
        public double[] Location { get; set; }
    }

    [Attributes.Collection(SoftDelete = true)]
    public class SoftDeleteIndexedEntity : Schema<Guid>
    {
        public string Name { get; set; }
    }

    public abstract class AbstractSchema : Schema<Guid>
    {
    }

    public class NotASchema
    {
        public string Name { get; set; }
    }

    public class CompoundUniqueIndexEntity : Schema<Guid>
    {
        [CompoundIndex(Name = "ux", IndexOrder = IndexOrder.Ascending, Unique = true)]
        public string InitiatorId { get; set; }

        [CompoundIndex(Name = "ux", IndexOrder = IndexOrder.Descending, Unique = true)]
        public int Year { get; set; }
    }

    public class CompoundNonUniqueIndexEntity : Schema<Guid>
    {
        [CompoundIndex(Name = "non_ux", IndexOrder = IndexOrder.Ascending, Unique = false)]
        public string InitiatorId { get; set; }

        [CompoundIndex(Name = "non_ux", IndexOrder = IndexOrder.Descending)]
        public int Year { get; set; }
    }

    public class CompoundMixedUniqueIndexEntity : Schema<Guid>
    {
        [CompoundIndex(Name = "mixed_ux", Unique = true)]
        public string FieldA { get; set; }

        [CompoundIndex(Name = "mixed_ux", Unique = false)]
        public string FieldB { get; set; }
    }

    public class CompoundMultiOrderEntity : Schema<Guid>
    {
        [CompoundIndex(Name = "multi_order_idx", IndexOrder = IndexOrder.Descending)]
        public string FieldDesc { get; set; }

        [CompoundIndex(Name = "multi_order_idx", IndexOrder = IndexOrder.Hashed)]
        public string FieldHashed { get; set; }
    }

    public class InvalidOrderSingleEntity : Schema<Guid>
    {
        [SingleFieldIndex(IndexOrder = (IndexOrder)999)]
        public string Field { get; set; }
    }

    public class InvalidGeospatialEntity : Schema<Guid>
    {
        [GeospatialIndex(IndexType = (GeospatialIndexType)999)]
        public double[] Location { get; set; }
    }

    public class InvalidOrderCompoundEntity : Schema<Guid>
    {
        [CompoundIndex(Name = "invalid_compound", IndexOrder = (IndexOrder)999)]
        public string Field { get; set; }
    }

    public class IntermediateBaseSchema<TId> : Schema<TId> where TId : IEquatable<TId>
    {
    }

    public class DeepDerivedSchema : IntermediateBaseSchema<Guid>
    {
        [UniqueIndex(Name = "deep_unique")]
        public string DeepCode { get; set; }
    }

    public class SingleFieldUnnamedEntity : Schema<Guid>
    {
        [SingleFieldIndex(IndexOrder = IndexOrder.Ascending)]
        public string Title { get; set; }
    }

    public class UniqueUnnamedEntity : Schema<Guid>
    {
        [UniqueIndex(Sparse = true)]
        public string Sku { get; set; }
    }

    public class SingleFieldTtlEntity : Schema<Guid>
    {
        [SingleFieldIndex(Name = "ttl_idx", ExpireAfter = "30d")]
        public DateTime Timestamp { get; set; }
    }

    public class SingleFieldTtlNullableDateTimeEntity : Schema<Guid>
    {
        [SingleFieldIndex(Name = "ttl_null_idx", ExpireAfter = "12h")]
        public DateTime? ExpireAt { get; set; }
    }

    public class SingleFieldTtlInvalidTypeEntity : Schema<Guid>
    {
        [SingleFieldIndex(ExpireAfter = "30d")]
        public string NotADate { get; set; }
    }

    public class SingleFieldNoExpireAfterEntity : Schema<Guid>
    {
        [SingleFieldIndex]
        public DateTime Timestamp { get; set; }
    }

    private readonly Mock<IMongoCollection<IndexedEntity>> _mockCollection;
    private readonly Mock<IMongoIndexManager<IndexedEntity>> _mockIndexes;

    public MeerkatIndexingTests()
    {
        _mockCollection = new Mock<IMongoCollection<IndexedEntity>>();
        _mockIndexes = new Mock<IMongoIndexManager<IndexedEntity>>();
        _mockCollection.Setup(x => x.Indexes).Returns(_mockIndexes.Object);
        _mockCollection.Setup(x => x.CollectionNamespace)
            .Returns(CollectionNamespace.FromFullName("testdb.indexedentities"));
        _mockCollection.Setup(x => x.DocumentSerializer)
            .Returns(BsonSerializer.LookupSerializer<IndexedEntity>());
        _mockCollection.Setup(x => x.Settings)
            .Returns(new MongoCollectionSettings());
        Meerkat.SchemasWithCheckedIndices.Clear();
    }

    [Fact]
    public void BuildIndexModels_ShouldReturnAllAttributeIndexes()
    {
        var models = Meerkat.BuildIndexModels<IndexedEntity>(typeof(IndexedEntity));

        models.Select(m => m.Options.Name).Must().Contain("unique_name");
        models.Select(m => m.Options.Name).Must().Contain("single_age");
        models.Select(m => m.Options.Name).Must().Contain("geo_location");
        models.Select(m => m.Options.Name).Must().Contain("compound_idx");
        models.Must().HaveCount(4);
    }

    [Fact]
    public void BuildUniqueIndexModels_ShouldSetUniqueAndSparseOptions()
    {
        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));

        models.Must().HaveCount(1);
        var model = models[0];
        model.Options.Name.Must().Be("unique_name");
        model.Options.Unique.Value.Must().BeTrue();
        model.Options.Sparse.Value.Must().BeTrue();
    }

    [Fact]
    public void BuildSingleFieldIndexModels_ShouldHonourIndexOrder()
    {
        var ascending = Meerkat.BuildSingleFieldIndexModels<SingleFieldAscEntity>(typeof(SingleFieldAscEntity));
        ascending.Must().HaveCount(1);
        RenderKeys(ascending[0]).GetValue("Name", 0).ToInt32().Must().Be(1);

        var hashed = Meerkat.BuildSingleFieldIndexModels<SingleFieldHashedEntity>(typeof(SingleFieldHashedEntity));
        hashed.Must().HaveCount(1);
        RenderKeys(hashed[0]).GetValue("Name", "").AsString.Must().Be("hashed");
    }

    [Fact]
    public void BuildGeospatialIndexModels_ShouldHonourIndexType()
    {
        var twoD = Meerkat.BuildGeospatialIndexModels<Geospatial2DEntity>(typeof(Geospatial2DEntity));
        twoD.Must().HaveCount(1);
        RenderKeys(twoD[0]).GetValue("Location", "").AsString.Must().Be("2d");

        var sphere = Meerkat.BuildGeospatialIndexModels<IndexedEntity>(typeof(IndexedEntity));
        sphere.Must().HaveCount(1);
        RenderKeys(sphere[0]).GetValue("Location", "").AsString.Must().Be("2dsphere");
    }

    [Fact]
    public void BuildIndexModels_ShouldReturnEmpty_WhenNoAttributes()
    {
        Meerkat.BuildIndexModels<NoIndexEntity>(typeof(NoIndexEntity)).Must().HaveCount(0);
    }

    [Fact]
    public void BuildIndexModels_ShouldIncludeDeletedAtIndex_WhenSoftDeleteEnabled()
    {
        var models = Meerkat.BuildIndexModels<SoftDeleteIndexedEntity>(typeof(SoftDeleteIndexedEntity));

        models.Select(m => m.Options.Name).Must().Contain("deleted_at_idx");
    }

    [Fact]
    public void HandleIndexing_ShouldApplyAllModelsOnce()
    {
        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), _mockCollection.Object);

        _mockIndexes.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<IndexedEntity>>>(models => models.Count() == 4),
            It.IsAny<CancellationToken>()), Times.Once);
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Must().BeTrue();
    }

    [Fact]
    public void HandleIndexing_ShouldOnlyRunOnce_PerType()
    {
        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), _mockCollection.Object);
        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), _mockCollection.Object);

        _mockIndexes.Verify(x => x.CreateMany(
            It.IsAny<IEnumerable<CreateIndexModel<IndexedEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void BuildCompoundIndexModels_WithUniqueOption_ShouldSetUniqueFlag()
    {
        var models = Meerkat.BuildCompoundIndexModels<CompoundUniqueIndexEntity>(typeof(CompoundUniqueIndexEntity));
        models.Must().HaveCount(1);
        models[0].Options.Name.Must().Be("ux");
        models[0].Options.Unique.Must().Be(true);
    }

    [Fact]
    public void BuildCompoundIndexModels_WithNonUniqueOption_ShouldSetUniqueFalse()
    {
        var models = Meerkat.BuildCompoundIndexModels<CompoundNonUniqueIndexEntity>(typeof(CompoundNonUniqueIndexEntity));
        models.Must().HaveCount(1);
        models[0].Options.Name.Must().Be("non_ux");
        models[0].Options.Unique.Must().Be(false);
    }

    [Fact]
    public void BuildCompoundIndexModels_WithMixedUniqueOptions_ShouldThrowInvalidAttributeException()
    {
        Action act = () => Meerkat.BuildCompoundIndexModels<CompoundMixedUniqueIndexEntity>(typeof(CompoundMixedUniqueIndexEntity));

        act.Throws<InvalidAttributeException>()
            .WithMessage("Members of a compound index group must agree on the 'Unique' value.");
    }

    [Fact]
    public void BuildSingleFieldIndexModels_WithExpireAfter_ShouldSetExpireAfterOption()
    {
        var models = Meerkat.BuildSingleFieldIndexModels<SingleFieldTtlEntity>(typeof(SingleFieldTtlEntity));

        models.Must().HaveCount(1);
        models[0].Options.Name.Must().Be("ttl_idx");
        models[0].Options.ExpireAfter.Must().Be(TimeSpan.FromDays(30));
    }

    [Fact]
    public void BuildSingleFieldIndexModels_WithNullableDateTimeAndExpireAfter_ShouldSetExpireAfterOption()
    {
        var models = Meerkat.BuildSingleFieldIndexModels<SingleFieldTtlNullableDateTimeEntity>(typeof(SingleFieldTtlNullableDateTimeEntity));

        models.Must().HaveCount(1);
        models[0].Options.Name.Must().Be("ttl_null_idx");
        models[0].Options.ExpireAfter.Must().Be(TimeSpan.FromHours(12));
    }

    [Fact]
    public void BuildSingleFieldIndexModels_WithExpireAfterOnInvalidType_ShouldThrowInvalidAttributeException()
    {
        Action act = () => Meerkat.BuildSingleFieldIndexModels<SingleFieldTtlInvalidTypeEntity>(typeof(SingleFieldTtlInvalidTypeEntity));

        act.Throws<InvalidAttributeException>()
            .WithMessage("The 'ExpireAfter' TTL option can only be applied to DateTime or DateTime? fields.");
    }

    [Fact]
    public void BuildSingleFieldIndexModels_WithoutExpireAfter_ShouldHaveNullExpireAfter()
    {
        var models = Meerkat.BuildSingleFieldIndexModels<SingleFieldNoExpireAfterEntity>(typeof(SingleFieldNoExpireAfterEntity));

        models.Must().HaveCount(1);
        models[0].Options.ExpireAfter.Must().BeNull();
    }

    [Fact]
    public void HandleIndexing_ShouldThrow_WhenIndexManagerIsNull()
    {
        var mockCol = new Mock<IMongoCollection<IndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns((IMongoIndexManager<IndexedEntity>?)null);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.indexedentities"));

        var act = () => Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), mockCol.Object);

        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public void VerifyIndexes_ShouldThrow_WhenNamedIndexMissing()
    {
        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));
        _mockIndexes.Setup(x => x.List()).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "_id_" }, { "key", new BsonDocument { { "_id", 1 } } } }));

        var act = () => Meerkat.VerifyIndexes(models, _mockCollection.Object);

        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public void VerifyIndexes_ShouldNotThrow_WhenAllNamedPresent()
    {
        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));
        _mockIndexes.Setup(x => x.List()).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "_id_" }, { "key", new BsonDocument { { "_id", 1 } } } },
            new BsonDocument { { "name", "unique_name" }, { "key", new BsonDocument { { "Name", 1 } } } }));

        Meerkat.VerifyIndexes(models, _mockCollection.Object);
    }

    [Fact]
    public void VerifyIndexes_ShouldNotThrow_WhenNoModels()
    {
        Meerkat.VerifyIndexes(new List<CreateIndexModel<IndexedEntity>>(), _mockCollection.Object);
    }

    [Fact]
    public void GetSchemaTypes_ShouldReturnOnlyConcreteSchemaTypes()
    {
        var types = Meerkat.GetSchemaTypes(typeof(IndexedEntity).Assembly).ToList();

        types.Must().Contain(typeof(IndexedEntity));
        types.Must().NotContain(typeof(AbstractSchema));
        types.Must().NotContain(typeof(NotASchema));
    }

    [Fact]
    public void ApplyIndexes_ShouldDoNothing_WhenModelsListIsEmpty()
    {
        Meerkat.ApplyIndexes(new List<CreateIndexModel<IndexedEntity>>(), _mockCollection.Object);
        _mockIndexes.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<IndexedEntity>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void ApplyIndexes_ShouldThrow_WhenIndexesIsNull()
    {
        var mockCol = new Mock<IMongoCollection<IndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns((IMongoIndexManager<IndexedEntity>?)null);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.indexedentities"));

        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));
        var act = () => Meerkat.ApplyIndexes(models, mockCol.Object);

        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public async Task ApplyIndexesAsync_ShouldDoNothing_WhenModelsListIsEmpty()
    {
        await Meerkat.ApplyIndexesAsync(new List<CreateIndexModel<IndexedEntity>>(), _mockCollection.Object);
        _mockIndexes.Verify(x => x.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<IndexedEntity>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApplyIndexesAsync_ShouldThrow_WhenIndexesIsNull()
    {
        var mockCol = new Mock<IMongoCollection<IndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns((IMongoIndexManager<IndexedEntity>?)null);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.indexedentities"));

        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));
        await Xunit.Assert.ThrowsAsync<IndexVerificationException>(() => Meerkat.ApplyIndexesAsync(models, mockCol.Object));
    }

    [Fact]
    public async Task ApplyIndexesAsync_ShouldCallCreateManyAsync_WhenModelsArePresent()
    {
        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));
        await Meerkat.ApplyIndexesAsync(models, _mockCollection.Object);

        _mockIndexes.Verify(x => x.CreateManyAsync(
            It.Is<IEnumerable<CreateIndexModel<IndexedEntity>>>(m => m.Count() == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyIndexesAsync_ShouldDoNothing_WhenModelsListIsEmpty()
    {
        await Meerkat.VerifyIndexesAsync(new List<CreateIndexModel<IndexedEntity>>(), _mockCollection.Object);
        _mockIndexes.Verify(x => x.ListAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyIndexesAsync_ShouldThrow_WhenIndexesIsNull()
    {
        var mockCol = new Mock<IMongoCollection<IndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns((IMongoIndexManager<IndexedEntity>?)null);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.indexedentities"));

        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));
        await Xunit.Assert.ThrowsAsync<IndexVerificationException>(() => Meerkat.VerifyIndexesAsync(models, mockCol.Object));
    }

    [Fact]
    public async Task VerifyIndexesAsync_ShouldThrow_WhenNamedIndexMissing()
    {
        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));
        _mockIndexes.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateAsyncIndexCursor(
                new BsonDocument { { "name", "_id_" }, { "key", new BsonDocument { { "_id", 1 } } } }));

        await Xunit.Assert.ThrowsAsync<IndexVerificationException>(() =>
            Meerkat.VerifyIndexesAsync(models, _mockCollection.Object));
    }

    [Fact]
    public async Task VerifyIndexesAsync_ShouldNotThrow_WhenAllNamedPresent()
    {
        var models = Meerkat.BuildUniqueIndexModels<IndexedEntity>(typeof(IndexedEntity));
        _mockIndexes.Setup(x => x.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateAsyncIndexCursor(
                new BsonDocument { { "name", "_id_" }, { "key", new BsonDocument { { "_id", 1 } } } },
                new BsonDocument { { "name", "unique_name" }, { "key", new BsonDocument { { "Name", 1 } } } }));

        await Meerkat.VerifyIndexesAsync(models, _mockCollection.Object);
    }

    [Fact]
    public void VerifyIndexes_ShouldMatchUnnamedIndex_WhenKeysAndOptionsMatch()
    {
        var mockCol = new Mock<IMongoCollection<UniqueUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<UniqueUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.uniqueunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<UniqueUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildUniqueIndexModels<UniqueUnnamedEntity>(typeof(UniqueUnnamedEntity));
        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "Sku_1" }, { "key", new BsonDocument { { "Sku", 1 } } }, { "unique", true }, { "sparse", true } }));

        Meerkat.VerifyIndexes(models, mockCol.Object);
    }

    [Fact]
    public void VerifyIndexes_ShouldThrow_WhenUnnamedIndexUniqueOptionMismatches()
    {
        var mockCol = new Mock<IMongoCollection<UniqueUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<UniqueUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.uniqueunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<UniqueUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildUniqueIndexModels<UniqueUnnamedEntity>(typeof(UniqueUnnamedEntity));
        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "Sku_1" }, { "key", new BsonDocument { { "Sku", 1 } } }, { "unique", false }, { "sparse", true } }));

        var act = () => Meerkat.VerifyIndexes(models, mockCol.Object);
        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public void VerifyIndexes_ShouldThrow_WhenUnnamedIndexSparseOptionMismatches()
    {
        var mockCol = new Mock<IMongoCollection<UniqueUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<UniqueUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.uniqueunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<UniqueUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildUniqueIndexModels<UniqueUnnamedEntity>(typeof(UniqueUnnamedEntity));
        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "Sku_1" }, { "key", new BsonDocument { { "Sku", 1 } } }, { "unique", true }, { "sparse", false } }));

        var act = () => Meerkat.VerifyIndexes(models, mockCol.Object);
        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public void VerifyIndexes_ShouldThrow_WhenUnnamedIndexKeyElementCountMismatches()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.singlefieldunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<SingleFieldUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildSingleFieldIndexModels<SingleFieldUnnamedEntity>(typeof(SingleFieldUnnamedEntity));
        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "other_idx" }, { "key", new BsonDocument { { "Title", 1 }, { "Extra", 1 } } } }));

        var act = () => Meerkat.VerifyIndexes(models, mockCol.Object);
        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public void VerifyIndexes_ShouldThrow_WhenUnnamedIndexKeyElementNameMismatches()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.singlefieldunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<SingleFieldUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildSingleFieldIndexModels<SingleFieldUnnamedEntity>(typeof(SingleFieldUnnamedEntity));
        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "other_idx" }, { "key", new BsonDocument { { "DifferentTitle", 1 } } } }));

        var act = () => Meerkat.VerifyIndexes(models, mockCol.Object);
        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public void VerifyIndexes_ShouldThrow_WhenUnnamedIndexKeyElementValueMismatches()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.singlefieldunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<SingleFieldUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildSingleFieldIndexModels<SingleFieldUnnamedEntity>(typeof(SingleFieldUnnamedEntity));
        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "other_idx" }, { "key", new BsonDocument { { "Title", -1 } } } }));

        var act = () => Meerkat.VerifyIndexes(models, mockCol.Object);
        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public void VerifyIndexes_ShouldThrow_WhenIndexDocumentMissingKeyProperty()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.singlefieldunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<SingleFieldUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildSingleFieldIndexModels<SingleFieldUnnamedEntity>(typeof(SingleFieldUnnamedEntity));
        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "no_key_doc" } }));

        var act = () => Meerkat.VerifyIndexes(models, mockCol.Object);
        act.Throws<IndexVerificationException>();
    }

    [Fact]
    public async Task VerifyIndexesAsync_ShouldMatchUnnamedIndex_WhenKeysAndOptionsMatch()
    {
        var mockCol = new Mock<IMongoCollection<UniqueUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<UniqueUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.uniqueunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<UniqueUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildUniqueIndexModels<UniqueUnnamedEntity>(typeof(UniqueUnnamedEntity));
        mockIdx.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateAsyncIndexCursor(
            new BsonDocument { { "name", "Sku_1" }, { "key", new BsonDocument { { "Sku", 1 } } }, { "unique", true }, { "sparse", true } }));

        await Meerkat.VerifyIndexesAsync(models, mockCol.Object);
    }

    [Fact]
    public async Task VerifyIndexesAsync_ShouldThrow_WhenUnnamedIndexMissing()
    {
        var mockCol = new Mock<IMongoCollection<UniqueUnnamedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<UniqueUnnamedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.uniqueunnamed"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<UniqueUnnamedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());

        var models = Meerkat.BuildUniqueIndexModels<UniqueUnnamedEntity>(typeof(UniqueUnnamedEntity));
        mockIdx.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateAsyncIndexCursor(
            new BsonDocument { { "name", "_id_" }, { "key", new BsonDocument { { "_id", 1 } } } }));

        await Xunit.Assert.ThrowsAsync<IndexVerificationException>(() =>
            Meerkat.VerifyIndexesAsync(models, mockCol.Object));
    }

    [Fact]
    public void EnsureIndexes_ShouldThrowArgumentNullException_WhenTypesIsNull()
    {
        var act = () => Meerkat.EnsureIndexes((IEnumerable<Type>)null!);
        act.Throws<ArgumentNullException>();
    }

    [Fact]
    public void EnsureIndexes_ShouldThrowArgumentNullException_WhenAssemblyIsNull()
    {
        var act = () => Meerkat.EnsureIndexes((System.Reflection.Assembly)null!);
        act.Throws<ArgumentNullException>();
    }

    [Fact]
    public async Task EnsureIndexesAsync_ShouldThrowArgumentNullException_WhenTypesIsNull()
    {
        await Xunit.Assert.ThrowsAsync<ArgumentNullException>(() =>
            Meerkat.EnsureIndexesAsync((IEnumerable<Type>)null!));
    }

    [Fact]
    public async Task EnsureIndexesAsync_ShouldThrowArgumentNullException_WhenAssemblyIsNull()
    {
        await Xunit.Assert.ThrowsAsync<ArgumentNullException>(() =>
            Meerkat.EnsureIndexesAsync((System.Reflection.Assembly)null!));
    }

    [Fact]
    public void EnsureIndexes_ShouldBeNoOp_WhenTypesIsEmpty()
    {
        Meerkat.EnsureIndexes(Array.Empty<Type>());
    }

    [Fact]
    public async Task EnsureIndexesAsync_ShouldBeNoOp_WhenTypesIsEmpty()
    {
        await Meerkat.EnsureIndexesAsync(Array.Empty<Type>());
    }

    [Fact]
    public void ResolveSchemaIdType_ShouldResolveDirectSchema()
    {
        var idType = Meerkat.ResolveSchemaIdType(typeof(IndexedEntity));
        idType.Must().Be(typeof(Guid));
    }

    [Fact]
    public void ResolveSchemaIdType_ShouldResolveDerivedSchemaHierarchy()
    {
        var idType = Meerkat.ResolveSchemaIdType(typeof(DeepDerivedSchema));
        idType.Must().Be(typeof(Guid));
    }

    [Fact]
    public void ResolveSchemaIdType_ShouldThrowArgumentException_WhenTypeDoesNotInheritSchema()
    {
        var act = () => Meerkat.ResolveSchemaIdType(typeof(NotASchema));
        act.Throws<ArgumentException>()
            .WithMessage("Type 'meerkat.Tests.MeerkatIndexingTests+NotASchema' does not inherit from Schema<TId>.");
    }

    [Fact]
    public void BuildSingleFieldIndexModels_ShouldThrowArgumentOutOfRangeException_ForInvalidIndexOrder()
    {
        var act = () => Meerkat.BuildSingleFieldIndexModels<InvalidOrderSingleEntity>(typeof(InvalidOrderSingleEntity));
        act.Throws<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BuildGeospatialIndexModels_ShouldThrowArgumentOutOfRangeException_ForInvalidGeospatialIndexType()
    {
        var act = () => Meerkat.BuildGeospatialIndexModels<InvalidGeospatialEntity>(typeof(InvalidGeospatialEntity));
        act.Throws<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BuildCompoundIndexModels_ShouldThrowArgumentOutOfRangeException_ForInvalidIndexOrder()
    {
        var act = () => Meerkat.BuildCompoundIndexModels<InvalidOrderCompoundEntity>(typeof(InvalidOrderCompoundEntity));
        act.Throws<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void BuildCompoundIndexModels_ShouldRenderDescendingAndHashedCompoundIndexesCorrectly()
    {
        var models = Meerkat.BuildCompoundIndexModels<CompoundMultiOrderEntity>(typeof(CompoundMultiOrderEntity));
        models.Must().HaveCount(1);
        models[0].Options.Name.Must().Be("multi_order_idx");

        var keys = RenderKeys(models[0]);
        keys.GetValue("FieldDesc").ToInt32().Must().Be(-1);
        keys.GetValue("FieldHashed").AsString.Must().Be("hashed");
    }

    [Fact]
    public void EnsureIndexes_Generic_ShouldEnsureIndexesForSchema()
    {
        var mockDb = new Mock<IMongoDatabase>();
        mockDb.Setup(x => x.GetCollection<IndexedEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockCollection.Object);

        _mockIndexes.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "unique_name" }, { "key", new BsonDocument { { "Name", 1 } } } },
            new BsonDocument { { "name", "single_age" }, { "key", new BsonDocument { { "Age", -1 } } } },
            new BsonDocument { { "name", "geo_location" }, { "key", new BsonDocument { { "Location", "2dsphere" } } } },
            new BsonDocument { { "name", "compound_idx" }, { "key", new BsonDocument { { "Category", 1 }, { "TotalAmount", -1 } } } }));

        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => mockDb.Object);

        Meerkat.EnsureIndexes<IndexedEntity, Guid>();

        _mockIndexes.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<IndexedEntity>>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Must().BeTrue();
    }

    [Fact]
    public async Task EnsureIndexesAsync_Generic_ShouldEnsureIndexesForSchema()
    {
        var mockDb = new Mock<IMongoDatabase>();
        mockDb.Setup(x => x.GetCollection<IndexedEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockCollection.Object);

        _mockIndexes.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateAsyncIndexCursor(
            new BsonDocument { { "name", "unique_name" }, { "key", new BsonDocument { { "Name", 1 } } } },
            new BsonDocument { { "name", "single_age" }, { "key", new BsonDocument { { "Age", -1 } } } },
            new BsonDocument { { "name", "geo_location" }, { "key", new BsonDocument { { "Location", "2dsphere" } } } },
            new BsonDocument { { "name", "compound_idx" }, { "key", new BsonDocument { { "Category", 1 }, { "TotalAmount", -1 } } } }));

        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => mockDb.Object);

        await Meerkat.EnsureIndexesAsync<IndexedEntity, Guid>();

        _mockIndexes.Verify(x => x.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<IndexedEntity>>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Must().BeTrue();
    }

    [Fact]
    public void EnsureIndexesOnCollection_ShouldApplyAndVerify()
    {
        _mockIndexes.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "unique_name" }, { "key", new BsonDocument { { "Name", 1 } } } },
            new BsonDocument { { "name", "single_age" }, { "key", new BsonDocument { { "Age", -1 } } } },
            new BsonDocument { { "name", "geo_location" }, { "key", new BsonDocument { { "Location", "2dsphere" } } } },
            new BsonDocument { { "name", "compound_idx" }, { "key", new BsonDocument { { "Category", 1 }, { "TotalAmount", -1 } } } }));

        Meerkat.EnsureIndexesOnCollection(typeof(IndexedEntity), _mockCollection.Object);

        _mockIndexes.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<IndexedEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Must().BeTrue();
    }

    [Fact]
    public async Task EnsureIndexesOnCollectionAsync_ShouldApplyAndVerifyAsync()
    {
        _mockIndexes.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateAsyncIndexCursor(
            new BsonDocument { { "name", "unique_name" }, { "key", new BsonDocument { { "Name", 1 } } } },
            new BsonDocument { { "name", "single_age" }, { "key", new BsonDocument { { "Age", -1 } } } },
            new BsonDocument { { "name", "geo_location" }, { "key", new BsonDocument { { "Location", "2dsphere" } } } },
            new BsonDocument { { "name", "compound_idx" }, { "key", new BsonDocument { { "Category", 1 }, { "TotalAmount", -1 } } } }));

        await Meerkat.EnsureIndexesOnCollectionAsync(typeof(IndexedEntity), _mockCollection.Object);

        _mockIndexes.Verify(x => x.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<IndexedEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Must().BeTrue();
    }

    [Fact]
    public void EnsureIndexes_WithTypes_ShouldEnsureIndexesForConcreteTypes()
    {
        var mockDb = new Mock<IMongoDatabase>();
        var mockCol = new Mock<IMongoCollection<DeepDerivedSchema>>();
        var mockIdx = new Mock<IMongoIndexManager<DeepDerivedSchema>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.deepderivedschemas"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<DeepDerivedSchema>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());
        mockDb.Setup(x => x.GetCollection<DeepDerivedSchema>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(mockCol.Object);

        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "deep_unique" }, { "key", new BsonDocument { { "DeepCode", 1 } } } }));

        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => mockDb.Object);

        Meerkat.EnsureIndexes(new[] { typeof(DeepDerivedSchema) });

        mockIdx.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<DeepDerivedSchema>>>(), It.IsAny<CancellationToken>()), Times.Once);
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(DeepDerivedSchema).FullName!).Must().BeTrue();
    }

    [Fact]
    public async Task EnsureIndexesAsync_WithTypes_ShouldEnsureIndexesForConcreteTypes()
    {
        var mockDb = new Mock<IMongoDatabase>();
        var mockCol = new Mock<IMongoCollection<DeepDerivedSchema>>();
        var mockIdx = new Mock<IMongoIndexManager<DeepDerivedSchema>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.deepderivedschemas"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<DeepDerivedSchema>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());
        mockDb.Setup(x => x.GetCollection<DeepDerivedSchema>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(mockCol.Object);

        mockIdx.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "deep_unique" }, { "key", new BsonDocument { { "DeepCode", 1 } } } }));
        mockIdx.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateAsyncIndexCursor(
            new BsonDocument { { "name", "deep_unique" }, { "key", new BsonDocument { { "DeepCode", 1 } } } }));

        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => mockDb.Object);

        await Meerkat.EnsureIndexesAsync(new[] { typeof(DeepDerivedSchema) });

        mockIdx.Verify(x => x.CreateManyAsync(It.IsAny<IEnumerable<CreateIndexModel<DeepDerivedSchema>>>(), It.IsAny<CancellationToken>()), Times.Once);
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(DeepDerivedSchema).FullName!).Must().BeTrue();
    }

    [Fact]
    public void EnsureIndexes_WithAssembly_ShouldEnsureIndexesForAssembly()
    {
        var mockDb = new Mock<IMongoDatabase>();
        var mockCol = new Mock<IMongoCollection<IndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(_mockIndexes.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.indexedentities"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<IndexedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());
        mockDb.Setup(x => x.GetCollection<IndexedEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(mockCol.Object);

        _mockIndexes.Setup(x => x.List(It.IsAny<CancellationToken>())).Returns(CreateIndexCursor(
            new BsonDocument { { "name", "unique_name" }, { "key", new BsonDocument { { "Name", 1 } } } },
            new BsonDocument { { "name", "single_age" }, { "key", new BsonDocument { { "Age", -1 } } } },
            new BsonDocument { { "name", "geo_location" }, { "key", new BsonDocument { { "Location", "2dsphere" } } } },
            new BsonDocument { { "name", "compound_idx" }, { "key", new BsonDocument { { "Category", 1 }, { "TotalAmount", -1 } } } }));

        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => mockDb.Object);

        var mockAssembly = new Mock<System.Reflection.Assembly>();
        mockAssembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(IndexedEntity) });

        Meerkat.EnsureIndexes(mockAssembly.Object);
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Must().BeTrue();
    }

    [Fact]
    public async Task EnsureIndexesAsync_WithAssembly_ShouldEnsureIndexesForAssembly()
    {
        var mockDb = new Mock<IMongoDatabase>();
        var mockCol = new Mock<IMongoCollection<IndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(_mockIndexes.Object);
        mockCol.Setup(x => x.CollectionNamespace).Returns(CollectionNamespace.FromFullName("testdb.indexedentities"));
        mockCol.Setup(x => x.DocumentSerializer).Returns(BsonSerializer.LookupSerializer<IndexedEntity>());
        mockCol.Setup(x => x.Settings).Returns(new MongoCollectionSettings());
        mockDb.Setup(x => x.GetCollection<IndexedEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(mockCol.Object);

        _mockIndexes.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(CreateAsyncIndexCursor(
            new BsonDocument { { "name", "unique_name" }, { "key", new BsonDocument { { "Name", 1 } } } },
            new BsonDocument { { "name", "single_age" }, { "key", new BsonDocument { { "Age", -1 } } } },
            new BsonDocument { { "name", "geo_location" }, { "key", new BsonDocument { { "Location", "2dsphere" } } } },
            new BsonDocument { { "name", "compound_idx" }, { "key", new BsonDocument { { "Category", 1 }, { "TotalAmount", -1 } } } }));

        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => mockDb.Object);

        var mockAssembly = new Mock<System.Reflection.Assembly>();
        mockAssembly.Setup(a => a.GetTypes()).Returns(new[] { typeof(IndexedEntity) });

        await Meerkat.EnsureIndexesAsync(mockAssembly.Object);
        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Must().BeTrue();
    }

    [Fact]
    public void GetSchemaTypes_ShouldHandleReflectionTypeLoadException()
    {
        var mockAssembly = new Mock<System.Reflection.Assembly>();
        mockAssembly.Setup(a => a.GetTypes()).Throws(new System.Reflection.ReflectionTypeLoadException(
            new[] { typeof(IndexedEntity), null },
            new Exception[] { new Exception("Cannot load type") }));

        var types = Meerkat.GetSchemaTypes(mockAssembly.Object).ToList();
        types.Must().Contain(typeof(IndexedEntity));
    }

    private static IAsyncCursor<BsonDocument> CreateAsyncIndexCursor(params BsonDocument[] indexes)
    {
        var cursor = new Mock<IAsyncCursor<BsonDocument>>();
        var moved = false;
        cursor.Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                if (moved || indexes.Length == 0)
                    return false;
                moved = true;
                return true;
            });
        cursor.Setup(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (moved || indexes.Length == 0)
                    return false;
                moved = true;
                return true;
            });
        cursor.SetupGet(c => c.Current).Returns(indexes);
        return cursor.Object;
    }

    private static IAsyncCursor<BsonDocument> CreateIndexCursor(params BsonDocument[] indexes)
    {
        var cursor = new Mock<IAsyncCursor<BsonDocument>>();
        var moved = false;
        cursor.Setup(c => c.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                if (moved || indexes.Length == 0)
                    return false;
                moved = true;
                return true;
            });
        cursor.SetupGet(c => c.Current).Returns(indexes);
        return cursor.Object;
    }

    private static BsonDocument RenderKeys<TSchema>(CreateIndexModel<TSchema> model)
    {
        var renderArgs = new RenderArgs<TSchema>(
            BsonSerializer.LookupSerializer<TSchema>(),
            BsonSerializer.SerializerRegistry,
            new PathRenderArgs("", false),
            renderForFind: false,
            renderForElemMatch: false,
            renderDollarForm: false,
            translationOptions: null);
        return model.Keys.Render(renderArgs);
    }
}
