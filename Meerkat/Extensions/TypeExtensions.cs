using System;
using System.Reflection;
using Meerkat.Attributes;

namespace Meerkat.Extensions
{
    internal static class TypeExtensions
    {
        public static string GetCollectionName(this Type type)
        {
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            return collectionAttribute?.Name ?? type.Name.Pluralize();
        }
        
        public static bool ShouldTrackTimestamps(this Type type)
        {
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            return collectionAttribute?.TrackTimestamps ?? false;
        }
    }
}