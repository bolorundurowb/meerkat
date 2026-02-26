using System;
using System.Linq;
using meerkat.Attributes;
using meerkat.Enums;
using meerkat.Extensions;
using MongoDB.Driver;

namespace meerkat;

public static partial class Meerkat
{
    internal static void HandleIndexing<TSchema, TId>(Type type, IMongoCollection<TSchema> collection)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var typeName = type.FullName;

        if (SchemasWithCheckedIndices.ContainsKey(typeName))
            return;

        HandleUniqueIndexing(type, collection);
        HandleSingleFieldIndexing(type, collection);
        HandleGeospatialFieldIndexing(type, collection);
        HandleCompoundFieldIndexing(type, collection);

        SchemasWithCheckedIndices[typeName] = true;
    }

    internal static void HandleUniqueIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection)
    {
        var attributedMembers = type.GetAttributedMembers<UniqueIndexAttribute>();
        var indices = attributedMembers
            .Select(x =>
            {
                var attribute = x.Key;
                var memberInfo = x.Value;

                var field = new StringFieldDefinition<TSchema>(memberInfo.Name);
                var definition = new IndexKeysDefinitionBuilder<TSchema>().Ascending(field);
                return new CreateIndexModel<TSchema>(definition,
                    new CreateIndexOptions { Unique = true, Sparse = attribute.Sparse, Name = attribute.Name });
            })
            .ToList();

        if (indices.Any() && collection.Indexes != null)
            collection.Indexes.CreateMany(indices);
    }

    internal static void HandleSingleFieldIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection)
    {
        var attributedMembers = type.GetAttributedMembers<SingleFieldIndexAttribute>();
        var indices = attributedMembers
            .Select(x =>
            {
                var attribute = x.Key;
                var memberInfo = x.Value;

                var field = new StringFieldDefinition<TSchema>(memberInfo.Name);
                var definitionBuilder = new IndexKeysDefinitionBuilder<TSchema>();
                var definition = attribute.IndexOrder switch
                {
                    IndexOrder.Ascending => definitionBuilder.Ascending(field),
                    IndexOrder.Descending => definitionBuilder.Descending(field),
                    IndexOrder.Hashed => definitionBuilder.Hashed(field),
                    _ => throw new ArgumentOutOfRangeException()
                };
                return new CreateIndexModel<TSchema>(definition,
                    new CreateIndexOptions { Sparse = attribute.Sparse, Name = attribute.Name });
            })
            .ToList();

        if (indices.Any() && collection.Indexes != null)
            collection.Indexes.CreateMany(indices);
    }

    internal static void HandleGeospatialFieldIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection)
    {
        var attributedMembers = type.GetAttributedMembers<GeospatialIndexAttribute>();
        var indices = attributedMembers
            .Select(x =>
            {
                var attribute = x.Key;
                var memberInfo = x.Value;

                var field = new StringFieldDefinition<TSchema>(memberInfo.Name);
                var definitionBuilder = new IndexKeysDefinitionBuilder<TSchema>();
                var definition = attribute.IndexType switch
                {
                    GeospatialIndexType.TwoD => definitionBuilder.Geo2D(field),
                    GeospatialIndexType.TwoDSphere => definitionBuilder.Geo2DSphere(field),
                    _ => throw new ArgumentOutOfRangeException()
                };
                return new CreateIndexModel<TSchema>(definition, new CreateIndexOptions { Name = attribute.Name });
            })
            .ToList();

        if (indices.Any() && collection.Indexes != null)
            collection.Indexes.CreateMany(indices);
    }

    internal static void HandleCompoundFieldIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection)
    {
        var attributedMembers = type.GetAttributedMembers<CompoundIndexAttribute>();

        if (!attributedMembers.Any())
            return;

        var groupedIndexes = attributedMembers
            .GroupBy(kvp => kvp.Key.Name)
            .ToDictionary(x => x.Key, y => y.ToList());

        foreach (var nameGroup in groupedIndexes)
        {
            var indexKeys = Builders<TSchema>.IndexKeys;
            var indexDefinition = nameGroup.Value.Aggregate(indexKeys.Combine(),
                (current, groupAttributedMembers) =>
                {
                    return groupAttributedMembers.Key.IndexOrder switch
                    {
                        IndexOrder.Ascending => current.Ascending(groupAttributedMembers.Value.Name),
                        IndexOrder.Descending => current.Descending(groupAttributedMembers.Value.Name),
                        IndexOrder.Hashed => current.Hashed(groupAttributedMembers.Value.Name),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                });

            var indexModel =
                new CreateIndexModel<TSchema>(indexDefinition, new CreateIndexOptions { Name = nameGroup.Key });
            if (collection is { Indexes: not null })
                collection.Indexes.CreateOne(indexModel);
        }
    }
}
