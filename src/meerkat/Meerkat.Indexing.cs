using System;
using System.Linq;
using System.Reflection;
using meerkat.Attributes;
using meerkat.Enums;
using meerkat.Exceptions;
using meerkat.Extensions;
using meerkat.Services;
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
        HandleSoftDeleteIndexing(type, collection);

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

                var expireAfter = attribute.ExpireAfter is string d
                    ? DurationParser.ParseDuration(d)
                    : (TimeSpan?)null;

                var memberType = memberInfo switch
                {
                    PropertyInfo propertyInfo => propertyInfo.PropertyType,
                    FieldInfo fieldInfo => fieldInfo.FieldType,
                    _ => null
                };

                var isDate = memberType == typeof(DateTime) || memberType == typeof(DateTime?);
                if (expireAfter.HasValue && !isDate)
                    throw new InvalidAttributeException(
                        "The 'ExpireAfter' TTL option can only be applied to DateTime or DateTime? fields.");

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
                    new CreateIndexOptions { Sparse = attribute.Sparse, Name = attribute.Name, ExpireAfter = expireAfter });
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
            var uniqueFlags = nameGroup.Value.Select(kvp => kvp.Key.Unique).Distinct().ToList();
            if (uniqueFlags.Count > 1)
                throw new InvalidAttributeException(
                    "Members of a compound index group must agree on the 'Unique' value.");

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
                new CreateIndexModel<TSchema>(indexDefinition,
                    new CreateIndexOptions { Name = nameGroup.Key, Unique = uniqueFlags.Single() });
            if (collection is { Indexes: not null })
                collection.Indexes.CreateOne(indexModel);
        }
    }

    internal static void HandleSoftDeleteIndexing<TSchema>(Type type, IMongoCollection<TSchema> collection)
    {
        if (!type.ShouldSoftDelete() || collection.Indexes == null)
            return;

        var field = new StringFieldDefinition<TSchema>(nameof(Schema<string>.DeletedAt));
        var definition = new IndexKeysDefinitionBuilder<TSchema>().Ascending(field);
        var indexModel = new CreateIndexModel<TSchema>(definition,
            new CreateIndexOptions { Name = "deleted_at_idx" });
        collection.Indexes.CreateOne(indexModel);
    }
}
