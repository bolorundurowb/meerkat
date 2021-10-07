using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace meerkat.Collections
{
    public static class Enumerables
    {
        private static readonly InsertManyOptions BulkInsertOptions = new InsertManyOptions
        {
            IsOrdered = false,
            BypassDocumentValidation = true
        };

        /// <summary>
        /// Persist a collection of entities to the matched collection synchronously
        /// </summary>
        /// <param name="entities">A collection of entities to be persisted</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <typeparam name="TSchema">The type of entity</typeparam>
        public static void SaveAll<TSchema>(this IEnumerable<TSchema> entities,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType<TSchema>();
            collection.InsertMany(entities, BulkInsertOptions, cancellationToken);
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
            await collection.InsertManyAsync(entities, BulkInsertOptions, cancellationToken);
        }
    }
}