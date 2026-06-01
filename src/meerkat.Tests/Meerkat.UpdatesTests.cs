using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatUpdatesTests
{
    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<CounterTestModel>> _mockCollection;

    public MeerkatUpdatesTests()
    {
        _mockDb = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<CounterTestModel>>();
        _mockCollection.Setup(x => x.Indexes).Returns(new Mock<IMongoIndexManager<CounterTestModel>>().Object);
        _mockDb.Setup(x => x.GetCollection<CounterTestModel>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(_mockCollection.Object);
        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => _mockDb.Object);
    }

    [Fact]
    public void IncrementById_ShouldCallUpdateOne()
    {
        Meerkat.IncrementById<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 5);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementByIdAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.IncrementByIdAsync<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 5);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementById_ShouldCallUpdateOne()
    {
        Meerkat.DecrementById<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 3);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementByIdAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.DecrementByIdAsync<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 3);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementByFilter_ShouldCallUpdateOne()
    {
        var filter = Builders<CounterTestModel>.Filter.Eq(x => x.Name, "test");
        Meerkat.IncrementByFilter<CounterTestModel, ObjectId, int>(filter, x => x.Value, 7);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementByFilterAsync_ShouldCallUpdateOneAsync()
    {
        var filter = Builders<CounterTestModel>.Filter.Eq(x => x.Name, "test");
        await Meerkat.IncrementByFilterAsync<CounterTestModel, ObjectId, int>(filter, x => x.Value, 7);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementByFilter_ShouldCallUpdateOne()
    {
        var filter = Builders<CounterTestModel>.Filter.Eq(x => x.Name, "test");
        Meerkat.DecrementByFilter<CounterTestModel, ObjectId, int>(filter, x => x.Value, 2);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementByFilterAsync_ShouldCallUpdateOneAsync()
    {
        var filter = Builders<CounterTestModel>.Filter.Eq(x => x.Name, "test");
        await Meerkat.DecrementByFilterAsync<CounterTestModel, ObjectId, int>(filter, x => x.Value, 2);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementOne_ShouldCallUpdateOne()
    {
        Meerkat.IncrementOne<CounterTestModel, ObjectId, int>(x => x.Name == "test", x => x.Value, 4);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementOneAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.IncrementOneAsync<CounterTestModel, ObjectId, int>(x => x.Name == "test", x => x.Value, 4);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementOne_ShouldCallUpdateOne()
    {
        Meerkat.DecrementOne<CounterTestModel, ObjectId, int>(x => x.Name == "test", x => x.Value, 1);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementOneAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.DecrementOneAsync<CounterTestModel, ObjectId, int>(x => x.Name == "test", x => x.Value, 1);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementMany_ShouldCallUpdateMany()
    {
        Meerkat.IncrementMany<CounterTestModel, ObjectId, int>(x => x.Name == "group", x => x.Value, 10);
        _mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementManyAsync_ShouldCallUpdateManyAsync()
    {
        await Meerkat.IncrementManyAsync<CounterTestModel, ObjectId, int>(x => x.Name == "group", x => x.Value, 10);
        _mockCollection.Verify(x => x.UpdateManyAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementMany_ShouldCallUpdateMany()
    {
        Meerkat.DecrementMany<CounterTestModel, ObjectId, int>(x => x.Name == "group", x => x.Value, 5);
        _mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementManyAsync_ShouldCallUpdateManyAsync()
    {
        await Meerkat.DecrementManyAsync<CounterTestModel, ObjectId, int>(x => x.Name == "group", x => x.Value, 5);
        _mockCollection.Verify(x => x.UpdateManyAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementByIdAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.IncrementByIdAndGetUpdated<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 3);
        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<CounterTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementByIdAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.IncrementByIdAndGetUpdatedAsync<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 3);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<CounterTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementByIdAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.DecrementByIdAndGetUpdated<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 2);
        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<CounterTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementByIdAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.DecrementByIdAndGetUpdatedAsync<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 2);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<CounterTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementOneAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.IncrementOneAndGetUpdated<CounterTestModel, ObjectId, int>(x => x.Name == "test", x => x.Value, 5);
        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<CounterTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementOneAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.IncrementOneAndGetUpdatedAsync<CounterTestModel, ObjectId, int>(x => x.Name == "test", x => x.Value, 5);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<CounterTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementOneAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.DecrementOneAndGetUpdated<CounterTestModel, ObjectId, int>(x => x.Name == "test", x => x.Value, 1);
        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<CounterTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementOneAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.DecrementOneAndGetUpdatedAsync<CounterTestModel, ObjectId, int>(x => x.Name == "test", x => x.Value, 1);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<CounterTestModel>>(),
            It.IsAny<UpdateDefinition<CounterTestModel>>(),
            It.IsAny<FindOneAndUpdateOptions<CounterTestModel>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementById_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.IncrementById<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value);
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void DecrementById_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.DecrementById<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value);
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void IncrementMany_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.IncrementMany<CounterTestModel, ObjectId, int>(x => true, x => x.Value);
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void IncrementByIdAndGetUpdated_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => { _ = Meerkat.IncrementByIdAndGetUpdated<CounterTestModel, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value); };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
