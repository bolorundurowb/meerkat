﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Attributes;
using meerkat.Constants;
using meerkat.Extensions;
using meerkat.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoUrlParser;

namespace meerkat
{
    public static class Meerkat
    {
        private static readonly ConcurrentBag<string> SchemasWithCheckedIndices = new ConcurrentBag<string>();

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
            
#if NET6_0 || NET7_0 || NET8_0
            BsonSerializer.RegisterSerializer(new DocumentDateOnlySerializer());
            BsonSerializer.RegisterSerializer(new DocumentTimeOnlySerializer());
#endif

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
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The found entity or null if not found</returns>
        public static TSchema? FindById<TSchema>(object entityId) where TSchema : Schema =>
            Query<TSchema>().FirstOrDefault(x => x.Id == entityId);

        /// <summary>
        /// Search for an entity by id asynchronously
        /// </summary>
        /// <param name="entityId">The entity id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The found entity or null if not found</returns>
        public static Task<TSchema?> FindByIdAsync<TSchema>(object entityId,
            CancellationToken cancellationToken = default) where TSchema : Schema =>
            Query<TSchema>().FirstOrDefaultAsync(x => x.Id == entityId, cancellationToken);

        /// <summary>
        /// Search for an entity by a predicate
        /// </summary>
        /// <param name="predicate">A function to test each element. If not defined, selects the first collection entity</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The found entity or null if not found</returns>
        public static TSchema? FindOne<TSchema>(Expression<Func<TSchema, bool>>? predicate = null) where TSchema : Schema
        {
            predicate ??= schema => true;
            return Query<TSchema>().FirstOrDefault(predicate);
        }

        /// <summary>
        /// Search for an entity by a predicate asynchronously
        /// </summary>
        /// <param name="predicate">A function to test each element. If not defined, selects the first collection entity</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The found entity or null if not found</returns>
        public static Task<TSchema?> FindOneAsync<TSchema>(Expression<Func<TSchema, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            predicate ??= schema => true;
            return Query<TSchema>().FirstOrDefaultAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Search for entities by predicate
        /// </summary>
        /// <param name="predicate">A function to test each element. If not defined, returns the entire collection</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The list of matched entities</returns>
        public static List<TSchema> Find<TSchema>(Expression<Func<TSchema, bool>>? predicate = null)
            where TSchema : Schema
        {
            predicate ??= schema => true;
            return Query<TSchema>().Where(predicate).ToList();
        }

        /// <summary>
        /// Search for entities by predicate asynchronously
        /// </summary>
        /// <param name="predicate">A function to test each element. If not defined, returns the entire collection</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The list of matched entities</returns>
        public static Task<List<TSchema>> FindAsync<TSchema>(Expression<Func<TSchema, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            predicate ??= schema => true;
            return Query<TSchema>().Where(predicate).ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Delete an entity by it's id
        /// </summary>
        /// <param name="entityId">The entity id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static void RemoveById<TSchema>(object entityId, CancellationToken cancellationToken = default)
            where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            collection.DeleteOne(x => x.Id == entityId, cancellationToken);
        }

        /// <summary>
        /// Delete an entity by it's id asynchronously
        /// </summary>
        /// <param name="entityId">The entity id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static Task RemoveByIdAsync<TSchema>(object entityId, CancellationToken cancellationToken = default)
            where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            return collection.DeleteOneAsync(x => x.Id == entityId, cancellationToken);
        }

        /// <summary>
        /// Delete an entity by a predicate
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static void RemoveOne<TSchema>(Expression<Func<TSchema, bool>> predicate,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            collection.DeleteOne(predicate, cancellationToken);
        }

        /// <summary>
        /// Delete an entity by a predicate asynchronously
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static Task RemoveOneAsync<TSchema>(Expression<Func<TSchema, bool>> predicate,
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
        public static void Remove<TSchema>(Expression<Func<TSchema, bool>> predicate,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            collection.DeleteMany(predicate, cancellationToken);
        }

        /// <summary>
        /// Delete multiple entities by a predicate asynchronously
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static Task RemoveAsync<TSchema>(Expression<Func<TSchema, bool>> predicate,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = GetCollectionForType<TSchema>();
            return collection.DeleteManyAsync(predicate, cancellationToken);
        }

        /// <summary>
        /// Count number of documents that match predicate
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The number of entries that match the predicate</returns>
        public static long Count<TSchema>(Expression<Func<TSchema, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            predicate ??= schema => true;
            var collection = GetCollectionForType<TSchema>();
            return collection.CountDocuments(predicate, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Count number of documents that match predicate asynchronously
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The number of entries that match the predicate</returns>
        public static Task<long> CountAsync<TSchema>(Expression<Func<TSchema, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            predicate ??= schema => true;
            var collection = GetCollectionForType<TSchema>();
            return collection.CountDocumentsAsync(predicate, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Count number of documents that match predicate
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The number of entries that match the predicate</returns>
        public static bool Exists<TSchema>(Expression<Func<TSchema, bool>>? predicate = null) where TSchema : Schema
        {
            predicate ??= schema => true;
            return Query<TSchema>().Any(predicate);
        }

        /// <summary>
        /// Count number of documents that match predicate asynchronously
        /// </summary>
        /// <param name="predicate">A function to test each element</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        /// <returns>The number of entries that match the predicate</returns>
        public static Task<bool> ExistsAsync<TSchema>(Expression<Func<TSchema, bool>>? predicate = null,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            predicate ??= schema => true;
            return Query<TSchema>().AnyAsync(predicate, cancellationToken);
        }

        internal static IMongoCollection<TSchema> GetCollectionForType<TSchema>(TSchema model) where TSchema : Schema
        {
            if (Database == null)
                throw new InvalidOperationException(
                    $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

            var type = model.GetType();
            var collectionName = type.GetCollectionName();
            var collection = Database.GetCollection<TSchema>(collectionName);

            HandleUniqueIndexing(type, collection);

            return collection;
        }

        internal static IMongoCollection<TSchema> GetCollectionForType<TSchema>() where TSchema : Schema
        {
            if (Database == null)
                throw new InvalidOperationException(
                    $"The database connection has not been initialized. Call {nameof(Connect)}() before carrying out any operations.");

            var type = typeof(TSchema);
            var collectionName = type.GetCollectionName();
            var collection = Database.GetCollection<TSchema>(collectionName);

            HandleUniqueIndexing(type, collection);

            return collection;
        }

        private static void HandleUniqueIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection) where TSchema : Schema
        {
            var typeName = type.FullName;

            if (!SchemasWithCheckedIndices.IsEmpty && SchemasWithCheckedIndices.Contains(typeName))
                return;

            // get properties that have the attribute applied
            var properties = type.AttributedWith<UniqueAttribute>();

            var indices = properties
                .Select(x =>
                {
                    var field = new StringFieldDefinition<TSchema>(x.Name);
                    var definition = new IndexKeysDefinitionBuilder<TSchema>().Ascending(field);
                    return new CreateIndexModel<TSchema>(definition, MongoDbConstants.UniqueIndexOptions);
                })
                .ToList();
            
            if (indices.Any()) 
                collection.Indexes.CreateMany(indices);

            // track this indexing
            SchemasWithCheckedIndices.Add(typeName);
        }
    }
}