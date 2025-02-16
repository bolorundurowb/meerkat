using MongoDB.Driver;

namespace meerkat.Constants;

internal static class MongoDbConstants
{
    public static readonly ReplaceOptions ReplaceOptions = new() { BypassDocumentValidation = false, IsUpsert = true };
}
