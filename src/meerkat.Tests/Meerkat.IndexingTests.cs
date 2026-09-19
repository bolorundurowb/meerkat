using meerkat.Attributes;
using meerkat.Enums;
using meerkat.Exceptions;
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
        public new DateTime CreatedAt { get; set; }
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
        Meerkat.SchemasWithCheckedIndices.Clear();
    }

    [Fact]
    public void HandleIndexing_ShouldCreateCorrectIndices()
    {
        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), _mockCollection.Object);

        _mockIndexes.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<IndexedEntity>>>(models =>
                models.Any(m => m.Options.Name == "unique_name" && m.Options.Unique == true && m.Options.Sparse == true)),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        _mockIndexes.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<IndexedEntity>>>(models =>
                models.Any(m => m.Options.Name == "single_age")),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        _mockIndexes.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<IndexedEntity>>>(models =>
                models.Any(m => m.Options.Name == "geo_location")),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);

        _mockIndexes.Verify(x => x.CreateOne(
            It.Is<CreateIndexModel<IndexedEntity>>(m => m.Options.Name == "compound_idx"),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleIndexing_ShouldOnlyRunOnce_PerType()
    {
        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), _mockCollection.Object);
        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), _mockCollection.Object);

        _mockIndexes.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<IndexedEntity>>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        _mockIndexes.Verify(x => x.CreateOne(It.IsAny<CreateIndexModel<IndexedEntity>>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleUniqueIndexing_ShouldDoNothing_WhenNoAttributes()
    {
        var mockCol = new Mock<IMongoCollection<NoIndexEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<NoIndexEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleUniqueIndexing(typeof(NoIndexEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<NoIndexEntity>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void HandleSingleFieldIndexing_WithAscendingOrder_ShouldCreateIndex()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldAscEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldAscEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleSingleFieldIndexing(typeof(SingleFieldAscEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<SingleFieldAscEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleSingleFieldIndexing_WithHashedOrder_ShouldCreateIndex()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldHashedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldHashedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleSingleFieldIndexing(typeof(SingleFieldHashedEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<SingleFieldHashedEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleGeospatialFieldIndexing_WithTwoDType_ShouldCreateIndex()
    {
        var mockCol = new Mock<IMongoCollection<Geospatial2DEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<Geospatial2DEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleGeospatialFieldIndexing(typeof(Geospatial2DEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateMany(It.IsAny<IEnumerable<CreateIndexModel<Geospatial2DEntity>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleCompoundFieldIndexing_ShouldDoNothing_WhenNoAttributes()
    {
        var mockCol = new Mock<IMongoCollection<NoIndexEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<NoIndexEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleCompoundFieldIndexing(typeof(NoIndexEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateOne(It.IsAny<CreateIndexModel<NoIndexEntity>>(), null, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void HandleCompoundFieldIndexing_WithUniqueOption_ShouldSetUniqueFlag()
    {
        var mockCol = new Mock<IMongoCollection<CompoundUniqueIndexEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<CompoundUniqueIndexEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleCompoundFieldIndexing(typeof(CompoundUniqueIndexEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateOne(
            It.Is<CreateIndexModel<CompoundUniqueIndexEntity>>(m =>
                m.Options.Name == "ux" && m.Options.Unique == true),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleCompoundFieldIndexing_WithNonUniqueOption_ShouldSetUniqueFalse()
    {
        var mockCol = new Mock<IMongoCollection<CompoundNonUniqueIndexEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<CompoundNonUniqueIndexEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleCompoundFieldIndexing(typeof(CompoundNonUniqueIndexEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateOne(
            It.Is<CreateIndexModel<CompoundNonUniqueIndexEntity>>(m =>
                m.Options.Name == "non_ux" && m.Options.Unique == false),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleCompoundFieldIndexing_WithMixedUniqueOptions_ShouldThrowInvalidAttributeException()
    {
        var mockCol = new Mock<IMongoCollection<CompoundMixedUniqueIndexEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<CompoundMixedUniqueIndexEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Action act = () => Meerkat.HandleCompoundFieldIndexing(typeof(CompoundMixedUniqueIndexEntity), mockCol.Object);

        act.Throws<InvalidAttributeException>()
            .WithMessage("Members of a compound index group must agree on the 'Unique' value.");
    }

    [Fact]
    public void HandleSingleFieldIndexing_WithExpireAfter_ShouldSetExpireAfterOption()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldTtlEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldTtlEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleSingleFieldIndexing(typeof(SingleFieldTtlEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<SingleFieldTtlEntity>>>(models =>
                models.Any(m => m.Options.Name == "ttl_idx" && m.Options.ExpireAfter == TimeSpan.FromDays(30))),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleSingleFieldIndexing_WithNullableDateTimeAndExpireAfter_ShouldSetExpireAfterOption()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldTtlNullableDateTimeEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldTtlNullableDateTimeEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleSingleFieldIndexing(typeof(SingleFieldTtlNullableDateTimeEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<SingleFieldTtlNullableDateTimeEntity>>>(models =>
                models.Any(m => m.Options.Name == "ttl_null_idx" && m.Options.ExpireAfter == TimeSpan.FromHours(12))),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleSingleFieldIndexing_WithExpireAfterOnInvalidType_ShouldThrowInvalidAttributeException()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldTtlInvalidTypeEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldTtlInvalidTypeEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Action act = () => Meerkat.HandleSingleFieldIndexing(typeof(SingleFieldTtlInvalidTypeEntity), mockCol.Object);

        act.Throws<InvalidAttributeException>()
            .WithMessage("The 'ExpireAfter' TTL option can only be applied to DateTime or DateTime? fields.");
    }

    [Fact]
    public void HandleSingleFieldIndexing_WithoutExpireAfter_ShouldHaveNullExpireAfter()
    {
        var mockCol = new Mock<IMongoCollection<SingleFieldNoExpireAfterEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SingleFieldNoExpireAfterEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleSingleFieldIndexing(typeof(SingleFieldNoExpireAfterEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<SingleFieldNoExpireAfterEntity>>>(models =>
                models.Any(m => m.Options.ExpireAfter == null)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HandleIndexing_ShouldDoNothing_WhenCollectionIndexesIsNull()
    {
        var mockCol = new Mock<IMongoCollection<IndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns((IMongoIndexManager<IndexedEntity>?)null);

        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), mockCol.Object);

        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Must().BeTrue();
    }

    [Attributes.Collection(SoftDelete = true)]
    public class SoftDeleteIndexedEntity : Schema<Guid>
    {
        public string Name { get; set; }
    }

    [Fact]
    public void HandleIndexing_ShouldCreateDeletedAtIndex_WhenSoftDeleteEnabled()
    {
        var mockCol = new Mock<IMongoCollection<SoftDeleteIndexedEntity>>();
        var mockIdx = new Mock<IMongoIndexManager<SoftDeleteIndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns(mockIdx.Object);

        Meerkat.HandleIndexing<SoftDeleteIndexedEntity, Guid>(typeof(SoftDeleteIndexedEntity), mockCol.Object);

        mockIdx.Verify(x => x.CreateOne(
            It.Is<CreateIndexModel<SoftDeleteIndexedEntity>>(m => m.Options.Name == "deleted_at_idx"),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
