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
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The number of entries that match the predicate.</returns>
    public static long Count<TSchema, TId>(Expression<Func<TSchema, bool>>? predicate = null,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>()
            .CountDocuments(predicate ?? (schema => true), cancellationToken: cancellationToken);

    /// <summary>
    /// Counts the number of documents that match the given predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <returns>The number of entries that match the predicate.</returns>
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
