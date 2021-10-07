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

        public static void SaveAll<TSchema>(this IEnumerable<TSchema> entities,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType<TSchema>();
            collection.InsertMany(entities, BulkInsertOptions, cancellationToken);
        }

        public static async Task SaveAllAsync<TSchema>(this IEnumerable<TSchema> entities,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType<TSchema>();
            await collection.InsertManyAsync(entities, BulkInsertOptions, cancellationToken);
        }
    }
}