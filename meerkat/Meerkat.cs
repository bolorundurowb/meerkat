using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Attributes;
using meerkat.Constants;
using meerkat.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoUrlParser;

namespace meerkat;

public static class Meerkat
{
    private static readonly ConcurrentDictionary<string, bool> SchemasWithCheckedIndices = new();
    private static Lazy<IMongoDatabase>? _database;

    /// <summary>
    /// The database that we have connected to
    /// </summary>
    public static IMongoDatabase Database =>
        _database?.Value ?? throw new InvalidOperationException(
            $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

    /// <summary>
    /// Connect to the database
    /// </summary>
    /// <param name="databaseConnectionString">A full mongodb connection string</param>
    public static void Connect(string databaseConnectionString)
    {
        var (dbUrl, dbName) = Parser.Parse(databaseConnectionString);

        _database = new Lazy<IMongoDatabase>(() =>
        {
            var dbClient = new MongoClient(dbUrl);
            return dbClient.GetDatabase(dbName);
        });
    }

    /// <summary>
    /// Get a raw queryable for a given type
    /// </summary>
    /// <typeparam name="TSchema">Type to be queried</typeparam>
    /// <returns>The queryable</returns>
    public static IQueryable<TSchema> Query<TSchema, TId>() where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        return collection
            .AsQueryable();
    }

    /// <summary>
    /// Search for an entity by id
    /// </summary>
    /// <param name="entityId">The entity id</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The found entity or null if not found</returns>
    public static TSchema? FindById<TSchema, TId>(TId entityId)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>()
            .FirstOrDefault(x => x.Id.Equals(entityId));

    /// <summary>
    /// Search for an entity by id asynchronously
    /// </summary>
    /// <param name="entityId">The entity id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The found entity or null if not found</returns>
    public static Task<TSchema?> FindByIdAsync<TSchema, TId>(TId entityId,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().FirstOrDefaultAsync(x => x.Id.Equals(entityId), cancellationToken);

    /// <summary>
    /// Search for an entity by a predicate
    /// </summary>
    /// <param name="predicate">A function to test each element. If not defined, selects the first collection entity</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The found entity or null if not found</returns>
    public static TSchema? FindOne<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        predicate ??= schema => true;
        return Query<TSchema, TId>().FirstOrDefault(predicate);
    }

    /// <summary>
    /// Search for an entity by a predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element. If not defined, selects the first collection entity</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The found entity or null if not found</returns>
    public static Task<TSchema?> FindOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        predicate ??= schema => true;
        return Query<TSchema, TId>()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Search for entities by predicate
    /// </summary>
    /// <param name="predicate">A function to test each element. If not defined, returns the entire collection</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The list of matched entities</returns>
    public static List<TSchema> Find<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        predicate ??= schema => true;
        return Query<TSchema, TId>()
            .Where(predicate)
            .ToList();
    }

    /// <summary>
    /// Search for entities by predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element. If not defined, returns the entire collection</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The list of matched entities</returns>
    public static Task<List<TSchema>> FindAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        predicate ??= schema => true;
        return Query<TSchema, TId>()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Delete an entity by its id
    /// </summary>
    /// <param name="entityId">The entity id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    public static void RemoveById<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        collection.DeleteOne(x => x.Id.Equals(entityId), cancellationToken);
    }

    /// <summary>
    /// Delete an entity by its id asynchronously
    /// </summary>
    /// <param name="entityId">The entity id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    public static Task RemoveByIdAsync<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        return collection.DeleteOneAsync(x => x.Id.Equals(entityId), cancellationToken);
    }

    /// <summary>
    /// Delete an entity by a predicate
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    public static void RemoveOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        collection.DeleteOne(predicate, cancellationToken);
    }

    /// <summary>
    /// Delete an entity by a predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    public static Task RemoveOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        return collection.DeleteOneAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Delete an multiple entities by a predicate
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    public static void Remove<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        collection.DeleteMany(predicate, cancellationToken);
    }

    /// <summary>
    /// Delete multiple entities by a predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    public static Task RemoveAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        return collection.DeleteManyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Count number of documents that match predicate
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The number of entries that match the predicate</returns>
    public static long Count<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        predicate ??= schema => true;
        var collection = GetCollectionForType<TSchema, TId>();
        return collection.CountDocuments(predicate, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Count number of documents that match predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The number of entries that match the predicate</returns>
    public static Task<long> CountAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        predicate ??= schema => true;
        var collection = GetCollectionForType<TSchema, TId>();
        return collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Count number of documents that match predicate
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The number of entries that match the predicate</returns>
    public static bool Exists<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        predicate ??= schema => true;
        return Query<TSchema, TId>().Any(predicate);
    }

    /// <summary>
    /// Count number of documents that match predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The type of entity</typeparam>
    /// <returns>The number of entries that match the predicate</returns>
    public static Task<bool> ExistsAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        predicate ??= schema => true;
        return Query<TSchema, TId>().AnyAsync(predicate, cancellationToken);
    }

    internal static IMongoCollection<TSchema> GetCollectionForType<TSchema, TId>(TSchema model)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (Database == null)
            throw new InvalidOperationException(
                $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

        var type = model.GetType();
        var collectionName = type.GetCollectionName();
        var collection = Database.GetCollection<TSchema>(collectionName);

        HandleUniqueIndexing<TSchema, TId>(type, collection);

        return collection;
    }

    internal static IMongoCollection<TSchema> GetCollectionForType<TSchema, TId>()
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (Database == null)
            throw new InvalidOperationException(
                $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

        var type = typeof(TSchema);
        var collectionName = type.GetCollectionName();
        var collection = Database.GetCollection<TSchema>(collectionName);

        HandleUniqueIndexing<TSchema, TId>(type, collection);

        return collection;
    }

    private static void HandleUniqueIndexing<TSchema, TId>(Type type, IMongoCollection<TSchema> collection)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var typeName = type.FullName;

        if (SchemasWithCheckedIndices.ContainsKey(typeName))
            return;

        var properties = type.AttributedWith<UniqueIndexAttribute>();
        var indices = properties
            .Select(x =>
            {
                var field = new StringFieldDefinition<TSchema>(x.Name);
                var definition = new IndexKeysDefinitionBuilder<TSchema>().Ascending(field);
                return new CreateIndexModel<TSchema>(definition, MongoDbConstants.UniqueIndexOptions);
            })
            .ToList();

        if (indices.Any())
            collection.Indexes.CreateMany(indices);

        SchemasWithCheckedIndices[typeName] = true;
    }
}
