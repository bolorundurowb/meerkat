using meerkat.Collections;
using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatCollectionsTests
{
    public class TestEntity : Schema<string>
    {
        public string Name { get; set; }
    }

    private readonly Mock<IMongoDatabase> _mockDb;
    private readonly Mock<IMongoCollection<TestEntity>> _mockCollection;

    public MeerkatCollectionsTests()
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

    [Fact]
    public void SaveAll_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        var entities = new List<TestEntity> { new TestEntity { Id = "1" } };
        Action act = () => entities.SaveAll<TestEntity, string>();
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
