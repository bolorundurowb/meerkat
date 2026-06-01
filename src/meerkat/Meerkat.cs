using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoUrlParser;

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
    /// </summary>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>An IQueryable instance for querying the collection.</returns>
    public static IQueryable<TSchema> Query<TSchema, TId>() where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().AsQueryable();

    // necessary for testing
    internal static void ResetDatabase() => _database = null;
}
