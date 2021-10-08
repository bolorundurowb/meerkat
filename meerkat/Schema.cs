using System;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace meerkat
{
    public abstract class Schema
    {
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions
            { BypassDocumentValidation = true, IsUpsert = true };

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

            // check whether to track updates
            var trackUpdates = GetType().ShouldTrackTimestamps();

            if (trackUpdates)
            {
                if (!CreatedAt.HasValue)
                    CreatedAt = DateTime.UtcNow;

                UpdatedAt = DateTime.UtcNow;
            }

            collection.ReplaceOne(x => x.Id == Id, this, _replaceOptions);
        }

        /// <summary>
        /// Upserts the current instance in the matched collection asynchronously
        /// </summary>
        public async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            var collection = Meerkat.GetCollectionForType(this);

            // check whether to track updates
            var trackUpdates = GetType().ShouldTrackTimestamps();

            if (trackUpdates)
            {
                if (!CreatedAt.HasValue)
                    CreatedAt = DateTime.UtcNow;

                UpdatedAt = DateTime.UtcNow;
            }

            await collection.ReplaceOneAsync(x => x.Id == Id, this, _replaceOptions, cancellationToken);
        }
    }
}