using System;
using meerkat.Extensions;
using MongoDB.Driver;

namespace meerkat;

public static partial class Meerkat
{
    internal static IMongoCollection<TSchema> GetCollectionForType<TSchema, TId>(TSchema model)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var type = model.GetType();
        return SharedGetCollection<TSchema, TId>(type);
    }

    internal static IMongoCollection<TSchema> GetCollectionForType<TSchema, TId>()
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var type = typeof(TSchema);
        return SharedGetCollection<TSchema, TId>(type);
    }

    private static IMongoCollection<TSchema> SharedGetCollection<TSchema, TId>(Type type)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (Database == null)
            throw new InvalidOperationException(
                $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

        var collectionName = type.GetCollectionName();
        var collection = Database.GetCollection<TSchema>(collectionName);

        HandleIndexing<TSchema, TId>(type, collection);

        return collection;
    }
}
