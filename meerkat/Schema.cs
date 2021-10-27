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
    public abstract class Schema
    {
        /// <summary>
        /// Can be a value of any type but defaults to ObjectId
        /// </summary>
        [BsonId]
        public virtual object Id { get; protected set; }

        /// <summary>
        /// Time when this entity was first persisted to the database
        /// </summary>
        public DateTime? CreatedAt { get; private set; }

        /// <summary>
        /// Time when this entity was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        protected Schema() => Id = ObjectId.GenerateNewId();

        /// <summary>
        /// Upserts the current instance in the matched collection synchronously
        /// </summary>
        public void Save()
        {
            var collection = Meerkat.GetCollectionForType(this);

            HandleTimestamps();
            HandleLowercaseTransformations();
            HandleUppercaseTransformations();

            collection.ReplaceOne(x => x.Id == Id, this, MongoDbConstants.ReplaceOptions);
        }

        /// <summary>
        /// Upserts the current instance in the matched collection asynchronously
        /// </summary>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            var collection = Meerkat.GetCollectionForType(this);

            HandleTimestamps();
            HandleLowercaseTransformations();
            HandleUppercaseTransformations();

            await collection.ReplaceOneAsync(x => x.Id == Id, this, MongoDbConstants.ReplaceOptions, cancellationToken);
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