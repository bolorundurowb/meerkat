using System;
using System.Reflection;
using Meerkat.Attributes;

namespace Meerkat.Extensions
{
    internal static class TypeExtensions
    {
        public static string GetName(this Type type)
        {
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            return collectionAttribute?.Name ?? type.Name;
        }
        
        public static bool ShouldTrackTimestamps(this Type type)
        {
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            return collectionAttribute?.TrackTimestamps ?? false;
        }
    }
}