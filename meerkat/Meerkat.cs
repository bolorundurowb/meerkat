﻿using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoUrlParser;

namespace meerkat
{
    public class Meerkat
    {
        /// <summary>
        /// The database that we have connected to
        /// </summary>
        public static IMongoDatabase Database;

        /// <summary>
        /// Connect to the database
        /// </summary>
        /// <param name="databaseConnectionString">A full mongodb connection string</param>
        public static void Connect(string databaseConnectionString)
        {
            var (dbUrl, dbName) = Parser.Parse(databaseConnectionString);

            var dbClient = new MongoClient(dbUrl);
            Database = dbClient.GetDatabase(dbName);
        }

        /// <summary>
        /// Get a raw queryable for a given type
        /// </summary>
        /// <typeparam name="TSchema">Type to be queried</typeparam>
        /// <returns>The queryable</returns>
        public static IMongoQueryable<TSchema> Query<TSchema>() where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            return collection
                .AsQueryable();
        }

        /// <summary>
        /// Search for an entity by id
        /// </summary>
        /// <param name="entityId">The entity id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The found entity or null if not found</returns>
        public static Task<TSchema> FindById<TSchema>(object entityId, CancellationToken cancellationToken = default)
            where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            return collection
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);
        }

        /// <summary>
        /// Search for an entity by a predicate
        /// </summary>
        /// <param name="predicate">A function to test each element. If not defined, selects the first collection entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The found entity or null if not found</returns>
        public static Task<TSchema> FindOne<TSchema>(Expression<Func<TSchema, bool>> predicate = null,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            predicate ??= schema => true;
            var collection = GetCollectionForType<TSchema>();
            return collection
                .AsQueryable()
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Delete an entity by it's id
        /// </summary>
        /// <param name="entityId">The entity id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static async Task RemoveById<TSchema>(object entityId, CancellationToken cancellationToken = default)
            where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            await collection.DeleteOneAsync(x => x.Id == entityId, cancellationToken);
        }

        /// <summary>
        /// Delete an entity by a predicate
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static Task RemoveOne<TSchema>(Expression<Func<TSchema, bool>> predicate,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            return collection.DeleteOneAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Delete an multiple entities by a predicate
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static Task Remove<TSchema>(Expression<Func<TSchema, bool>> predicate,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            return collection.DeleteManyAsync(predicate, cancellationToken);
        }

        internal static IMongoCollection<TSchema> GetCollectionForType<TSchema>(TSchema model) where TSchema : Schema
        {
            if (Database == null)
                throw new InvalidOperationException(
                    $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

            var collectionName = model.GetType().GetCollectionName();
            return Database.GetCollection<TSchema>(collectionName);
        }

        internal static IMongoCollection<TSchema> GetCollectionForType<TSchema>() where TSchema : Schema
        {
            if (Database == null)
                throw new InvalidOperationException(
                    $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

            var collectionName = typeof(TSchema).GetCollectionName();
            return Database.GetCollection<TSchema>(collectionName);
        }
    }
}