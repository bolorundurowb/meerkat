using meerkat.Attributes;
using meerkat.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class SchemaTests
{
    [Attributes.Collection(TrackTimestamps = true)]
    private class TrackedEntity : Schema<Guid>
    {
        [Lowercase]
        public string Email { get; set; }

        [Uppercase]
        public string Code { get; set; }

        public string Normal { get; set; }
    }

    private class UntrackedEntity : Schema<Guid> { }

    private class InvalidLowercaseEntity : Schema<Guid>
    {
        [Lowercase]
        public int Number { get; set; }
    }

    private class InvalidUppercaseEntity : Schema<Guid>
    {
        [Uppercase]
        public int Number { get; set; }
    }

    public class SaveTestEntity : Schema<string>
    {
        public string Name { get; set; }
    }

    [Attributes.Collection(SoftDelete = true)]
    public class SoftDeleteSaveTestEntity : Schema<string>
    {
        public string Name { get; set; }
    }

    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<Schema<string>>> _mockSchemaCollection;

    public SchemaTests()
    {
        _mockDb = new Mock<IMongoDatabase>();
        _mockSchemaCollection = new Mock<IMongoCollection<Schema<string>>>();

        _mockDb.Setup(x => x.GetCollection<Schema<string>>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(_mockSchemaCollection.Object);
        _mockSchemaCollection.Setup(x => x.Indexes).Returns(new Mock<IMongoIndexManager<Schema<string>>>().Object);

        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => _mockDb.Object);
    }

    [Fact]
    public void HandleTimestamps_ShouldSetCreatedAtAndUpdatedAt_WhenTrackedAndNew()
    {
        var entity = new TrackedEntity();
        entity.HandleTimestamps();
        entity.CreatedAt.Must().NotBeNull();
        entity.UpdatedAt.Must().NotBeNull();
        entity.UpdatedAt.Must().Be(entity.CreatedAt);
    }

    [Fact]
    public void HandleTimestamps_ShouldOnlySetUpdatedAt_WhenTrackedAndExisting()
    {
        var entity = new TrackedEntity();
        entity.HandleTimestamps();
        var originalCreatedAt = entity.CreatedAt;
        entity.HandleTimestamps();
        entity.CreatedAt.Must().Be(originalCreatedAt);
        entity.UpdatedAt.Must().NotBeNull();
    }

    [Fact]
    public void HandleTimestamps_ShouldDoNothing_WhenUntracked()
    {
        var entity = new UntrackedEntity();
        entity.HandleTimestamps();
        entity.CreatedAt.Must().BeNull();
        entity.UpdatedAt.Must().BeNull();
    }

    [Fact]
    public void MarkDeleted_ShouldSetDeletedAtAndIsDeleted()
    {
        var entity = new UntrackedEntity();
        entity.IsDeleted.Must().BeFalse();
        entity.MarkDeleted();
        entity.DeletedAt.Must().NotBeNull();
        entity.IsDeleted.Must().BeTrue();
    }

    [Fact]
    public void MarkRestored_ShouldClearDeletedAt()
    {
        var entity = new UntrackedEntity();
        entity.MarkDeleted();
        entity.MarkRestored();
        entity.DeletedAt.Must().BeNull();
        entity.IsDeleted.Must().BeFalse();
    }

    [Fact]
    public void MarkDeleted_ShouldTouchUpdatedAt_WhenTimestampsTracked()
    {
        var entity = new TrackedEntity();
        entity.HandleTimestamps();
        entity.MarkDeleted();
        entity.UpdatedAt.Must().NotBeNull();
        entity.IsDeleted.Must().BeTrue();
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldLowercaseTargetProperties()
    {
        var entity = new TrackedEntity { Email = "TEST@EXAMPLE.COM", Normal = "STAY_SAME" };
        entity.HandleLowercaseTransformations();
        entity.Email.Must().Be("test@example.com");
        entity.Normal.Must().Be("STAY_SAME");
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldUppercaseTargetProperties()
    {
        var entity = new TrackedEntity { Code = "abc-123", Normal = "stay_same" };
        entity.HandleUppercaseTransformations();
        entity.Code.Must().Be("ABC-123");
        entity.Normal.Must().Be("stay_same");
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldThrow_WhenAppliedToNonString()
    {
        var entity = new InvalidLowercaseEntity { Number = 123 };
        var act = () => entity.HandleLowercaseTransformations();
        act.Throws<InvalidAttributeException>();
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldThrow_WhenAppliedToNonString()
    {
        var entity = new InvalidUppercaseEntity { Number = 123 };
        var act = () => entity.HandleUppercaseTransformations();
        act.Throws<InvalidAttributeException>();
    }

    [Fact]
    public async Task SaveAsync_ShouldCallReplaceOneAsync()
    {
        var entity = new SaveTestEntity { Id = "123", Name = "Test" };
        await entity.SaveAsync();
        _mockSchemaCollection.Verify(x => x.ReplaceOneAsync(It.IsAny<FilterDefinition<Schema<string>>>(), entity, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Save_ShouldCallReplaceOne()
    {
        var entity = new SaveTestEntity { Id = "123", Name = "Test" };
        entity.Save();
        _mockSchemaCollection.Verify(x => x.ReplaceOne(It.IsAny<FilterDefinition<Schema<string>>>(), entity, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Save_ShouldCallReplaceOne_WithSession_WhenAmbientSessionActive()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var entity = new SaveTestEntity { Id = "123", Name = "Test" };
            entity.Save();
            _mockSchemaCollection.Verify(x => x.ReplaceOne(mockSession, It.IsAny<FilterDefinition<Schema<string>>>(), entity, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task SaveAsync_ShouldCallReplaceOneAsync_WithSession_WhenAmbientSessionActive()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var entity = new SaveTestEntity { Id = "123", Name = "Test" };
            await entity.SaveAsync();
            _mockSchemaCollection.Verify(x => x.ReplaceOneAsync(mockSession, It.IsAny<FilterDefinition<Schema<string>>>(), entity, It.IsAny<ReplaceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void Delete_ShouldCallDeleteOne_WhenSoftDeleteDisabled()
    {
        var entity = new SaveTestEntity { Id = "123", Name = "Test" };
        entity.Delete();
        _mockSchemaCollection.Verify(x => x.DeleteOne(It.IsAny<FilterDefinition<Schema<string>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Delete_ShouldCallDeleteOne_WithSession_WhenSoftDeleteDisabledAndAmbientSessionActive()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var entity = new SaveTestEntity { Id = "123", Name = "Test" };
            entity.Delete();
            _mockSchemaCollection.Verify(x => x.DeleteOne(mockSession, It.IsAny<FilterDefinition<Schema<string>>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDeleteOneAsync_WhenSoftDeleteDisabled()
    {
        var entity = new SaveTestEntity { Id = "123", Name = "Test" };
        await entity.DeleteAsync();
        _mockSchemaCollection.Verify(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<Schema<string>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDeleteOneAsync_WithSession_WhenSoftDeleteDisabledAndAmbientSessionActive()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var entity = new SaveTestEntity { Id = "123", Name = "Test" };
            await entity.DeleteAsync();
            _mockSchemaCollection.Verify(x => x.DeleteOneAsync(mockSession, It.IsAny<FilterDefinition<Schema<string>>>(), It.IsAny<DeleteOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void Delete_ShouldCallUpdateOne_WhenSoftDeleteEnabled()
    {
        var entity = new SoftDeleteSaveTestEntity { Id = "123", Name = "Test" };
        entity.Delete();
        _mockSchemaCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Schema<string>>>(),
            It.IsAny<UpdateDefinition<Schema<string>>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Delete_ShouldCallUpdateOne_WithSession_WhenSoftDeleteEnabledAndAmbientSessionActive()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var entity = new SoftDeleteSaveTestEntity { Id = "123", Name = "Test" };
            entity.Delete();
            _mockSchemaCollection.Verify(x => x.UpdateOne(
                mockSession,
                It.IsAny<FilterDefinition<Schema<string>>>(),
                It.IsAny<UpdateDefinition<Schema<string>>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallUpdateOneAsync_WhenSoftDeleteEnabled()
    {
        var entity = new SoftDeleteSaveTestEntity { Id = "123", Name = "Test" };
        await entity.DeleteAsync();
        _mockSchemaCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Schema<string>>>(),
            It.IsAny<UpdateDefinition<Schema<string>>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallUpdateOneAsync_WithSession_WhenSoftDeleteEnabledAndAmbientSessionActive()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var entity = new SoftDeleteSaveTestEntity { Id = "123", Name = "Test" };
            await entity.DeleteAsync();
            _mockSchemaCollection.Verify(x => x.UpdateOneAsync(
                mockSession,
                It.IsAny<FilterDefinition<Schema<string>>>(),
                It.IsAny<UpdateDefinition<Schema<string>>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void Restore_ShouldBeNoOp_WhenSoftDeleteDisabled()
    {
        var entity = new SaveTestEntity { Id = "123", Name = "Test" };
        entity.Restore();
        _mockSchemaCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Schema<string>>>(),
            It.IsAny<UpdateDefinition<Schema<string>>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RestoreAsync_ShouldBeNoOp_WhenSoftDeleteDisabled()
    {
        var entity = new SaveTestEntity { Id = "123", Name = "Test" };
        await entity.RestoreAsync();
        _mockSchemaCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Schema<string>>>(),
            It.IsAny<UpdateDefinition<Schema<string>>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Restore_ShouldCallUpdateOne_WhenSoftDeleteEnabled()
    {
        var entity = new SoftDeleteSaveTestEntity { Id = "123", Name = "Test" };
        entity.Restore();
        _mockSchemaCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Schema<string>>>(),
            It.IsAny<UpdateDefinition<Schema<string>>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Restore_ShouldCallUpdateOne_WithSession_WhenSoftDeleteEnabledAndAmbientSessionActive()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var entity = new SoftDeleteSaveTestEntity { Id = "123", Name = "Test" };
            entity.Restore();
            _mockSchemaCollection.Verify(x => x.UpdateOne(
                mockSession,
                It.IsAny<FilterDefinition<Schema<string>>>(),
                It.IsAny<UpdateDefinition<Schema<string>>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task RestoreAsync_ShouldCallUpdateOneAsync_WhenSoftDeleteEnabled()
    {
        var entity = new SoftDeleteSaveTestEntity { Id = "123", Name = "Test" };
        await entity.RestoreAsync();
        _mockSchemaCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Schema<string>>>(),
            It.IsAny<UpdateDefinition<Schema<string>>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RestoreAsync_ShouldCallUpdateOneAsync_WithSession_WhenSoftDeleteEnabledAndAmbientSessionActive()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var entity = new SoftDeleteSaveTestEntity { Id = "123", Name = "Test" };
            await entity.RestoreAsync();
            _mockSchemaCollection.Verify(x => x.UpdateOneAsync(
                mockSession,
                It.IsAny<FilterDefinition<Schema<string>>>(),
                It.IsAny<UpdateDefinition<Schema<string>>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void DeletedAt_ShouldSerialiseAsBsonDateTime_AndOmitIsDeleted()
    {
        var entity = new SaveTestEntity { Id = "123" };
        entity.MarkDeleted();
        var document = entity.ToBsonDocument();
        document.Contains("DeletedAt").Must().BeTrue();
        document["DeletedAt"].BsonType.Must().Be(BsonType.DateTime);
        document.Contains("IsDeleted").Must().BeFalse();
    }

    [Fact]
    public void DeletedAt_ShouldBeOmitted_WhenNull()
    {
        var entity = new SaveTestEntity { Id = "123" };
        var document = entity.ToBsonDocument();
        document.Contains("DeletedAt").Must().BeFalse();
    }

    public class LifecycleEntity : Schema<string>
    {
        public bool PreSaveCalled { get; private set; }
        public bool PostSaveCalled { get; private set; }

        public override void PreSave()
        {
            base.PreSave();
            PreSaveCalled = true;
        }

        public override void PostSave()
        {
            base.PostSave();
            PostSaveCalled = true;
        }
    }

    [Fact]
    public void Save_ShouldCallPreSaveAndPostSaveHooks()
    {
        var entity = new LifecycleEntity { Id = "lifecycle_1" };
        entity.PreSaveCalled.Must().BeFalse();
        entity.PostSaveCalled.Must().BeFalse();

        entity.Save();

        entity.PreSaveCalled.Must().BeTrue();
        entity.PostSaveCalled.Must().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_ShouldCallPreSaveAndPostSaveHooks()
    {
        var entity = new LifecycleEntity { Id = "lifecycle_2" };
        entity.PreSaveCalled.Must().BeFalse();
        entity.PostSaveCalled.Must().BeFalse();

        await entity.SaveAsync();

        entity.PreSaveCalled.Must().BeTrue();
        entity.PostSaveCalled.Must().BeTrue();
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldHandleNullPropertyValue()
    {
        var entity = new TrackedEntity { Email = null!, Code = "ABC", Normal = "123" };
        entity.HandleLowercaseTransformations();
        entity.Email.Must().BeNull();
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldHandleNullPropertyValue()
    {
        var entity = new TrackedEntity { Code = null!, Email = "abc", Normal = "123" };
        entity.HandleUppercaseTransformations();
        entity.Code.Must().BeNull();
    }
}
