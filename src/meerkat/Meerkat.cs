using System;
using System.Collections.Concurrent;
using System.Linq;
using meerkat.Extensions;
using MongoDB.Driver;
using UriCredentialParser;

namespace meerkat;

/// <summary>
/// Provides database connectivity and collection access capabilities for MongoDB using generic schemas.
/// </summary>
public static partial class Meerkat
{
    internal static readonly ConcurrentDictionary<string, bool> SchemasWithCheckedIndices = new();
    internal static Lazy<IMongoDatabase>? _database;

    /// <summary>
    /// Gets the connected MongoDB database instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the database connection is not initialised.</exception>
    public static IMongoDatabase Database =>
        _database?.Value ?? throw new InvalidOperationException(
            $"The database connection has not been initialised. Call {nameof(Connect)}() before carrying out any operations.");

    /// <summary>
    /// Establishes a connection to the MongoDB database.
    /// </summary>
    /// <param name="databaseConnectionString">A fully qualified MongoDB connection string.</param>
    public static void Connect(string databaseConnectionString)
    {
        var (dbUrl, dbName) = CredentialsParser.Parse(databaseConnectionString).ToMongoConnectionSplit();
        _database = new Lazy<IMongoDatabase>(() => new MongoClient(dbUrl).GetDatabase(dbName));
    }

    /// <summary>
    /// Retrieves the underlying MongoDB collection for the specified schema type.
    /// </summary>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The IMongoCollection instance for the schema.</returns>
    public static IMongoCollection<TSchema> Collection<TSchema, TId>()
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>();

    /// <summary>
    /// Retrieves a queryable collection of the specified schema type.
    /// When the schema opts into soft delete, deleted documents are excluded unless <paramref name="includeDeleted"/> is true.
    /// </summary>
    /// <param name="includeDeleted">When true, includes soft-deleted documents. Ignored when soft delete is not enabled.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>An IQueryable instance for querying the collection.</returns>
    public static IQueryable<TSchema> Query<TSchema, TId>(bool includeDeleted = false)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var query = GetCollectionForType<TSchema, TId>().AsQueryable();
        if (typeof(TSchema).ShouldSoftDelete() && !includeDeleted)
            query = query.Where(x => x.DeletedAt == null);

        return query;
    }

    // necessary for testing
    internal static void ResetDatabase() => _database = null;
}
