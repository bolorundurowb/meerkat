using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Attributes;
using meerkat.Constants;
using meerkat.Exceptions;
using meerkat.Extensions;
using meerkat.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace meerkat;

/// <summary>
/// Represents a base schema for MongoDB documents with an identifier of type <typeparamref name="TId"/>.
/// </summary>
/// <typeparam name="TId">The type of the document's unique identifier, which must implement <see cref="IEquatable{T}"/>.</typeparam>
public abstract class Schema<TId> where TId : IEquatable<TId>
{
    /// <summary>
    /// Gets the unique identifier of the document.
    /// It is recommended that this value be unique within the collection.
    /// </summary>
    [BsonId]
    public TId Id { get; set; }

    /// <summary>
    /// Gets the timestamp indicating when the document was first persisted to the database.
    /// </summary>
    public DateTime? CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp indicating when the document was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp indicating when the document was soft-deleted.
    /// Null when the document is active. Persisted as a UTC BSON DateTime.
    /// </summary>
    [BsonIgnoreIfNull]
    [BsonSerializer(typeof(UtcNullableDateTimeOffsetSerializer))]
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the document has been soft-deleted.
    /// </summary>
    [BsonIgnore]
    public bool IsDeleted => DeletedAt != null;

    /// <summary>
    /// Saves or updates the current instance in the corresponding MongoDB collection synchronously.
    /// </summary>
    public void Save()
    {
        HandleTimestamps();
        HandleLowercaseTransformations();
        HandleUppercaseTransformations();

        PreSave();

        var collection = Meerkat.GetCollectionForType<Schema<TId>, TId>(this);
        if (collection == null)
            throw new Exception("Collection is null for Schema<TId>");

        collection.ReplaceOne(x => x.Id.Equals(Id), this, MongoDbConstants.ReplaceOptions);
        PostSave();
    }

    /// <summary>
    /// Saves or updates the current instance in the corresponding MongoDB collection asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        HandleTimestamps();
        HandleLowercaseTransformations();
        HandleUppercaseTransformations();

        PreSave();

        var collection = Meerkat.GetCollectionForType<Schema<TId>, TId>(this);
        if (collection == null)
            throw new Exception("Collection is null for Schema<TId> Async");

        await collection.ReplaceOneAsync(x => x.Id.Equals(Id), this, MongoDbConstants.ReplaceOptions,
            cancellationToken);
        PostSave();
    }

    /// <summary>
    /// Soft-deletes this document when the schema opts into soft delete; otherwise physically deletes it.
    /// </summary>
    public void Delete(CancellationToken cancellationToken = default)
    {
        var collection = Meerkat.GetCollectionForType<Schema<TId>, TId>(this);
        if (GetType().ShouldSoftDelete())
        {
            collection.UpdateOne(x => x.Id.Equals(Id),
                Meerkat.BuildSoftDeleteUpdate<Schema<TId>, TId>(GetType()),
                cancellationToken: cancellationToken);
            MarkDeleted();
            return;
        }

        collection.DeleteOne(x => x.Id.Equals(Id), cancellationToken);
    }

    /// <summary>
    /// Soft-deletes this document when the schema opts into soft delete; otherwise physically deletes it.
    /// </summary>
    public async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        var collection = Meerkat.GetCollectionForType<Schema<TId>, TId>(this);
        if (GetType().ShouldSoftDelete())
        {
            await collection.UpdateOneAsync(x => x.Id.Equals(Id),
                Meerkat.BuildSoftDeleteUpdate<Schema<TId>, TId>(GetType()),
                cancellationToken: cancellationToken);
            MarkDeleted();
            return;
        }

        await collection.DeleteOneAsync(x => x.Id.Equals(Id), cancellationToken);
    }

    /// <summary>
    /// Restores this document if the schema opts into soft delete; otherwise a no-op.
    /// </summary>
    public void Restore(CancellationToken cancellationToken = default)
    {
        if (!GetType().ShouldSoftDelete())
            return;

        var collection = Meerkat.GetCollectionForType<Schema<TId>, TId>(this);
        collection.UpdateOne(x => x.Id.Equals(Id),
            Meerkat.BuildRestoreUpdate<Schema<TId>, TId>(GetType()),
            cancellationToken: cancellationToken);
        MarkRestored();
    }

    /// <summary>
    /// Restores this document if the schema opts into soft delete; otherwise a no-op.
    /// </summary>
    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        if (!GetType().ShouldSoftDelete())
            return;

        var collection = Meerkat.GetCollectionForType<Schema<TId>, TId>(this);
        await collection.UpdateOneAsync(x => x.Id.Equals(Id),
            Meerkat.BuildRestoreUpdate<Schema<TId>, TId>(GetType()),
            cancellationToken: cancellationToken);
        MarkRestored();
    }

    /// <summary>
    /// A virtual method that is invoked before the entity is persisted.
    /// Can be overridden to provide custom pre-save logic.
    /// </summary>
    public virtual void PreSave()
    {
    }

    /// <summary>
    /// A virtual method that is invoked after the entity has been persisted.
    /// Can be overridden to provide custom post-save logic.
    /// </summary>
    public virtual void PostSave()
    {
    }

    internal void HandleTimestamps()
    {
        // check whether to track updates
        var trackUpdates = GetType().ShouldTrackTimestamps();

        if (trackUpdates)
        {
            var now = DateTime.UtcNow;
            CreatedAt ??= now;
            UpdatedAt = now;
        }
    }

    internal void MarkDeleted()
    {
        DeletedAt = DateTimeOffset.UtcNow;
        TouchUpdatedAt();
    }

    internal void MarkRestored()
    {
        DeletedAt = null;
        TouchUpdatedAt();
    }

    private void TouchUpdatedAt()
    {
        if (GetType().ShouldTrackTimestamps())
            UpdatedAt = DateTime.UtcNow;
    }

    internal void HandleLowercaseTransformations()
    {
        var properties = this.AttributedWith<LowercaseAttribute>().ToList();

        if (properties.Any(x => x.PropertyType != TypeConstants.StringType))
            throw new InvalidAttributeException("The 'Lowercase' attribute can only be applied to strings.");

        foreach (var property in properties)
        {
            var value = (string)property.GetValue(this, null);
            property.SetValue(this, value?.ToLower(CultureInfo.CurrentCulture));
        }
    }

    internal void HandleUppercaseTransformations()
    {
        var properties = this.AttributedWith<UppercaseAttribute>().ToList();

        if (properties.Any(x => x.PropertyType != TypeConstants.StringType))
            throw new InvalidAttributeException("The 'Uppercase' attribute can only be applied to strings.");

        foreach (var property in properties)
        {
            var value = (string)property.GetValue(this, null);
            property.SetValue(this, value?.ToUpper(CultureInfo.CurrentCulture));
        }
    }
}