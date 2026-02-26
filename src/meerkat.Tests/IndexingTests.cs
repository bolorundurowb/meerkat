using meerkat.Attributes;
using meerkat.Enums;
using MongoDB.Driver;
using Moq;

namespace meerkat.Tests;

public class IndexingTests
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
        public DateTime CreatedAt { get; set; }
    }

    private readonly Mock<IMongoCollection<IndexedEntity>> _mockCollection;
    private readonly Mock<IMongoIndexManager<IndexedEntity>> _mockIndexes;

    public IndexingTests()
    {
        _mockCollection = new Mock<IMongoCollection<IndexedEntity>>();
        _mockIndexes = new Mock<IMongoIndexManager<IndexedEntity>>();
        _mockCollection.Setup(x => x.Indexes).Returns(_mockIndexes.Object);
        
        // Clear cached indices check for each test to ensure HandleIndexing runs
        Meerkat.SchemasWithCheckedIndices.Clear();
    }

    [Fact]
    public void HandleIndexing_ShouldCreateCorrectIndices()
    {
        // Act
        Meerkat.HandleIndexing<IndexedEntity, Guid>(typeof(IndexedEntity), _mockCollection.Object);

        // Assert
        // Verify Unique Index
        _mockIndexes.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<IndexedEntity>>>(models => 
                models.Any(m => m.Options.Name == "unique_name" && m.Options.Unique == true && m.Options.Sparse == true)),
            It.IsAny<System.Threading.CancellationToken>()), Times.AtLeastOnce);

        // Verify Single Field Index
        _mockIndexes.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<IndexedEntity>>>(models => 
                models.Any(m => m.Options.Name == "single_age")),
            It.IsAny<System.Threading.CancellationToken>()), Times.AtLeastOnce);

        // Verify Geospatial Index
        _mockIndexes.Verify(x => x.CreateMany(
            It.Is<IEnumerable<CreateIndexModel<IndexedEntity>>>(models => 
                models.Any(m => m.Options.Name == "geo_location")),
            It.IsAny<System.Threading.CancellationToken>()), Times.AtLeastOnce);

        // Verify Compound Index
        _mockIndexes.Verify(x => x.CreateOne(
            It.Is<CreateIndexModel<IndexedEntity>>(m => m.Options.Name == "compound_idx"),
            null,
            It.IsAny<System.Threading.CancellationToken>()), Times.Once);
    }
}
