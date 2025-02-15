using MongoDB.Driver;

namespace meerkat.Constants;

internal static class MongoDbConstants
{
    public static readonly CreateIndexOptions UniqueIndexOptions = new() { Unique = true, Background = false };

    public static readonly CreateIndexOptions SingleFieldIndexOptions = new();

    public static readonly ReplaceOptions ReplaceOptions = new() { BypassDocumentValidation = false, IsUpsert = true };

    public static readonly BulkWriteOptions BulkInsertOptions = new()
        { IsOrdered = false, BypassDocumentValidation = false };
}
