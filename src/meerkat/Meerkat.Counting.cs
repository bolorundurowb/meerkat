using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace meerkat;

/// <summary>
/// Provides count and existence check operations for MongoDB using generic schemas.
/// </summary>
public static partial class Meerkat
{
    /// <summary>
    /// Counts the number of documents that match the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <param name="includeDeleted">When true, includes soft-deleted documents.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The number of entries that match the predicate.</returns>
    public static long Count<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default, bool includeDeleted = false)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        FilterDefinition<TSchema> filter = predicate ?? FilterDefinition<TSchema>.Empty;
        return GetCollectionForType<TSchema, TId>()
            .CountDocuments(ApplySoftDeleteFilter<TSchema, TId>(filter, includeDeleted),
                cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Counts the number of documents that match the given predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="includeDeleted">When true, includes soft-deleted documents.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The number of entries that match the predicate.</returns>
    public static Task<long> CountAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default, bool includeDeleted = false)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        FilterDefinition<TSchema> filter = predicate ?? FilterDefinition<TSchema>.Empty;
        return GetCollectionForType<TSchema, TId>()
            .CountDocumentsAsync(ApplySoftDeleteFilter<TSchema, TId>(filter, includeDeleted),
                cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Checks whether any entity matches the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element. If null, checks if any entity exists.</param>
    /// <param name="includeDeleted">When true, includes soft-deleted documents.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>True if at least one entity matches the predicate; otherwise, false.</returns>
    public static bool Exists<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        bool includeDeleted = false)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Queryable.Any(Query<TSchema, TId>(includeDeleted), predicate ?? (schema => true));

    /// <summary>
    /// Asynchronously checks whether any entity matches the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element. If null, checks if any entity exists.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <param name="includeDeleted">When true, includes soft-deleted documents.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>True if at least one entity matches the predicate; otherwise, false.</returns>
    public static Task<bool> ExistsAsync<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default, bool includeDeleted = false)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        Query<TSchema, TId>(includeDeleted).AnyAsync(predicate ?? (schema => true), cancellationToken);
}
