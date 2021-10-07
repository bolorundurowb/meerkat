using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace meerkat.Collections
{
    public static class Enumerables
    {
        public static async Task SaveAllAsync<TSchema>(this IEnumerable<TSchema> entities,
            CancellationToken cancellationToken = default) where TSchema : Schema
        {
            var collection = Meerkat.GetCollectionForType<TSchema>();
            var options = new InsertManyOptions
            {
                IsOrdered = false,
                BypassDocumentValidation = true
            };
            await collection.InsertManyAsync(entities, options, cancellationToken);
        }
    }
}