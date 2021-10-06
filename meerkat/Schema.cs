﻿using System;
using System.Threading.Tasks;
using meerkat.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

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
        /// Upserts the current instance in the matched collection
        /// </summary>
        public async Task Save()
        {
            var collection = Meerkat.GetCollectionForType(this);

            // check whether to track updates
            var trackUpdates = GetType().ShouldTrackTimestamps();

            // check to see if the object exists in storage
            var instance = await collection
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
    }
}