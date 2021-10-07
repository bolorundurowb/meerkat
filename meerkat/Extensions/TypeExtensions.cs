using System;
using System.Reflection;
using System.Text.RegularExpressions;
using meerkat.Attributes;

namespace meerkat.Extensions
{
    internal static class TypeExtensions
    {
        private static readonly Regex Whitespace = new Regex("\\s+", RegexOptions.Compiled);
        
        public static string GetCollectionName(this Type type)
        {
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            var attributeName = collectionAttribute?.Name;
            var name = string.IsNullOrWhiteSpace(attributeName) ? type.Name.Pluralize() : attributeName;
            return Whitespace.Replace(name.ToLowerInvariant(), "_");
        }
        
        public static bool ShouldTrackTimestamps(this Type type)
        {
            var collectionAttribute = type.GetCustomAttribute<CollectionAttribute>();
            return collectionAttribute?.TrackTimestamps ?? false;
        }
    }
}