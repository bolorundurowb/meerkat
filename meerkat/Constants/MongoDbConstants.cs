using MongoDB.Driver;

namespace meerkat.Constants
{
    internal static class MongoDbConstants
    {
        public static readonly CreateIndexOptions UniqueIndexOptions = new CreateIndexOptions
            { Unique = true, Background = false };

        public static readonly ReplaceOptions ReplaceOptions = new ReplaceOptions
            { BypassDocumentValidation = true, IsUpsert = true };

        public static readonly BulkWriteOptions BulkInsertOptions = new BulkWriteOptions
            { IsOrdered = false, BypassDocumentValidation = true };
    }
}