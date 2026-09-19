using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Attributes;
using meerkat.Enums;
using meerkat.Exceptions;
using meerkat.Extensions;
using meerkat.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace meerkat;

public static partial class Meerkat
{
    internal static void HandleIndexing<TSchema, TId>(Type type, IMongoCollection<TSchema> collection)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var typeName = type.FullName ?? type.Name;

        if (SchemasWithCheckedIndices.ContainsKey(typeName))
            return;

        var models = BuildIndexModels<TSchema>(type);
        ApplyIndexes(models, collection);
        SchemasWithCheckedIndices[typeName] = true;
    }

    internal static List<CreateIndexModel<TSchema>> BuildIndexModels<TSchema>(Type type)
    {
        var models = new List<CreateIndexModel<TSchema>>();
        models.AddRange(BuildUniqueIndexModels<TSchema>(type));
        models.AddRange(BuildSingleFieldIndexModels<TSchema>(type));
        models.AddRange(BuildGeospatialIndexModels<TSchema>(type));
        models.AddRange(BuildCompoundIndexModels<TSchema>(type));
        models.AddRange(BuildSoftDeleteIndexModels<TSchema>(type));
        return models;
    }

    internal static List<CreateIndexModel<TSchema>> BuildUniqueIndexModels<TSchema>(Type type)
    {
        return type.GetAttributedMembers<UniqueIndexAttribute>()
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
    }

    internal static List<CreateIndexModel<TSchema>> BuildSingleFieldIndexModels<TSchema>(Type type)
    {
        return type.GetAttributedMembers<SingleFieldIndexAttribute>()
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
    }

    internal static List<CreateIndexModel<TSchema>> BuildGeospatialIndexModels<TSchema>(Type type)
    {
        return type.GetAttributedMembers<GeospatialIndexAttribute>()
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
    }

    internal static List<CreateIndexModel<TSchema>> BuildCompoundIndexModels<TSchema>(Type type)
    {
        var models = new List<CreateIndexModel<TSchema>>();

        foreach (var nameGroup in type.GetAttributedMembers<CompoundIndexAttribute>().GroupBy(kvp => kvp.Key.Name))
        {
            var uniqueFlags = nameGroup.Select(kvp => kvp.Key.Unique).Distinct().ToList();
            if (uniqueFlags.Count > 1)
                throw new InvalidAttributeException(
                    "Members of a compound index group must agree on the 'Unique' value.");

            var indexKeys = Builders<TSchema>.IndexKeys;
            var indexDefinition = nameGroup.Aggregate(indexKeys.Combine(),
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

            models.Add(new CreateIndexModel<TSchema>(indexDefinition,
                new CreateIndexOptions { Name = nameGroup.Key, Unique = uniqueFlags.Single() }));
        }

        return models;
    }

    internal static List<CreateIndexModel<TSchema>> BuildSoftDeleteIndexModels<TSchema>(Type type)
    {
        if (!type.ShouldSoftDelete())
            return new List<CreateIndexModel<TSchema>>();

        var field = new StringFieldDefinition<TSchema>(nameof(Schema<string>.DeletedAt));
        var definition = new IndexKeysDefinitionBuilder<TSchema>().Ascending(field);
        return new List<CreateIndexModel<TSchema>>
        {
            new(definition, new CreateIndexOptions { Name = "deleted_at_idx" })
        };
    }

    internal static void ApplyIndexes<TSchema>(IEnumerable<CreateIndexModel<TSchema>> models,
        IMongoCollection<TSchema> collection, CancellationToken ct = default)
    {
        var indexModels = models.ToList();
        if (indexModels.Count == 0)
            return;

        if (collection.Indexes == null)
            throw new IndexVerificationException(collection.CollectionNamespace.CollectionName,
                new[] { "index manager is unavailable (null Indexes)" });

        collection.Indexes.CreateMany(indexModels, ct);
    }

    internal static async Task ApplyIndexesAsync<TSchema>(IEnumerable<CreateIndexModel<TSchema>> models,
        IMongoCollection<TSchema> collection, CancellationToken ct = default)
    {
        var indexModels = models.ToList();
        if (indexModels.Count == 0)
            return;

        if (collection.Indexes == null)
            throw new IndexVerificationException(collection.CollectionNamespace.CollectionName,
                new[] { "index manager is unavailable (null Indexes)" });

        await collection.Indexes.CreateManyAsync(indexModels, cancellationToken: ct).ConfigureAwait(false);
    }

    internal static void VerifyIndexes<TSchema>(IReadOnlyList<CreateIndexModel<TSchema>> models,
        IMongoCollection<TSchema> collection)
    {
        if (models.Count == 0)
            return;

        if (collection.Indexes == null)
            throw new IndexVerificationException(collection.CollectionNamespace.CollectionName,
                new[] { "index manager is unavailable (null Indexes)" });

        var existing = collection.Indexes.List().ToList();
        var missing = FindMissingIndexes(models, existing, collection);

        if (missing.Count > 0)
            throw new IndexVerificationException(collection.CollectionNamespace.CollectionName, missing);
    }

    internal static async Task VerifyIndexesAsync<TSchema>(IReadOnlyList<CreateIndexModel<TSchema>> models,
        IMongoCollection<TSchema> collection, CancellationToken ct = default)
    {
        if (models.Count == 0)
            return;

        if (collection.Indexes == null)
            throw new IndexVerificationException(collection.CollectionNamespace.CollectionName,
                new[] { "index manager is unavailable (null Indexes)" });

        var cursor = await collection.Indexes.ListAsync(cancellationToken: ct).ConfigureAwait(false);
        var existing = cursor.ToList();
        var missing = FindMissingIndexes(models, existing, collection);

        if (missing.Count > 0)
            throw new IndexVerificationException(collection.CollectionNamespace.CollectionName, missing);
    }

    private static List<string> FindMissingIndexes<TSchema>(IReadOnlyList<CreateIndexModel<TSchema>> models,
        IReadOnlyList<BsonDocument> existing, IMongoCollection<TSchema> collection)
    {
        var missing = new List<string>();

        foreach (var model in models)
        {
            if (!string.IsNullOrEmpty(model.Options.Name))
            {
                if (existing.All(doc => doc.GetValue("name", "").AsString != model.Options.Name))
                    missing.Add(model.Options.Name!);
                continue;
            }

            var expectedKey = RenderKeys(model, collection);
            var matched = existing.Any(doc =>
                doc.Contains("key") && doc["key"].IsBsonDocument &&
                KeysMatch(doc["key"].AsBsonDocument, expectedKey) &&
                OptionsMatch(doc, model.Options));

            if (!matched)
                missing.Add(expectedKey.ToJson());
        }

        return missing;
    }

    private static BsonDocument RenderKeys<TSchema>(CreateIndexModel<TSchema> model, IMongoCollection<TSchema> collection)
    {
        var renderArgs = new RenderArgs<TSchema>(
            collection.DocumentSerializer,
            collection.Settings.SerializerRegistry,
            new PathRenderArgs("", false),
            renderForFind: false,
            renderForElemMatch: false,
            renderDollarForm: false,
            translationOptions: null);
        return model.Keys.Render(renderArgs);
    }

    private static bool KeysMatch(BsonDocument actual, BsonDocument expected)
    {
        if (actual.ElementCount != expected.ElementCount)
            return false;

        foreach (var element in expected)
        {
            if (!actual.TryGetElement(element.Name, out var actualElement))
                return false;
            if (!actualElement.Value.Equals(element.Value))
                return false;
        }

        return true;
    }

    private static bool OptionsMatch(BsonDocument existing, CreateIndexOptions options)
    {
        if (options.Unique.HasValue && existing.GetValue("unique", false).ToBoolean() != options.Unique.Value)
            return false;

        if (options.Sparse.HasValue && existing.GetValue("sparse", false).ToBoolean() != options.Sparse.Value)
            return false;

        return true;
    }

    private static void MarkIndexesChecked(Type type)
    {
        SchemasWithCheckedIndices[type.FullName ?? type.Name] = true;
    }

    public static void EnsureIndexes<TSchema, TId>(CancellationToken ct = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        EnsureIndexesCore<TSchema, TId>(ct);

    public static Task EnsureIndexesAsync<TSchema, TId>(CancellationToken ct = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        EnsureIndexesCoreAsync<TSchema, TId>(ct);

    public static void EnsureIndexes(IEnumerable<Type> schemaTypes, CancellationToken ct = default)
    {
        if (schemaTypes == null)
            throw new ArgumentNullException(nameof(schemaTypes));

        foreach (var type in schemaTypes)
            EnsureIndexesForType(type, ct);
    }

    public static async Task EnsureIndexesAsync(IEnumerable<Type> schemaTypes, CancellationToken ct = default)
    {
        if (schemaTypes == null)
            throw new ArgumentNullException(nameof(schemaTypes));

        foreach (var type in schemaTypes)
            await EnsureIndexesForTypeAsync(type, ct).ConfigureAwait(false);
    }

    public static void EnsureIndexes(Assembly assembly, CancellationToken ct = default)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        EnsureIndexes(GetSchemaTypes(assembly), ct);
    }

    public static async Task EnsureIndexesAsync(Assembly assembly, CancellationToken ct = default)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        await EnsureIndexesAsync(GetSchemaTypes(assembly), ct).ConfigureAwait(false);
    }

    internal static void EnsureIndexesOnCollection<TSchema>(Type type, IMongoCollection<TSchema> collection,
        CancellationToken ct = default)
    {
        var models = BuildIndexModels<TSchema>(type);
        ApplyIndexes(models, collection, ct);
        VerifyIndexes(models, collection);
        MarkIndexesChecked(type);
    }

    internal static async Task EnsureIndexesOnCollectionAsync<TSchema>(Type type, IMongoCollection<TSchema> collection,
        CancellationToken ct = default)
    {
        var models = BuildIndexModels<TSchema>(type);
        await ApplyIndexesAsync(models, collection, ct).ConfigureAwait(false);
        await VerifyIndexesAsync(models, collection, ct).ConfigureAwait(false);
        MarkIndexesChecked(type);
    }

    private static void EnsureIndexesCore<TSchema, TId>(CancellationToken ct)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        EnsureIndexesOnCollection<TSchema>(typeof(TSchema), collection, ct);
    }

    private static async Task EnsureIndexesCoreAsync<TSchema, TId>(CancellationToken ct)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var collection = GetCollectionForType<TSchema, TId>();
        await EnsureIndexesOnCollectionAsync<TSchema>(typeof(TSchema), collection, ct).ConfigureAwait(false);
    }

    private static void EnsureIndexesForType(Type schemaType, CancellationToken ct)
    {
        var idType = ResolveSchemaIdType(schemaType);
        var method = typeof(Meerkat)
            .GetMethod(nameof(EnsureIndexesCore), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(schemaType, idType);
        method.Invoke(null, new object[] { ct });
    }

    private static async Task EnsureIndexesForTypeAsync(Type schemaType, CancellationToken ct)
    {
        var idType = ResolveSchemaIdType(schemaType);
        var method = typeof(Meerkat)
            .GetMethod(nameof(EnsureIndexesCoreAsync), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(schemaType, idType);
        var task = (Task)method.Invoke(null, new object[] { ct })!;
        await task.ConfigureAwait(false);
    }

    internal static IEnumerable<Type> GetSchemaTypes(Assembly assembly)
    {
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
        }

        return types.Where(IsSchemaType);
    }

    private static bool IsSchemaType(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
            return false;

        for (var baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Schema<>))
                return true;
        }

        return false;
    }

    private static Type ResolveSchemaIdType(Type schemaType)
    {
        for (var baseType = schemaType; baseType != null; baseType = baseType.BaseType)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Schema<>))
                return baseType.GetGenericArguments()[0];
        }

        throw new ArgumentException($"Type '{schemaType.FullName}' does not inherit from Schema<>.", nameof(schemaType));
    }
}
