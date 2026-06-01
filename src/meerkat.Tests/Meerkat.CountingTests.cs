using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatCountingTests
{
    public class TestEntity : Schema<string>
    {
        public string Name { get; set; }
    }

    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<TestEntity>> _mockCollection;

    public MeerkatCountingTests()
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
    public void Count_ShouldCallCountDocuments()
    {
        Meerkat.Count<TestEntity, string>();
        _mockCollection.Verify(x => x.CountDocuments(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Count_WithPredicate_ShouldCallCountDocuments()
    {
        Meerkat.Count<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.CountDocuments(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_ShouldCallCountDocumentsAsync()
    {
        await Meerkat.CountAsync<TestEntity, string>();
        _mockCollection.Verify(x => x.CountDocumentsAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldCallCountDocumentsAsync()
    {
        await Meerkat.CountAsync<TestEntity, string>(x => x.Name == "test");
        _mockCollection.Verify(x => x.CountDocumentsAsync(It.IsAny<FilterDefinition<TestEntity>>(), It.IsAny<CountOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Count_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.Count<TestEntity, string>();
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void Exists_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.Exists<TestEntity, string>();
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void ExistsAsync_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => { _ = Meerkat.ExistsAsync<TestEntity, string>(); };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
