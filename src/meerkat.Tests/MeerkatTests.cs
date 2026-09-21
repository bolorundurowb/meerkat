using MongoDB.Bson;
using OmniAssert;

namespace meerkat.Tests;

[CollectionDefinition("MeerkatTests", DisableParallelization = true)]
public class MeerkatTestsCollection;

[CollectionDefinition("MeerkatUnitTests", DisableParallelization = true)]
public class MeerkatUnitTestsCollection;

[Xunit.Collection("MeerkatTests")]
public class MeerkatTests
{
    [Fact]
    public void Connect_ShouldInitializeDatabase()
    {
        Meerkat.ResetDatabase();
        var connectionString = "mongodb://localhost:27017/testdb";
        Meerkat.Connect(connectionString);
        Meerkat.Database.Must().NotBeNull();
    }

    [Fact]
    public void Database_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        var act = () => { _ = Meerkat.Database; };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialised. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void Client_ShouldInitializeMongoClient()
    {
        Meerkat.ResetDatabase();
        var connectionString = "mongodb://localhost:27017/testdb";
        Meerkat.Connect(connectionString);
        Meerkat.Client.Must().NotBeNull();
    }

    [Fact]
    public void Client_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        var act = () => { _ = Meerkat.Client; };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialised. Call Connect() before carrying out any operations.");
    }

    [Attributes.Collection(Name = "test_students")]
    public class TestStudent : Schema<Guid>
    {
        public string FirstName { get; set; } = string.Empty;
    }

    public class TestProduct : Schema<ObjectId>
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Collection_ShouldReturnIMongoCollection_WithCustomCollectionName()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");
        var collection = Meerkat.Collection<TestStudent, Guid>();
        collection.Must().NotBeNull();
        collection.CollectionNamespace.CollectionName.Must().Be("test_students");
    }

    [Fact]
    public void Collection_ShouldReturnIMongoCollection_WithDefaultCollectionName()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect("mongodb://localhost:27017/testdb");
        var collection = Meerkat.Collection<TestProduct, ObjectId>();
        collection.Must().NotBeNull();
        collection.CollectionNamespace.CollectionName.Must().Be("testproducts");
    }

    [Fact]
    public void Collection_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        var act = () => { _ = Meerkat.Collection<TestStudent, Guid>(); };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialised. Call Connect() before carrying out any operations.");
    }

    [Attributes.Collection(SoftDelete = true)]
    public class SoftDeleteTestItem : Schema<Guid>
    {
        public string Title { get; set; } = string.Empty;
    }

    [Fact]
    public void Query_ShouldReturnQueryable_WhenConnected()
    {
        Meerkat.ResetDatabase();
        var mockDb = new Moq.Mock<MongoDB.Driver.IMongoDatabase>();
        var mockCollection = new Moq.Mock<MongoDB.Driver.IMongoCollection<TestStudent>>();
        mockCollection.Setup(x => x.Indexes).Returns(new Moq.Mock<MongoDB.Driver.IMongoIndexManager<TestStudent>>().Object);
        mockDb.Setup(x => x.GetCollection<TestStudent>(Moq.It.IsAny<string>(), Moq.It.IsAny<MongoDB.Driver.MongoCollectionSettings>()))
            .Returns(mockCollection.Object);
        Meerkat._database = new Lazy<MongoDB.Driver.IMongoDatabase>(() => mockDb.Object);

        var query = Meerkat.Query<TestStudent, Guid>();
        query.Must().NotBeNull();
    }

    [Fact]
    public void Query_WithAmbientSession_ShouldReturnQueryableWithSession()
    {
        Meerkat.ResetDatabase();
        var mockDb = new Moq.Mock<MongoDB.Driver.IMongoDatabase>();
        var mockCollection = new Moq.Mock<MongoDB.Driver.IMongoCollection<TestStudent>>();
        mockCollection.Setup(x => x.Indexes).Returns(new Moq.Mock<MongoDB.Driver.IMongoIndexManager<TestStudent>>().Object);
        mockDb.Setup(x => x.GetCollection<TestStudent>(Moq.It.IsAny<string>(), Moq.It.IsAny<MongoDB.Driver.MongoCollectionSettings>()))
            .Returns(mockCollection.Object);
        Meerkat._database = new Lazy<MongoDB.Driver.IMongoDatabase>(() => mockDb.Object);
        var mockSession = new Moq.Mock<MongoDB.Driver.IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;

        try
        {
            var query = Meerkat.Query<TestStudent, Guid>();
            query.Must().NotBeNull();
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void Query_WhenSoftDeleteEnabled_ShouldReturnQueryable()
    {
        Meerkat.ResetDatabase();
        var mockDb = new Moq.Mock<MongoDB.Driver.IMongoDatabase>();
        var mockCollection = new Moq.Mock<MongoDB.Driver.IMongoCollection<SoftDeleteTestItem>>();
        mockCollection.Setup(x => x.Indexes).Returns(new Moq.Mock<MongoDB.Driver.IMongoIndexManager<SoftDeleteTestItem>>().Object);
        mockDb.Setup(x => x.GetCollection<SoftDeleteTestItem>(Moq.It.IsAny<string>(), Moq.It.IsAny<MongoDB.Driver.MongoCollectionSettings>()))
            .Returns(mockCollection.Object);
        Meerkat._database = new Lazy<MongoDB.Driver.IMongoDatabase>(() => mockDb.Object);

        var query = Meerkat.Query<SoftDeleteTestItem, Guid>(includeDeleted: false);
        query.Must().NotBeNull();

        var queryWithDeleted = Meerkat.Query<SoftDeleteTestItem, Guid>(includeDeleted: true);
        queryWithDeleted.Must().NotBeNull();
    }
}
