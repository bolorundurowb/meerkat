using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Attributes.Collection(Name = "partial_update_test_models")]
public class PartialUpdateTestModel : Schema<ObjectId>
{
    public string Name { get; set; }
    public string? Nickname { get; set; }
    public int Value { get; set; }
    public List<string> Tags { get; set; } = new();

    public PartialUpdateTestModel()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Attributes.Collection(Name = "timestamped_partial_update_models", TrackTimestamps = true)]
public class TimestampedPartialUpdateTestModel : Schema<ObjectId>
{
    public string Name { get; set; }

    public TimestampedPartialUpdateTestModel()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatPartialUpdatesTests
{
    private readonly Mock<IMongoCollection<PartialUpdateTestModel>> _mockCollection;
    private readonly Mock<IMongoCollection<TimestampedPartialUpdateTestModel>> _mockTimestampedCollection;

    public MeerkatPartialUpdatesTests()
    {
        var mockDb = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<PartialUpdateTestModel>>();
        _mockCollection.Setup(x => x.Indexes).Returns(new Mock<IMongoIndexManager<PartialUpdateTestModel>>().Object);
        mockDb.Setup(x => x.GetCollection<PartialUpdateTestModel>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockCollection.Object);

        _mockTimestampedCollection = new Mock<IMongoCollection<TimestampedPartialUpdateTestModel>>();
        _mockTimestampedCollection.Setup(x => x.Indexes)
            .Returns(new Mock<IMongoIndexManager<TimestampedPartialUpdateTestModel>>().Object);
        mockDb.Setup(x => x.GetCollection<TimestampedPartialUpdateTestModel>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockTimestampedCollection.Object);

        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => mockDb.Object);
    }

    [Fact]
    public void Update_ShouldCallUpdateOne()
    {
        Meerkat.Update<PartialUpdateTestModel, ObjectId>(ObjectId.GenerateNewId())
            .Set(x => x.Name, "Ada")
            .Execute();

        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.Update<PartialUpdateTestModel, ObjectId>(ObjectId.GenerateNewId())
            .Set(x => x.Name, "Ada")
            .ExecuteAsync();

        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void UpdateOne_ShouldCallUpdateOne()
    {
        Meerkat.UpdateOne<PartialUpdateTestModel, ObjectId>(x => x.Name == "test")
            .Unset(x => x.Nickname)
            .Execute();

        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOneAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.UpdateOne<PartialUpdateTestModel, ObjectId>(x => x.Name == "test")
            .Push(x => x.Tags, "honor")
            .ExecuteAsync();

        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void UpdateMany_ShouldCallUpdateMany()
    {
        Meerkat.UpdateMany<PartialUpdateTestModel, ObjectId>(x => x.Value < 18)
            .Pull(x => x.Tags, "draft")
            .Execute();

        _mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateManyAsync_ShouldCallUpdateManyAsync()
    {
        await Meerkat.UpdateMany<PartialUpdateTestModel, ObjectId>(x => x.Value < 18)
            .AddToSet(x => x.Tags, "alumni")
            .ExecuteAsync();

        _mockCollection.Verify(x => x.UpdateManyAsync(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void UpdateByFilter_ShouldCallUpdateOne()
    {
        var filter = Builders<PartialUpdateTestModel>.Filter.Eq(x => x.Name, "test");
        Meerkat.UpdateByFilter<PartialUpdateTestModel, ObjectId>(filter)
            .Inc(x => x.Value, 1)
            .Execute();

        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void UpdateByFilterMany_ShouldCallUpdateMany()
    {
        var filter = Builders<PartialUpdateTestModel>.Filter.Eq(x => x.Name, "test");
        Meerkat.UpdateByFilter<PartialUpdateTestModel, ObjectId>(filter, many: true)
            .Set(x => x.Name, "updated")
            .Execute();

        _mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void ExecuteAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.Update<PartialUpdateTestModel, ObjectId>(ObjectId.GenerateNewId())
            .Set(x => x.Name, "Ada")
            .ExecuteAndGetUpdated();

        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<PartialUpdateTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.UpdateOne<PartialUpdateTestModel, ObjectId>(x => x.Name == "test")
            .Set(x => x.Name, "Ada")
            .ExecuteAndGetUpdatedAsync();

        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<PartialUpdateTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void CombinedOperations_ShouldCallUpdateOneOnce()
    {
        Meerkat.Update<PartialUpdateTestModel, ObjectId>(ObjectId.GenerateNewId())
            .Set(x => x.Name, "Ada")
            .Unset(x => x.Nickname)
            .Push(x => x.Tags, "honor")
            .Pull(x => x.Tags, "draft")
            .AddToSet(x => x.Tags, "alumni")
            .Inc(x => x.Value, 1)
            .Execute();

        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<PartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void TimestampedSchema_ShouldCallUpdateOne()
    {
        Meerkat.Update<TimestampedPartialUpdateTestModel, ObjectId>(ObjectId.GenerateNewId())
            .Set(x => x.Name, "Ada")
            .Execute();

        _mockTimestampedCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<TimestampedPartialUpdateTestModel>>(),
            It.IsAny<UpdateDefinition<TimestampedPartialUpdateTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Execute_ShouldThrowIfNoOperationsSpecified()
    {
        Action act = () => Meerkat.Update<PartialUpdateTestModel, ObjectId>(ObjectId.GenerateNewId()).Execute();
        act.Throws<InvalidOperationException>()
            .WithMessage(
                "No update operations have been specified. Call at least one of Set, Unset, Push, Pull, AddToSet, or Inc before executing.");
    }

    [Fact]
    public void ExecuteAndGetUpdated_ShouldThrowForUpdateMany()
    {
        Action act = () => Meerkat.UpdateMany<PartialUpdateTestModel, ObjectId>(x => true)
            .Set(x => x.Name, "Ada")
            .ExecuteAndGetUpdated();

        act.Throws<InvalidOperationException>()
            .WithMessage("ExecuteAndGetUpdated cannot be used with UpdateMany. Use Execute or ExecuteAsync instead.");
    }

    [Fact]
    public void Update_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.Update<PartialUpdateTestModel, ObjectId>(ObjectId.GenerateNewId());
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
