using System;
using System.Collections.Generic;
using System.Linq;
using meerkat.Extensions;

namespace meerkat.Services;

internal static class PluralizationService
{
    private static readonly List<PluralizationRule> Rules;

    static PluralizationService() => Rules =
        [
            new("s", false, "th", "ph", "ey"),
            new("es", false, "o"),
            new("ves", true, "f", "fe"),
            new("na", true, "non"),
            new("ia", true, "ion"),
            new("es", true, "is"),
            new("ies", true, "y"),
            new("i", true, "us"),
            new("ice", true, "ouse")
        ];

    public static string Pluralize(string singular)
    {
        // find matching rule
        var rule = Rules.FirstOrDefault(x => x.IsMatch(singular));

        if (rule == null)
            return $"{singular}s";

        var suffix = rule.Match(singular);
        return rule.Pluralize(suffix, singular);
    }

    private class PluralizationRule(string pluralizedSuffix, bool replaceSuffix, params string[] suffixes)
    {
        private string[] Suffixes { get; set; } = suffixes;

        private string PluralizedSuffix { get; set; } = pluralizedSuffix;

        private bool ReplaceSuffix { get; set; } = replaceSuffix;

        public string Pluralize(string suffix, string input) => ReplaceSuffix
            ? input.ReplaceLastOccurrence(suffix, PluralizedSuffix)
            : $"{input}{PluralizedSuffix}";

        public bool IsMatch(string input) =>
            Suffixes.Any(x => input.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));

        public string? Match(string input) =>
            Suffixes.FirstOrDefault(x => input.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
    }
}