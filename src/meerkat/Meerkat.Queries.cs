using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace meerkat;

/// <summary>
/// Provides find/query operations for MongoDB using generic schemas.
/// </summary>
public static partial class Meerkat
{
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
    /// Searches for entities by a predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element. If not defined, returns the entire collection.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>A list of matched entities.</returns>
    public static List<TSchema> Find<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().Where(predicate ?? (schema => true)).ToList();

    /// <summary>
    /// Searches for entities by a predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element. If not defined, returns the entire collection.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>A list of matched entities.</returns>
    public static Task<List<TSchema>> FindAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>().Where(predicate ?? (schema => true)).ToListAsync(cancellationToken);
}
