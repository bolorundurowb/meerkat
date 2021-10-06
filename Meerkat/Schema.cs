using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Meerkat.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Meerkat
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
        /// Upserts the current instance in the matched collection
        /// </summary>
        public async Task Save()
        {
            var collection = Meerkat.GetCollectionForType(this);

            // check whether to track updates
            var trackUpdates = GetType().ShouldTrackTimestamps();

            // check to see if the object exists in storage
            var instance = collection
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Id == Id);

            if (instance == null)
            {
                if (trackUpdates)
                    CreatedAt = UpdatedAt = DateTime.UtcNow;

                await collection.InsertOneAsync(this);
            }
            else
            {
                if (trackUpdates)
                    UpdatedAt = DateTime.UtcNow;

                await collection.ReplaceOneAsync(x => x.Id == Id, this);
            }
        }

        public IMongoQueryable<TSchema> Query<TSchema>() where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType(this as TSchema);
            return collection
                .AsQueryable();
        }

        public Task<TSchema> FindById<TSchema>(object entityId, CancellationToken cancellationToken = default)
            where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType(this as TSchema);
            return collection
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);
        }

        public Task<TSchema> FindOne<TSchema>(Expression<Func<TSchema, bool>> predicate,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType(this as TSchema);
            return collection
                .AsQueryable()
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }
    }
}