using System;
using System.Linq;
using meerkat.Attributes;
using meerkat.Constants;
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

        SchemasWithCheckedIndices[typeName] = true;
    }
    
    private static void HandleUniqueIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection)
    {
        var properties = type.AttributedWith<UniqueIndexAttribute>();
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
    }
}
