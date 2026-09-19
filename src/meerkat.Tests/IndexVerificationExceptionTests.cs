using meerkat.Exceptions;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class IndexVerificationExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetCollectionNameAndFailedIndexes()
    {
        var collectionName = "test_collection";
        var failedIndexes = new[] { "index_1", "index_2" };

        var exception = new IndexVerificationException(collectionName, failedIndexes);

        exception.CollectionName.Must().Be(collectionName);
        exception.FailedIndexes.Must().HaveCount(2);
        exception.FailedIndexes[0].Must().Be("index_1");
        exception.FailedIndexes[1].Must().Be("index_2");
    }

    [Fact]
    public void Constructor_ShouldFormatMessageCorrectly()
    {
        var collectionName = "users";
        var failedIndexes = new[] { "email_1", "username_unique" };

        var exception = new IndexVerificationException(collectionName, failedIndexes);

        exception.Message.Must().Be("Index verification failed for collection 'users': email_1, username_unique");
    }

    [Fact]
    public void Constructor_WithEmptyFailedIndexes_ShouldInitializeEmptyList()
    {
        var collectionName = "empty_failures";
        var failedIndexes = Array.Empty<string>();

        var exception = new IndexVerificationException(collectionName, failedIndexes);

        exception.CollectionName.Must().Be(collectionName);
        exception.FailedIndexes.Must().BeEmpty();
        exception.Message.Must().Be("Index verification failed for collection 'empty_failures': ");
    }
}
