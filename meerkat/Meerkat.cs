using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoUrlParser;

namespace meerkat;

/// <summary>
/// Provides database connectivity and querying capabilities for MongoDB using generic schemas.
/// </summary>
public static partial class Meerkat
{
    private static readonly ConcurrentDictionary<string, bool> SchemasWithCheckedIndices = new();
    private static Lazy<IMongoDatabase>? _database;

    /// <summary>
    /// Gets the connected MongoDB database instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the database connection is not initialized.</exception>
    public static IMongoDatabase Database =>
        _database?.Value ?? throw new InvalidOperationException(
            $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

    /// <summary>
    /// Establishes a connection to the MongoDB database.
    /// </summary>
    /// <param name="databaseConnectionString">A fully qualified MongoDB connection string.</param>
    public static void Connect(string databaseConnectionString)
    {
        var (dbUrl, dbName) = Parser.Parse(databaseConnectionString);
        _database = new Lazy<IMongoDatabase>(() => new MongoClient(dbUrl).GetDatabase(dbName));
    }

    /// <summary>
    /// Retrieves a queryable collection of the specified schema type.
    /// </summary>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>An IQueryable instance for querying the collection.</returns>
    public static IQueryable<TSchema> Query<TSchema, TId>() where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().AsQueryable();

    /// <summary>
    /// Finds an entity by its unique identifier.
    /// </summary>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The matching entity or null if not found.</returns>
    public static TSchema? FindById<TSchema, TId>(TId entityId)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().FirstOrDefault(x => x.Id.Equals(entityId));

    /// <summary>
    /// Asynchronously finds an entity by its unique identifier.
    /// </summary>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The matching entity or null if not found.</returns>
    public static Task<TSchema?> FindByIdAsync<TSchema, TId>(TId entityId,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().FirstOrDefaultAsync(x => x.Id.Equals(entityId), cancellationToken);

    /// <summary>
    /// Finds the first entity that matches the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element. If null, selects the first entity in the collection.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The matching entity or null if not found.</returns>
    public static TSchema? FindOne<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().FirstOrDefault(predicate ?? (schema => true));

    /// <summary>
    /// Asynchronously finds the first entity that matches the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element. If null, selects the first entity in the collection.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The matching entity or null if not found.</returns>
    public static Task<TSchema?> FindOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().FirstOrDefaultAsync(predicate ?? (schema => true), cancellationToken);

    /// <summary>
    /// Search for entities by predicate
    /// </summary>
    /// <param name="predicate">A function to test each element. If not defined, returns the entire collection</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The list of matched entities</returns>
    public static List<TSchema> Find<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().Where(predicate ?? (schema => true)).ToList();

    /// <summary>
    /// Search for entities by predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element. If not defined, returns the entire collection</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The list of matched entities</returns>
    public static Task<List<TSchema>> FindAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().Where(predicate ?? (schema => true)).ToListAsync(cancellationToken);

    /// <summary>
    /// Delete an entity by its id
    /// </summary>
    /// <param name="entityId">The entity id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void RemoveById<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOne(x => x.Id.Equals(entityId), cancellationToken);

    /// <summary>
    /// Delete an entity by its id asynchronously
    /// </summary>
    /// <param name="entityId">The entity id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveByIdAsync<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOneAsync(x => x.Id.Equals(entityId), cancellationToken);

    /// <summary>
    /// Delete an entity by a predicate
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void RemoveOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOne(predicate, cancellationToken);

    /// <summary>
    /// Delete an entity by a predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOneAsync(predicate, cancellationToken);

    /// <summary>
    /// Delete an multiple entities by a predicate
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void Remove<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteMany(predicate, cancellationToken);

    /// <summary>
    /// Delete multiple entities by a predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteManyAsync(predicate, cancellationToken);

    /// <summary>
    /// Count number of documents that match predicate
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The number of entries that match the predicate</returns>
    public static long Count<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>()
            .CountDocuments(predicate ?? (schema => true), cancellationToken: cancellationToken);

    /// <summary>
    /// Count number of documents that match predicate asynchronously
    /// </summary>
    /// <param name="predicate">A function to test each element</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The number of entries that match the predicate</returns>
    public static Task<long> CountAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>()
            .CountDocumentsAsync(predicate ?? (schema => true), cancellationToken: cancellationToken);

    /// <summary>
    /// Checks whether any entity matches the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element. If null, checks if any entity exists.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>True if at least one entity matches the predicate; otherwise, false.</returns>
    public static bool Exists<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().Any(predicate ?? (schema => true));

    /// <summary>
    /// Asynchronously checks whether any entity matches the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element. If null, checks if any entity exists.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>True if at least one entity matches the predicate; otherwise, false.</returns>
    public static Task<bool> ExistsAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().AnyAsync(predicate ?? (schema => true), cancellationToken);
}
