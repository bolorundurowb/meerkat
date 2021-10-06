using System;
using meerkat.Services;

namespace meerkat.Extensions
{
    internal static class StringExtensions
    {
        public static string Pluralize(this string singular)
        {
            if (string.IsNullOrWhiteSpace(singular))
                return singular;

            return PluralizationService.Pluralize(singular);
        }

        public static string ReplaceLastOccurrence(this string input, string oldValue, string newValue)
        {
            var place = input.LastIndexOf(oldValue, StringComparison.OrdinalIgnoreCase);

            if (place == -1)
                return input;

            return input.Remove(place, oldValue.Length).Insert(place, newValue);
        }
    }
}