using meerkat.Attributes;
using meerkat.Enums;
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
    public void HandleIndexing_ShouldDoNothing_WhenCollectionIndexesIsNull()
    {
        var mockCol = new Mock<IMongoCollection<IndexedEntity>>();
        mockCol.Setup(x => x.Indexes).Returns((IMongoIndexManager<IndexedEntity>?)null);

        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), mockCol.Object);

        Meerkat.SchemasWithCheckedIndices.ContainsKey(typeof(IndexedEntity).FullName!).Verify().ToBeTrue();
    }
}
