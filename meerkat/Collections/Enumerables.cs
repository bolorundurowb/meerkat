using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Constants;
using MongoDB.Driver;

namespace meerkat.Collections
{
    public static class Enumerables
    {
        /// <summary>
        /// Persist a collection of entities to the matched collection synchronously
        /// </summary>
        /// <param name="entities">A collection of entities to be persisted</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static void SaveAll<TSchema>(this IEnumerable<TSchema> entities) where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType<TSchema>();
            var operations = GetBulkOps(entities);
            collection.BulkWrite(operations, MongoDbConstants.BulkInsertOptions);
        }

        /// <summary>
        /// Persist a collection of entities to the matched collection asynchronously
        /// </summary>
        /// <param name="entities">A collection of entities to be persisted</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static async Task SaveAllAsync<TSchema>(this IEnumerable<TSchema> entities,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType<TSchema>();
            var operations = GetBulkOps(entities);
            await collection.BulkWriteAsync(operations, MongoDbConstants.BulkInsertOptions, cancellationToken);
        }

        private static List<WriteModel<TSchema>> GetBulkOps<TSchema>(IEnumerable<TSchema> entities)
            where TSchema : Schema
        {
            return entities.Select(entity =>
                    new ReplaceOneModel<TSchema>(Builders<TSchema>.Filter.Where(x => x.Id == entity.Id), entity)
                        { IsUpsert = true })
                .Cast<WriteModel<TSchema>>()
                .ToList();
        }
    }
}