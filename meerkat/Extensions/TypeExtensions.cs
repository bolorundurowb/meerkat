using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using meerkat.Attributes;

namespace meerkat.Extensions
{
    internal static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<string, string> CollectionNameCache = new ConcurrentDictionary<string, string>();
        private static readonly ConcurrentDictionary<string, bool> TimestampTrackCache = new ConcurrentDictionary<string, bool>();
        private static readonly Regex Whitespace = new Regex("\\s+", RegexOptions.Compiled);
        
        public static string GetCollectionName(this Type type)
        {
            var cacheKey = type.FullName ?? type.Name;

            if (CollectionNameCache.ContainsKey(cacheKey))
                return CollectionNameCache[cacheKey];
            
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            var attributeName = collectionAttribute?.Name;
            var name = string.IsNullOrWhiteSpace(attributeName) ? type.Name.Pluralize() : attributeName;
            var collectionName = Whitespace.Replace(name.ToLowerInvariant(), "_");
            
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
            var shouldTrack =  collectionAttribute?.TrackTimestamps ?? false;
            
            // cache the tracking option
            TimestampTrackCache[cacheKey] = shouldTrack;

            return shouldTrack;
        }
    }
}