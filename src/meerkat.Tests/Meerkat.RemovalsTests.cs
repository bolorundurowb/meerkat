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
            .WithMessage("The database connection has not been initialised. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void RemoveOne_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.RemoveOne<TestEntity, string>(x => x.Name == "test");
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialised. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void Remove_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.Remove<TestEntity, string>(x => x.Name == "test");
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialised. Call Connect() before carrying out any operations.");
    }

    [Attributes.Collection(SoftDelete = true)]
    public class SoftDeleteEntity : Schema<string>
    {
        public string Name { get; set; }
    }

    private void SetupSoftDeleteCollection(out Mock<IMongoCollection<SoftDeleteEntity>> mockCollection)
    {
        mockCollection = new Mock<IMongoCollection<SoftDeleteEntity>>();
        _mockDb.Setup(x => x.GetCollection<SoftDeleteEntity>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(mockCollection.Object);
        mockCollection.Setup(x => x.Indexes).Returns(new Mock<IMongoIndexManager<SoftDeleteEntity>>().Object);
    }

    [Fact]
    public void RemoveById_ShouldCallUpdateOne_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.RemoveById<SoftDeleteEntity, string>("123");
        mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
        mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<SoftDeleteEntity>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveByIdAsync_ShouldCallUpdateOneAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.RemoveByIdAsync<SoftDeleteEntity, string>("123");
        mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RemoveOne_ShouldCallUpdateOne_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.RemoveOne<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Remove_ShouldCallUpdateMany_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.Remove<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HardRemoveById_ShouldCallDeleteOne_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.HardRemoveById<SoftDeleteEntity, string>("123");
        mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<SoftDeleteEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HardRemoveByIdAsync_ShouldCallDeleteOneAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.HardRemoveByIdAsync<SoftDeleteEntity, string>("123");
        mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<SoftDeleteEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HardRemoveOne_ShouldCallDeleteOne()
    {
        Meerkat.HardRemoveOne<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HardRemove_ShouldCallDeleteMany()
    {
        Meerkat.HardRemove<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteMany(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RestoreById_ShouldCallUpdateOne_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.RestoreById<SoftDeleteEntity, string>("123");
        mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RestoreById_ShouldBeNoOp_WhenSoftDeleteDisabled()
    {
        Meerkat.RestoreById<TestEntity, string>("123");
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<TestEntity>>(),
            It.IsAny<UpdateDefinition<TestEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void RestoreOne_ShouldCallUpdateOne_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.RestoreOne<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Restore_ShouldCallUpdateMany_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.Restore<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreByIdAsync_ShouldCallUpdateOneAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.RestoreByIdAsync<SoftDeleteEntity, string>("123");
        mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallUpdateManyAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.RemoveAsync<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.UpdateManyAsync(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_ShouldCallUpdateManyAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.RestoreAsync<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.UpdateManyAsync(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
