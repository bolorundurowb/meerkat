using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace meerkat;

/// <summary>
/// Provides delete/remove operations for MongoDB using generic schemas.
/// </summary>
public static partial class Meerkat
{
    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void RemoveById<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOne(x => x.Id.Equals(entityId), cancellationToken);

    /// <summary>
    /// Deletes an entity by its unique identifier asynchronously.
    /// </summary>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveByIdAsync<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOneAsync(x => x.Id.Equals(entityId), cancellationToken);

    /// <summary>
    /// Deletes the first entity that matches the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void RemoveOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOne(predicate, cancellationToken);

    /// <summary>
    /// Deletes the first entity that matches the given predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOneAsync(predicate, cancellationToken);

    /// <summary>
    /// Deletes multiple entities that match the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void Remove<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteMany(predicate, cancellationToken);

    /// <summary>
    /// Deletes multiple entities that match the given predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteManyAsync(predicate, cancellationToken);
}
