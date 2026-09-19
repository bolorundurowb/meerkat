using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Extensions;
using MongoDB.Driver;

namespace meerkat;

/// <summary>
/// Provides delete/remove operations for MongoDB using generic schemas.
/// </summary>
public static partial class Meerkat
{
    /// <summary>
    /// Deletes an entity by its unique identifier.
    /// When the schema opts into soft delete, sets <c>DeletedAt</c> instead of physically removing the document.
    /// </summary>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void RemoveById<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        var filter = ApplySoftDeleteFilter<TSchema, TId>(Builders<TSchema>.Filter.Where(x => x.Id.Equals(entityId)));
        if (typeof(TSchema).ShouldSoftDelete())
            collection.UpdateOne(filter, BuildSoftDeleteUpdate<TSchema, TId>(), cancellationToken: cancellationToken);
        else
            collection.DeleteOne(x => x.Id.Equals(entityId), cancellationToken);
    }

    /// <summary>
    /// Deletes an entity by its unique identifier asynchronously.
    /// When the schema opts into soft delete, sets <c>DeletedAt</c> instead of physically removing the document.
    /// </summary>
    /// <param name="entityId">The entity's unique identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveByIdAsync<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        var filter = ApplySoftDeleteFilter<TSchema, TId>(Builders<TSchema>.Filter.Where(x => x.Id.Equals(entityId)));
        if (typeof(TSchema).ShouldSoftDelete())
            return collection.UpdateOneAsync(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken);

        return collection.DeleteOneAsync(x => x.Id.Equals(entityId), cancellationToken);
    }

    /// <summary>
    /// Deletes the first entity that matches the given predicate.
    /// When the schema opts into soft delete, sets <c>DeletedAt</c> instead of physically removing the document.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void RemoveOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        var filter = ApplySoftDeleteFilter<TSchema, TId>(predicate);
        if (typeof(TSchema).ShouldSoftDelete())
            collection.UpdateOne(filter, BuildSoftDeleteUpdate<TSchema, TId>(), cancellationToken: cancellationToken);
        else
            collection.DeleteOne(predicate, cancellationToken);
    }

    /// <summary>
    /// Deletes the first entity that matches the given predicate asynchronously.
    /// When the schema opts into soft delete, sets <c>DeletedAt</c> instead of physically removing the document.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        var filter = ApplySoftDeleteFilter<TSchema, TId>(predicate);
        if (typeof(TSchema).ShouldSoftDelete())
            return collection.UpdateOneAsync(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken);

        return collection.DeleteOneAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Deletes multiple entities that match the given predicate.
    /// When the schema opts into soft delete, sets <c>DeletedAt</c> instead of physically removing the document.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void Remove<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        var filter = ApplySoftDeleteFilter<TSchema, TId>(predicate);
        if (typeof(TSchema).ShouldSoftDelete())
            collection.UpdateMany(filter, BuildSoftDeleteUpdate<TSchema, TId>(), cancellationToken: cancellationToken);
        else
            collection.DeleteMany(predicate, cancellationToken);
    }

    /// <summary>
    /// Deletes multiple entities that match the given predicate asynchronously.
    /// When the schema opts into soft delete, sets <c>DeletedAt</c> instead of physically removing the document.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static Task RemoveAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        var filter = ApplySoftDeleteFilter<TSchema, TId>(predicate);
        if (typeof(TSchema).ShouldSoftDelete())
            return collection.UpdateManyAsync(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken);

        return collection.DeleteManyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Permanently deletes an entity by its unique identifier, including soft-deleted documents.
    /// </summary>
    public static void HardRemoveById<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOne(x => x.Id.Equals(entityId), cancellationToken);

    /// <summary>
    /// Permanently deletes an entity by its unique identifier asynchronously, including soft-deleted documents.
    /// </summary>
    public static Task HardRemoveByIdAsync<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOneAsync(x => x.Id.Equals(entityId), cancellationToken);

    /// <summary>
    /// Permanently deletes the first entity that matches the given predicate, including soft-deleted documents.
    /// </summary>
    public static void HardRemoveOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOne(predicate, cancellationToken);

    /// <summary>
    /// Permanently deletes the first entity that matches the given predicate asynchronously, including soft-deleted documents.
    /// </summary>
    public static Task HardRemoveOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteOneAsync(predicate, cancellationToken);

    /// <summary>
    /// Permanently deletes multiple entities that match the given predicate, including soft-deleted documents.
    /// </summary>
    public static void HardRemove<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteMany(predicate, cancellationToken);

    /// <summary>
    /// Permanently deletes multiple entities that match the given predicate asynchronously, including soft-deleted documents.
    /// </summary>
    public static Task HardRemoveAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        GetCollectionForType<TSchema, TId>().DeleteManyAsync(predicate, cancellationToken);

    /// <summary>
    /// Restores a soft-deleted entity by its unique identifier. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static void RestoreById<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return;

        GetCollectionForType<TSchema, TId>().UpdateOne(x => x.Id.Equals(entityId),
            BuildRestoreUpdate<TSchema, TId>(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Restores a soft-deleted entity by its unique identifier asynchronously. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static Task RestoreByIdAsync<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return Task.CompletedTask;

        return GetCollectionForType<TSchema, TId>().UpdateOneAsync(x => x.Id.Equals(entityId),
            BuildRestoreUpdate<TSchema, TId>(), cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Restores the first matching soft-deleted entity. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static void RestoreOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return;

        GetCollectionForType<TSchema, TId>().UpdateOne(predicate, BuildRestoreUpdate<TSchema, TId>(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Restores the first matching soft-deleted entity asynchronously. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static Task RestoreOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return Task.CompletedTask;

        return GetCollectionForType<TSchema, TId>().UpdateOneAsync(predicate, BuildRestoreUpdate<TSchema, TId>(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Restores all matching soft-deleted entities. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static void Restore<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return;

        GetCollectionForType<TSchema, TId>().UpdateMany(predicate, BuildRestoreUpdate<TSchema, TId>(),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Restores all matching soft-deleted entities asynchronously. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static Task RestoreAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return Task.CompletedTask;

        return GetCollectionForType<TSchema, TId>().UpdateManyAsync(predicate, BuildRestoreUpdate<TSchema, TId>(),
            cancellationToken: cancellationToken);
    }
}
