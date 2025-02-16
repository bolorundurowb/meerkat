using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Attributes;
using meerkat.Constants;
using meerkat.Exceptions;
using meerkat.Extensions;
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
    public TId Id { get; protected set; }

    /// <summary>
    /// Gets the timestamp indicating when the document was first persisted to the database.
    /// </summary>
    public DateTime? CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp indicating when the document was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Saves or updates the current instance in the corresponding MongoDB collection synchronously.
    /// </summary>
    public void Save()
    {
        var collection = Meerkat.GetCollectionForType<Schema<TId>, TId>(this);

        HandleTimestamps();
        HandleLowercaseTransformations();
        HandleUppercaseTransformations();

        PreSave();

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
        var collection = Meerkat.GetCollectionForType<Schema<TId>, TId>(this);

        HandleTimestamps();
        HandleLowercaseTransformations();
        HandleUppercaseTransformations();

        PreSave();

        await collection.ReplaceOneAsync(x => x.Id.Equals(Id), this, MongoDbConstants.ReplaceOptions,
            cancellationToken);

        PostSave();
    }

    /// <summary>
    /// A virtual method that is invoked before the entity is persisted.
    /// Can be overridden to provide custom pre-save logic.
    /// </summary>
    public virtual void PreSave() { }

    /// <summary>
    /// A virtual method that is invoked after the entity has been persisted.
    /// Can be overridden to provide custom post-save logic.
    /// </summary>
    public virtual void PostSave() { }
    
    private void HandleTimestamps()
    {
        // check whether to track updates
        var trackUpdates = GetType().ShouldTrackTimestamps();

        if (trackUpdates)
        {
            if (!CreatedAt.HasValue)
                CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }
    }

    private void HandleLowercaseTransformations()
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

    private void HandleUppercaseTransformations()
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
