using System;
using System.Reflection;
using meerkat.Attributes;

namespace meerkat.Extensions
{
    internal static class TypeExtensions
    {
        public static string GetCollectionName(this Type type)
        {
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            return (collectionAttribute?.Name ?? type.Name.Pluralize()).ToLowerInvariant();
        }
        
        public static bool ShouldTrackTimestamps(this Type type)
        {
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            return collectionAttribute?.TrackTimestamps ?? false;
        }
    }
}