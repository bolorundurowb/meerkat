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

    [Fact]
    public async Task RemoveOneAsync_ShouldCallUpdateOneAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.RemoveOneAsync<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HardRemoveById_ShouldCallDeleteOne()
    {
        Meerkat.HardRemoveById<TestEntity, string>("123");
        _mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HardRemoveByIdAsync_ShouldCallDeleteOneAsync()
    {
        await Meerkat.HardRemoveByIdAsync<TestEntity, string>("123");
        _mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HardRemoveOneAsync_ShouldCallDeleteOneAsync()
    {
        await Meerkat.HardRemoveOneAsync<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HardRemoveAsync_ShouldCallDeleteManyAsync()
    {
        await Meerkat.HardRemoveAsync<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.DeleteManyAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HardRemoveOne_ShouldCallDeleteOne_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.HardRemoveOne<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<SoftDeleteEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HardRemoveOneAsync_ShouldCallDeleteOneAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.HardRemoveOneAsync<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<SoftDeleteEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void HardRemove_ShouldCallDeleteMany_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        Meerkat.HardRemove<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.DeleteMany(It.IsAny<FilterDefinition<SoftDeleteEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HardRemoveAsync_ShouldCallDeleteManyAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.HardRemoveAsync<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.DeleteManyAsync(It.IsAny<FilterDefinition<SoftDeleteEntity>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreOneAsync_ShouldCallUpdateOneAsync_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        await Meerkat.RestoreOneAsync<SoftDeleteEntity, string>(x => x.Name == "test");
        mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreByIdAsync_ShouldBeNoOp_WhenSoftDeleteDisabled()
    {
        await Meerkat.RestoreByIdAsync<TestEntity, string>("123");
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<TestEntity>>(),
            It.IsAny<UpdateDefinition<TestEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void RestoreOne_ShouldBeNoOp_WhenSoftDeleteDisabled()
    {
        Meerkat.RestoreOne<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<TestEntity>>(),
            It.IsAny<UpdateDefinition<TestEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RestoreOneAsync_ShouldBeNoOp_WhenSoftDeleteDisabled()
    {
        await Meerkat.RestoreOneAsync<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<TestEntity>>(),
            It.IsAny<UpdateDefinition<TestEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Restore_ShouldBeNoOp_WhenSoftDeleteDisabled()
    {
        Meerkat.Restore<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<TestEntity>>(),
            It.IsAny<UpdateDefinition<TestEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_ShouldBeNoOp_WhenSoftDeleteDisabled()
    {
        await Meerkat.RestoreAsync<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.UpdateManyAsync(
            It.IsAny<FilterDefinition<TestEntity>>(),
            It.IsAny<UpdateDefinition<TestEntity>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void RemoveById_WithAmbientSession_ShouldCallDeleteOneWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.RemoveById<TestEntity, string>("123");
            _mockCollection.Verify(x => x.DeleteOne(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RemoveByIdAsync_WithAmbientSession_ShouldCallDeleteOneAsyncWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RemoveByIdAsync<TestEntity, string>("123");
            _mockCollection.Verify(x => x.DeleteOneAsync(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void RemoveOne_WithAmbientSession_ShouldCallDeleteOneWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.RemoveOne<TestEntity, string>(x => x.Name == "test");
            _mockCollection.Verify(x => x.DeleteOne(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RemoveOneAsync_WithAmbientSession_ShouldCallDeleteOneAsyncWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RemoveOneAsync<TestEntity, string>(x => x.Name == "test");
            _mockCollection.Verify(x => x.DeleteOneAsync(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void Remove_WithAmbientSession_ShouldCallDeleteManyWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.Remove<TestEntity, string>(x => x.Name == "test");
            _mockCollection.Verify(x => x.DeleteMany(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RemoveAsync_WithAmbientSession_ShouldCallDeleteManyAsyncWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RemoveAsync<TestEntity, string>(x => x.Name == "test");
            _mockCollection.Verify(x => x.DeleteManyAsync(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void HardRemoveById_WithAmbientSession_ShouldCallDeleteOneWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.HardRemoveById<TestEntity, string>("123");
            _mockCollection.Verify(x => x.DeleteOne(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task HardRemoveByIdAsync_WithAmbientSession_ShouldCallDeleteOneAsyncWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.HardRemoveByIdAsync<TestEntity, string>("123");
            _mockCollection.Verify(x => x.DeleteOneAsync(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void HardRemoveOne_WithAmbientSession_ShouldCallDeleteOneWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.HardRemoveOne<TestEntity, string>(x => x.Name == "test");
            _mockCollection.Verify(x => x.DeleteOne(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task HardRemoveOneAsync_WithAmbientSession_ShouldCallDeleteOneAsyncWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.HardRemoveOneAsync<TestEntity, string>(x => x.Name == "test");
            _mockCollection.Verify(x => x.DeleteOneAsync(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void HardRemove_WithAmbientSession_ShouldCallDeleteManyWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.HardRemove<TestEntity, string>(x => x.Name == "test");
            _mockCollection.Verify(x => x.DeleteMany(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task HardRemoveAsync_WithAmbientSession_ShouldCallDeleteManyAsyncWithSession()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.HardRemoveAsync<TestEntity, string>(x => x.Name == "test");
            _mockCollection.Verify(x => x.DeleteManyAsync(mockSession, It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void RemoveById_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.RemoveById<SoftDeleteEntity, string>("123");
            mockCollection.Verify(x => x.UpdateOne(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RemoveByIdAsync_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RemoveByIdAsync<SoftDeleteEntity, string>("123");
            mockCollection.Verify(x => x.UpdateOneAsync(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void RemoveOne_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.RemoveOne<SoftDeleteEntity, string>(x => x.Name == "test");
            mockCollection.Verify(x => x.UpdateOne(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RemoveOneAsync_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RemoveOneAsync<SoftDeleteEntity, string>(x => x.Name == "test");
            mockCollection.Verify(x => x.UpdateOneAsync(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void Remove_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.Remove<SoftDeleteEntity, string>(x => x.Name == "test");
            mockCollection.Verify(x => x.UpdateMany(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RemoveAsync_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RemoveAsync<SoftDeleteEntity, string>(x => x.Name == "test");
            mockCollection.Verify(x => x.UpdateManyAsync(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void RestoreById_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.RestoreById<SoftDeleteEntity, string>("123");
            mockCollection.Verify(x => x.UpdateOne(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RestoreByIdAsync_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RestoreByIdAsync<SoftDeleteEntity, string>("123");
            mockCollection.Verify(x => x.UpdateOneAsync(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void RestoreOne_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.RestoreOne<SoftDeleteEntity, string>(x => x.Name == "test");
            mockCollection.Verify(x => x.UpdateOne(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RestoreOneAsync_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RestoreOneAsync<SoftDeleteEntity, string>(x => x.Name == "test");
            mockCollection.Verify(x => x.UpdateOneAsync(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void Restore_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Meerkat.Restore<SoftDeleteEntity, string>(x => x.Name == "test");
            mockCollection.Verify(x => x.UpdateMany(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RestoreAsync_WithAmbientSession_WhenSoftDeleteEnabled()
    {
        SetupSoftDeleteCollection(out var mockCollection);
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Meerkat.RestoreAsync<SoftDeleteEntity, string>(x => x.Name == "test");
            mockCollection.Verify(x => x.UpdateManyAsync(
                mockSession,
                It.IsAny<FilterDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateDefinition<SoftDeleteEntity>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }
}
