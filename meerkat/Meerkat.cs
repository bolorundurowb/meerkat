using System;
using System.Collections.Concurrent;
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
    public static IQueryable<TSchema> Query<TSchema, TId>() where TSchema : Schema<TId> where TId : IEquatable<TId> => GetCollectionForType<TSchema, TId>().AsQueryable();

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
