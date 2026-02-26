using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace meerkat.Collections;

public static class Enumerables
{
    private static readonly BulkWriteOptions BulkInsertOptions = new()
        { IsOrdered = false, BypassDocumentValidation = false };

    /// <summary>
    /// Persists a collection of entities to their corresponding MongoDB collection synchronously.
    /// </summary>
    /// <param name="entities">A collection of entities to be persisted.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static void SaveAll<TSchema, TId>(this IEnumerable<TSchema> entities)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var entityList = entities.ToList();
        var operations = GetBulkOps<TSchema, TId>(entityList);

        ProcessEntitiesPreSave<TSchema, TId>(entityList);

        var collection = Meerkat.GetCollectionForType<TSchema, TId>();
        collection.BulkWrite(operations, BulkInsertOptions);

        ProcessEntitiesPostSaves<TSchema, TId>(entityList);
    }

    /// <summary>
    /// Persists a collection of entities to their corresponding MongoDB collection asynchronously.
    /// </summary>
    /// <param name="entities">A collection of entities to be persisted.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static async Task SaveAllAsync<TSchema, TId>(this IEnumerable<TSchema> entities,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var entityList = entities.ToList();
        var operations = GetBulkOps<TSchema, TId>(entityList);

        ProcessEntitiesPreSave<TSchema, TId>(entityList);

        var collection = Meerkat.GetCollectionForType<TSchema, TId>();
        await collection.BulkWriteAsync(operations, BulkInsertOptions, cancellationToken);

        ProcessEntitiesPostSaves<TSchema, TId>(entityList);
    }

    private static void ProcessEntitiesPreSave<TSchema, TId>(IEnumerable<TSchema> entities)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        foreach (var entity in entities)
        {
            entity.HandleTimestamps();
            entity.HandleLowercaseTransformations();
            entity.HandleUppercaseTransformations();

            entity.PreSave();
        }
    }

    private static void ProcessEntitiesPostSaves<TSchema, TId>(IEnumerable<TSchema> entities)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        foreach (var entity in entities)
            entity.PostSave();
    }

    private static List<WriteModel<TSchema>> GetBulkOps<TSchema, TId>(IEnumerable<TSchema> entities)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        return entities.Select(entity =>
                new ReplaceOneModel<TSchema>(
                        Builders<TSchema>.Filter.Where(x => x.Id.Equals(entity.Id)), entity)
                    { IsUpsert = true })
            .Cast<WriteModel<TSchema>>()
            .ToList();
    }
}
