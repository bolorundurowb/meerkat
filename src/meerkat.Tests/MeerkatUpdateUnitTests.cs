using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatUpdateUnitTests
{
    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<Counter>> _mockCollection;

    public MeerkatUpdateUnitTests()
    {
        _mockDb = new Mock<IMongoDatabase>();
        _mockCollection = new Mock<IMongoCollection<Counter>>();
        _mockCollection.Setup(x => x.Indexes).Returns(new Mock<IMongoIndexManager<Counter>>().Object);
        _mockDb.Setup(x => x.GetCollection<Counter>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
               .Returns(_mockCollection.Object);
        Meerkat.ResetDatabase();
        Meerkat._database = new Lazy<IMongoDatabase>(() => _mockDb.Object);
    }

    [Fact]
    public void IncrementById_ShouldCallUpdateOne()
    {
        Meerkat.IncrementById<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 5);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementByIdAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.IncrementByIdAsync<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 5);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementById_ShouldCallUpdateOne()
    {
        Meerkat.DecrementById<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 3);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementByIdAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.DecrementByIdAsync<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 3);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementByFilter_ShouldCallUpdateOne()
    {
        var filter = Builders<Counter>.Filter.Eq(x => x.Name, "test");
        Meerkat.IncrementByFilter<Counter, ObjectId, int>(filter, x => x.Value, 7);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementByFilterAsync_ShouldCallUpdateOneAsync()
    {
        var filter = Builders<Counter>.Filter.Eq(x => x.Name, "test");
        await Meerkat.IncrementByFilterAsync<Counter, ObjectId, int>(filter, x => x.Value, 7);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementByFilter_ShouldCallUpdateOne()
    {
        var filter = Builders<Counter>.Filter.Eq(x => x.Name, "test");
        Meerkat.DecrementByFilter<Counter, ObjectId, int>(filter, x => x.Value, 2);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementByFilterAsync_ShouldCallUpdateOneAsync()
    {
        var filter = Builders<Counter>.Filter.Eq(x => x.Name, "test");
        await Meerkat.DecrementByFilterAsync<Counter, ObjectId, int>(filter, x => x.Value, 2);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementOne_ShouldCallUpdateOne()
    {
        Meerkat.IncrementOne<Counter, ObjectId, int>(x => x.Name == "test", x => x.Value, 4);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementOneAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.IncrementOneAsync<Counter, ObjectId, int>(x => x.Name == "test", x => x.Value, 4);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementOne_ShouldCallUpdateOne()
    {
        Meerkat.DecrementOne<Counter, ObjectId, int>(x => x.Name == "test", x => x.Value, 1);
        _mockCollection.Verify(x => x.UpdateOne(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementOneAsync_ShouldCallUpdateOneAsync()
    {
        await Meerkat.DecrementOneAsync<Counter, ObjectId, int>(x => x.Name == "test", x => x.Value, 1);
        _mockCollection.Verify(x => x.UpdateOneAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementMany_ShouldCallUpdateMany()
    {
        Meerkat.IncrementMany<Counter, ObjectId, int>(x => x.Name == "group", x => x.Value, 10);
        _mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementManyAsync_ShouldCallUpdateManyAsync()
    {
        await Meerkat.IncrementManyAsync<Counter, ObjectId, int>(x => x.Name == "group", x => x.Value, 10);
        _mockCollection.Verify(x => x.UpdateManyAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementMany_ShouldCallUpdateMany()
    {
        Meerkat.DecrementMany<Counter, ObjectId, int>(x => x.Name == "group", x => x.Value, 5);
        _mockCollection.Verify(x => x.UpdateMany(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementManyAsync_ShouldCallUpdateManyAsync()
    {
        await Meerkat.DecrementManyAsync<Counter, ObjectId, int>(x => x.Name == "group", x => x.Value, 5);
        _mockCollection.Verify(x => x.UpdateManyAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<UpdateOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementByIdAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.IncrementByIdAndGetUpdated<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 3);
        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<FindOneAndUpdateOptions<Counter>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementByIdAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.IncrementByIdAndGetUpdatedAsync<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 3);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<FindOneAndUpdateOptions<Counter>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementByIdAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.DecrementByIdAndGetUpdated<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 2);
        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<FindOneAndUpdateOptions<Counter>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementByIdAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.DecrementByIdAndGetUpdatedAsync<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value, 2);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<FindOneAndUpdateOptions<Counter>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementOneAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.IncrementOneAndGetUpdated<Counter, ObjectId, int>(x => x.Name == "test", x => x.Value, 5);
        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<FindOneAndUpdateOptions<Counter>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementOneAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.IncrementOneAndGetUpdatedAsync<Counter, ObjectId, int>(x => x.Name == "test", x => x.Value, 5);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<FindOneAndUpdateOptions<Counter>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void DecrementOneAndGetUpdated_ShouldCallFindOneAndUpdate()
    {
        Meerkat.DecrementOneAndGetUpdated<Counter, ObjectId, int>(x => x.Name == "test", x => x.Value, 1);
        _mockCollection.Verify(x => x.FindOneAndUpdate(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<FindOneAndUpdateOptions<Counter>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DecrementOneAndGetUpdatedAsync_ShouldCallFindOneAndUpdateAsync()
    {
        await Meerkat.DecrementOneAndGetUpdatedAsync<Counter, ObjectId, int>(x => x.Name == "test", x => x.Value, 1);
        _mockCollection.Verify(x => x.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<Counter>>(),
            It.IsAny<UpdateDefinition<Counter>>(),
            It.IsAny<FindOneAndUpdateOptions<Counter>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void IncrementById_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.IncrementById<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value);
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void DecrementById_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.DecrementById<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value);
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void IncrementMany_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.IncrementMany<Counter, ObjectId, int>(x => true, x => x.Value);
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void IncrementByIdAndGetUpdated_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => { _ = Meerkat.IncrementByIdAndGetUpdated<Counter, ObjectId, int>(ObjectId.GenerateNewId(), x => x.Value); };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
