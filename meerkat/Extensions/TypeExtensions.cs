using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using meerkat.Attributes;

namespace meerkat.Extensions;

internal static class TypeExtensions
{
    private static readonly ConcurrentDictionary<string, string> CollectionNameCache = new();

    private static readonly ConcurrentDictionary<string, bool> TimestampTrackCache = new();

    private static readonly Regex Whitespace = new("\\s+", RegexOptions.Compiled);

    public static string GetCollectionName(this Type type)
    {
        var cacheKey = type.FullName ?? type.Name;

        if (CollectionNameCache.TryGetValue(cacheKey, out var collectionName))
            return collectionName;

        var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
        var attributeName = collectionAttribute?.Name;
        var name = string.IsNullOrWhiteSpace(attributeName) ? type.Name.Pluralize() : attributeName;
        collectionName = Whitespace.Replace(name.ToLowerInvariant(), "_");

        // cache this generated name
        CollectionNameCache[cacheKey] = collectionName;

        return collectionName;
    }

    public static bool ShouldTrackTimestamps(this Type type)
    {
        var cacheKey = type.FullName ?? type.Name;

        if (TimestampTrackCache.ContainsKey(cacheKey))
            return TimestampTrackCache[cacheKey];

        var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
        var shouldTrack = collectionAttribute?.TrackTimestamps ?? false;

        // cache the tracking option
        TimestampTrackCache[cacheKey] = shouldTrack;

        return shouldTrack;
    }

    public static IEnumerable<PropertyInfo> AttributedWith<TAttribute>(this Type type) where TAttribute : Attribute
    {
        var attributeType = typeof(TAttribute);
        return type.GetProperties()
            .Where(x => x.CustomAttributes.Any(y => y.AttributeType == attributeType));
    }
}