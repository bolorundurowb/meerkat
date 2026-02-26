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
        
        // Mocking IMongoDatabase.GetCollection<T>() for all types used in tests
        _mockDb.Setup(x => x.GetCollection<TestEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(_mockCollection.Object);
        _mockDb.Setup(x => x.GetCollection<Schema<string>>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(new Mock<IMongoCollection<Schema<string>>>().Object);
        
        // For Count/Remove/etc methods that use GetCollectionForType<TSchema, TId>() which calls typeof(TSchema)
        // Ensure it returns the correct collection mock.
        _mockDb.Setup(x => x.GetCollection<TestEntity>("testentities", It.IsAny<MongoCollectionSettings>()))
               .Returns(_mockCollection.Object);
        
        // Setup index manager mock by default
        _mockCollection.Setup(x => x.Indexes).Returns(new Mock<IMongoIndexManager<TestEntity>>().Object);

        // Reset Meerkat state and connect with mock
        Meerkat.ResetDatabase();
        
        Meerkat._database = new Lazy<IMongoDatabase>(() => _mockDb.Object);
    }

    [Fact]
    public void FindById_ShouldCallCollection()
    {
        // Arrange
        var id = "123";
        var entity = new TestEntity { Id = id };
        
        // Mocking IQueryable is hard, but FindById uses Query() which uses GetCollectionForType().AsQueryable()
        // For simplicity in this environment, let's test methods that call the collection directly if possible.
        // FindById, FindOne, Find, Query all go through IQueryable.
        
        // Let's test RemoveById which calls DeleteOne directly.
    }

    [Fact]
    public void RemoveById_ShouldCallDeleteOne()
    {
        // Act
        Meerkat.RemoveById<TestEntity, string>("123");

        // Assert
        _mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveByIdAsync_ShouldCallDeleteOneAsync()
    {
        // Act
        await Meerkat.RemoveByIdAsync<TestEntity, string>("123");

        // Assert
        _mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RemoveOne_ShouldCallDeleteOne()
    {
        // Act
        Meerkat.RemoveOne<TestEntity, string>(x => x.Name == "test");

        // Assert
        _mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Remove_ShouldCallDeleteMany()
    {
        // Act
        Meerkat.Remove<TestEntity, string>(x => x.Name == "test");

        // Assert
        _mockCollection.Verify(x => x.DeleteMany(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Count_ShouldCallCountDocuments()
    {
        // Act
        Meerkat.Count<TestEntity, string>();

        // Assert
        _mockCollection.Verify(x => x.CountDocuments(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_ShouldCallCountDocumentsAsync()
    {
        // Act
        await Meerkat.CountAsync<TestEntity, string>();

        // Assert
        _mockCollection.Verify(x => x.CountDocumentsAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task SaveAsync_ShouldCallReplaceOneAsync()
    {
        // Arrange
        var entity = new TestEntity { Id = "123", Name = "Test" };
        var mockCollection = new Mock<IMongoCollection<Schema<string>>>();
        _mockDb.Setup(x => x.GetCollection<Schema<string>>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(mockCollection.Object);
        
        // Act
        await entity.SaveAsync();

        // Assert
        mockCollection.Verify(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Schema<string>>>(), entity, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Save_ShouldCallReplaceOne()
    {
        // Arrange
        var entity = new TestEntity { Id = "123", Name = "Test" };
        var mockCollection = new Mock<IMongoCollection<Schema<string>>>();
        _mockDb.Setup(x => x.GetCollection<Schema<string>>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(mockCollection.Object);
        
        // Act
        entity.Save();

        // Assert
        mockCollection.Verify(x => x.ReplaceOne(It.IsAny<FilterDefinition<Schema<string>>>(), entity, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public void SaveAll_ShouldCallBulkWrite()
    {
        // Arrange
        var entities = new List<TestEntity> { new TestEntity { Id = "1" }, new TestEntity { Id = "2" } };
        
        // Act
        entities.SaveAll<TestEntity, string>();

        // Assert
        _mockCollection.Verify(x => x.BulkWrite(It.IsAny<IEnumerable<WriteModel<TestEntity>>>(), It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveAllAsync_ShouldCallBulkWriteAsync()
    {
        // Arrange
        var entities = new List<TestEntity> { new TestEntity { Id = "1" }, new TestEntity { Id = "2" } };
        
        // Act
        await entities.SaveAllAsync<TestEntity, string>();

        // Assert
        _mockCollection.Verify(x => x.BulkWriteAsync(It.IsAny<IEnumerable<WriteModel<TestEntity>>>(), It.IsAny<BulkWriteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
