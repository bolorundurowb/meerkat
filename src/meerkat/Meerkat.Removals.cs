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
            WithAmbientSession(
                session => collection.UpdateOne(session, filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken),
                () => collection.UpdateOne(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken));
        else
            WithAmbientSession(
                session => collection.DeleteOne(session, x => x.Id.Equals(entityId), cancellationToken: cancellationToken),
                () => collection.DeleteOne(x => x.Id.Equals(entityId), cancellationToken));
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
            return WithAmbientSessionAsync(
                session => collection.UpdateOneAsync(session, filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken),
                () => collection.UpdateOneAsync(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken));

        return WithAmbientSessionAsync(
            session => collection.DeleteOneAsync(session, x => x.Id.Equals(entityId), cancellationToken: cancellationToken),
            () => collection.DeleteOneAsync(x => x.Id.Equals(entityId), cancellationToken));
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
            WithAmbientSession(
                session => collection.UpdateOne(session, filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken),
                () => collection.UpdateOne(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken));
        else
            WithAmbientSession(
                session => collection.DeleteOne(session, predicate, cancellationToken: cancellationToken),
                () => collection.DeleteOne(predicate, cancellationToken));
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
            return WithAmbientSessionAsync(
                session => collection.UpdateOneAsync(session, filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken),
                () => collection.UpdateOneAsync(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken));

        return WithAmbientSessionAsync(
            session => collection.DeleteOneAsync(session, predicate, cancellationToken: cancellationToken),
            () => collection.DeleteOneAsync(predicate, cancellationToken));
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
            WithAmbientSession(
                session => collection.UpdateMany(session, filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken),
                () => collection.UpdateMany(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken));
        else
            WithAmbientSession(
                session => collection.DeleteMany(session, predicate, cancellationToken: cancellationToken),
                () => collection.DeleteMany(predicate, cancellationToken));
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
            return WithAmbientSessionAsync(
                session => collection.UpdateManyAsync(session, filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken),
                () => collection.UpdateManyAsync(filter, BuildSoftDeleteUpdate<TSchema, TId>(),
                    cancellationToken: cancellationToken));

        return WithAmbientSessionAsync(
            session => collection.DeleteManyAsync(session, predicate, cancellationToken: cancellationToken),
            () => collection.DeleteManyAsync(predicate, cancellationToken));
    }

    /// <summary>
    /// Permanently deletes an entity by its unique identifier, including soft-deleted documents.
    /// </summary>
    public static void HardRemoveById<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        WithAmbientSession(
            session => collection.DeleteOne(session, x => x.Id.Equals(entityId), cancellationToken: cancellationToken),
            () => collection.DeleteOne(x => x.Id.Equals(entityId), cancellationToken));
    }

    /// <summary>
    /// Permanently deletes an entity by its unique identifier asynchronously, including soft-deleted documents.
    /// </summary>
    public static Task HardRemoveByIdAsync<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        return WithAmbientSessionAsync(
            session => collection.DeleteOneAsync(session, x => x.Id.Equals(entityId), cancellationToken: cancellationToken),
            () => collection.DeleteOneAsync(x => x.Id.Equals(entityId), cancellationToken));
    }

    /// <summary>
    /// Permanently deletes the first entity that matches the given predicate, including soft-deleted documents.
    /// </summary>
    public static void HardRemoveOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        WithAmbientSession(
            session => collection.DeleteOne(session, predicate, cancellationToken: cancellationToken),
            () => collection.DeleteOne(predicate, cancellationToken));
    }

    /// <summary>
    /// Permanently deletes the first entity that matches the given predicate asynchronously, including soft-deleted documents.
    /// </summary>
    public static Task HardRemoveOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        return WithAmbientSessionAsync(
            session => collection.DeleteOneAsync(session, predicate, cancellationToken: cancellationToken),
            () => collection.DeleteOneAsync(predicate, cancellationToken));
    }

    /// <summary>
    /// Permanently deletes multiple entities that match the given predicate, including soft-deleted documents.
    /// </summary>
    public static void HardRemove<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        WithAmbientSession(
            session => collection.DeleteMany(session, predicate, cancellationToken: cancellationToken),
            () => collection.DeleteMany(predicate, cancellationToken));
    }

    /// <summary>
    /// Permanently deletes multiple entities that match the given predicate asynchronously, including soft-deleted documents.
    /// </summary>
    public static Task HardRemoveAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        return WithAmbientSessionAsync(
            session => collection.DeleteManyAsync(session, predicate, cancellationToken: cancellationToken),
            () => collection.DeleteManyAsync(predicate, cancellationToken));
    }

    /// <summary>
    /// Restores a soft-deleted entity by its unique identifier. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static void RestoreById<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return;

        var collection = GetCollectionForType<TSchema, TId>();
        WithAmbientSession(
            session => collection.UpdateOne(session, x => x.Id.Equals(entityId),
                BuildRestoreUpdate<TSchema, TId>(), cancellationToken: cancellationToken),
            () => collection.UpdateOne(x => x.Id.Equals(entityId),
                BuildRestoreUpdate<TSchema, TId>(), cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Restores a soft-deleted entity by its unique identifier asynchronously. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static Task RestoreByIdAsync<TSchema, TId>(TId entityId, CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return Task.CompletedTask;

        var collection = GetCollectionForType<TSchema, TId>();
        return WithAmbientSessionAsync(
            session => collection.UpdateOneAsync(session, x => x.Id.Equals(entityId),
                BuildRestoreUpdate<TSchema, TId>(), cancellationToken: cancellationToken),
            () => collection.UpdateOneAsync(x => x.Id.Equals(entityId),
                BuildRestoreUpdate<TSchema, TId>(), cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Restores the first matching soft-deleted entity. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static void RestoreOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return;

        var collection = GetCollectionForType<TSchema, TId>();
        WithAmbientSession(
            session => collection.UpdateOne(session, predicate, BuildRestoreUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken),
            () => collection.UpdateOne(predicate, BuildRestoreUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Restores the first matching soft-deleted entity asynchronously. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static Task RestoreOneAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return Task.CompletedTask;

        var collection = GetCollectionForType<TSchema, TId>();
        return WithAmbientSessionAsync(
            session => collection.UpdateOneAsync(session, predicate, BuildRestoreUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken),
            () => collection.UpdateOneAsync(predicate, BuildRestoreUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Restores all matching soft-deleted entities. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static void Restore<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return;

        var collection = GetCollectionForType<TSchema, TId>();
        WithAmbientSession(
            session => collection.UpdateMany(session, predicate, BuildRestoreUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken),
            () => collection.UpdateMany(predicate, BuildRestoreUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Restores all matching soft-deleted entities asynchronously. No-op when the schema does not opt into soft delete.
    /// </summary>
    public static Task RestoreAsync<TSchema, TId>(Expression<Func<TSchema, bool>> predicate,
        CancellationToken cancellationToken = default) where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete())
            return Task.CompletedTask;

        var collection = GetCollectionForType<TSchema, TId>();
        return WithAmbientSessionAsync(
            session => collection.UpdateManyAsync(session, predicate, BuildRestoreUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken),
            () => collection.UpdateManyAsync(predicate, BuildRestoreUpdate<TSchema, TId>(),
                cancellationToken: cancellationToken));
    }
}
