using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Attributes;
using meerkat.Constants;
using meerkat.Extensions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoUrlParser;

namespace meerkat;

public static partial class Meerkat
{
    private static void HandleUniqueIndexing<TSchema, TId>(Type type, IMongoCollection<TSchema> collection)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var typeName = type.FullName;

        if (SchemasWithCheckedIndices.ContainsKey(typeName))
            return;

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

        SchemasWithCheckedIndices[typeName] = true;
    }
}
