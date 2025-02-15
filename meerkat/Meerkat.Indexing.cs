using System;
using System.Linq;
using meerkat.Attributes;
using meerkat.Constants;
using meerkat.Enums;
using meerkat.Extensions;
using MongoDB.Driver;

namespace meerkat;

public static partial class Meerkat
{
    private static void HandleIndexing<TSchema, TId>(Type type, IMongoCollection<TSchema> collection)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var typeName = type.FullName;

        if (SchemasWithCheckedIndices.ContainsKey(typeName))
            return;

        HandleUniqueIndexing(type, collection);
        HandleSingleFieldIndexing(type, collection);

        SchemasWithCheckedIndices[typeName] = true;
    }
    
    private static void HandleUniqueIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection)
    {
        var attributedMembers = type.GetAttributedMembers<UniqueIndexAttribute>();
        var indices = attributedMembers
            .Select(x =>
            {
                var field = new StringFieldDefinition<TSchema>(x.Value.Name);
                var definition = new IndexKeysDefinitionBuilder<TSchema>().Ascending(field);
                return new CreateIndexModel<TSchema>(definition, MongoDbConstants.UniqueIndexOptions);
            })
            .ToList();

        if (indices.Any())
            collection.Indexes.CreateMany(indices);
    }
    
    private static void HandleSingleFieldIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection)
    {
        var attributedMembers = type.GetAttributedMembers<SingleFieldIndexAttribute>();
        var indices = attributedMembers
            .Select(x =>
            {
                var field = new StringFieldDefinition<TSchema>(x.Value.Name);
                var definitionBuilder = new IndexKeysDefinitionBuilder<TSchema>();
                var definition =  x.Key.IndexOrder switch
                {
                     IndexOrder.Ascending => definitionBuilder.Ascending(field),
                     IndexOrder.Descending => definitionBuilder.Descending(field),
                     IndexOrder.Hashed => definitionBuilder.Hashed(field),
                    _ => throw new ArgumentOutOfRangeException()
                };
                return new CreateIndexModel<TSchema>(definition, new CreateIndexOptions());
            })
            .ToList();

        if (indices.Any())
            collection.Indexes.CreateMany(indices);
    }
}
