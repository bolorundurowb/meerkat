using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Attributes;
using meerkat.Constants;
using meerkat.Exceptions;
using meerkat.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace meerkat
{
    public abstract class Schema<TKey> where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Can be a value of any type but defaults to ObjectId
        /// </summary>
        [BsonId]
        public virtual TKey Id { get; protected set; }

        /// <summary>
        /// Time when this entity was first persisted to the database
        /// </summary>
        public DateTime? CreatedAt { get; private set; }

        /// <summary>
        /// Time when this entity was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>
        /// Upserts the current instance in the matched collection synchronously
        /// </summary>
        public void Save<TKey, TSchema>() where TSchema : Schema<TKey> where TKey : IEquatable<TKey>
        {
            var collection = Meerkat.GetCollectionForType<TKey, TSchema>(this);

            HandleTimestamps();
            HandleLowercaseTransformations();
            HandleUppercaseTransformations();

            collection.ReplaceOne(x => x.Id.Equals(Id), this, MongoDbConstants.ReplaceOptions);
        }

        /// <summary>
        /// Upserts the current instance in the matched collection asynchronously
        /// </summary>
        public async Task SaveAsync<TKey, TSchema>(CancellationToken cancellationToken = default) where TSchema : Schema<TKey> where TKey : IEquatable<TKey>
        {
            var collection = Meerkat.GetCollectionForType<TKey, TSchema>(this);

            HandleTimestamps();
            HandleLowercaseTransformations();
            HandleUppercaseTransformations();

            await collection.ReplaceOneAsync<TSchema>(x => x.Id.Equals(Id), this, MongoDbConstants.ReplaceOptions, cancellationToken);
        }

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
}