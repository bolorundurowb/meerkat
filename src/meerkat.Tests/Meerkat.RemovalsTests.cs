using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatRemovalsTests
{
    public class TestEntity : Schema<string>
    {
        public string Name { get; set; }
    }

    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<TestEntity>> _mockCollection;

    public MeerkatRemovalsTests()
    {
        _mockDb = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<TestEntity>>();
        _mockDb.Setup(x => x.GetCollection<TestEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
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
    public async Task RemoveOneAsync_ShouldCallDeleteOneAsync()
    {
        await Meerkat.RemoveOneAsync<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Remove_ShouldCallDeleteMany()
    {
        Meerkat.Remove<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteMany(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallDeleteManyAsync()
    {
        await Meerkat.RemoveAsync<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteManyAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RemoveById_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.RemoveById<TestEntity, string>("123");
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void RemoveOne_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.RemoveOne<TestEntity, string>(x => x.Name == "test");
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void Remove_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.Remove<TestEntity, string>(x => x.Name == "test");
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
