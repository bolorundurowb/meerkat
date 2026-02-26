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

        if (string.IsNullOrWhiteSpace(name)) 
            throw new ArgumentNullException(nameof(name), "Failed to generate a collection name for the provided type.");

        collectionName = Whitespace.Replace(name!.ToLowerInvariant(), "_");

        // cache this generated name
        CollectionNameCache[cacheKey] = collectionName;

        return collectionName;
    }

    public static bool ShouldTrackTimestamps(this Type type)
    {
        var cacheKey = type.FullName ?? type.Name;

        if (TimestampTrackCache.TryGetValue(cacheKey, out var trackTimestamps))
            return trackTimestamps;

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

    public static List<KeyValuePair<TAttribute, MemberInfo>> GetAttributedMembers<TAttribute>(this Type type) where TAttribute : Attribute
    {
        const BindingFlags eligibleBindingFlags = BindingFlags.Public | BindingFlags.Instance;
        var properties = type.GetProperties(eligibleBindingFlags);
        var fields = type.GetFields(eligibleBindingFlags);
        var allMembers = properties.Concat(fields.Cast<MemberInfo>());
        var attributeMemberPairs = new List<KeyValuePair<TAttribute, MemberInfo>>();

        foreach (var member in allMembers)
        {
            // only one attribute of a given type is allowed per member
            var attribute = member.GetCustomAttribute<TAttribute>( false);

            if (attribute != null) 
                attributeMemberPairs.Add(new KeyValuePair<TAttribute, MemberInfo>(attribute, member));
        }

        return attributeMemberPairs;
    }
}