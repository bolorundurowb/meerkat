using meerkat.Collections;
using MongoDB.Driver;
using Moq;

namespace meerkat.Tests;

public class MeerkatCrudTests
{
    public class TestEntity : Schema<string>
    {
        public string Name { get; set; }
    }

    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<TestEntity>> _mockCollection;

    public MeerkatCrudTests()
    {
        _mockDb = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<TestEntity>>();
        _mockDb.Setup(x => x.GetCollection<TestEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(_mockCollection.Object);
        _mockDb.Setup(x => x.GetCollection<Schema<string>>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(new Mock<IMongoCollection<Schema<string>>>().Object);
        _mockDb.Setup(x => x.GetCollection<TestEntity>("testentities", It.IsAny<MongoCollectionSettings>()))
               .Returns(_mockCollection.Object);
        _mockCollection.Setup(x => x.Indexes).Returns(new Mock<IMongoIndexManager<TestEntity>>().Object);
        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => _mockDb.Object);
    }

    [Fact]
    public void RemoveById_ShouldCallDeleteOne()
    {
        Meerkat.RemoveById<TestEntity, string>("123");
        _mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveByIdAsync_ShouldCallDeleteOneAsync()
    {
        await Meerkat.RemoveByIdAsync<TestEntity, string>("123");
        _mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RemoveOne_ShouldCallDeleteOne()
    {
        Meerkat.RemoveOne<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Remove_ShouldCallDeleteMany()
    {
        Meerkat.Remove<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteMany(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Count_ShouldCallCountDocuments()
    {
        Meerkat.Count<TestEntity, string>();
        _mockCollection.Verify(x => x.CountDocuments(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_ShouldCallCountDocumentsAsync()
    {
        await Meerkat.CountAsync<TestEntity, string>();
        _mockCollection.Verify(x => x.CountDocumentsAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAsync_ShouldCallReplaceOneAsync()
    {
        var entity = new TestEntity { Id = "123", Name = "Test" };
        var mockCollection = new Mock<IMongoCollection<Schema<string>>>();
        _mockDb.Setup(x => x.GetCollection<Schema<string>>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(mockCollection.Object);
        await entity.SaveAsync();
        mockCollection.Verify(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Schema<string>>>(), entity, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Save_ShouldCallReplaceOne()
    {
        var entity = new TestEntity { Id = "123", Name = "Test" };
        var mockCollection = new Mock<IMongoCollection<Schema<string>>>();
        _mockDb.Setup(x => x.GetCollection<Schema<string>>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(mockCollection.Object);
        entity.Save();
        mockCollection.Verify(x => x.ReplaceOne(It.IsAny<FilterDefinition<Schema<string>>>(), entity, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void SaveAll_ShouldCallBulkWrite()
    {
        var entities = new List<TestEntity> { new TestEntity { Id = "1" }, new TestEntity { Id = "2" } };
        entities.SaveAll<TestEntity, string>();
        _mockCollection.Verify(x => x.BulkWrite(It.IsAny<IEnumerable<WriteModel<TestEntity>>>(), It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAllAsync_ShouldCallBulkWriteAsync()
    {
        var entities = new List<TestEntity> { new TestEntity { Id = "1" }, new TestEntity { Id = "2" } };
        await entities.SaveAllAsync<TestEntity, string>();
        _mockCollection.Verify(x => x.BulkWriteAsync(It.IsAny<IEnumerable<WriteModel<TestEntity>>>(), It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
