using System;
using System.Collections.Generic;
using System.Linq;
using meerkat.Extensions;

namespace meerkat.Services;

internal static class PluralisationService
{
    private static readonly List<PluralisationRule> Rules;

    static PluralisationService() => Rules =
    [
        new("s", false, "th", "ph", "ey", "ay", "oy", "uy"),
        new("es", false, "o", "ch", "sh", "ss", "x"),
        new("ves", true, "f", "fe"),
        new("na", true, "non"),
        new("ia", true, "ion"),
        new("es", true, "is"),
        new("ies", true, "y"),
        new("ice", true, "ouse"),
        new("i", true, "us")
    ];

    public static string Pluralise(string singular)
    {
        // find matching rule
        var rule = Rules.FirstOrDefault(x => x.IsMatch(singular));

        if (rule == null)
            return $"{singular}s";

        var suffix = rule.Match(singular);

        // special case for 'us' ending to prefer 'es' (e.g. bus -> buses) over 'i' (e.g. cactus -> cacti)
        // when 'us' is a short word (common nouns)
        if (suffix == "us" && singular.Length <= 3)
            return $"{singular}es";

        return rule.Pluralise(suffix, singular);
    }

    private class PluralisationRule(string pluralisedSuffix, bool replaceSuffix, params string[] suffixes)
    {
        private string[] Suffixes { get; set; } = suffixes;

        private string PluralisedSuffix { get; set; } = pluralisedSuffix;

        private bool ReplaceSuffix { get; set; } = replaceSuffix;

        public string Pluralise(string suffix, string input) => ReplaceSuffix
            ? input.ReplaceLastOccurrence(suffix, PluralisedSuffix)
            : $"{input}{PluralisedSuffix}";

        public bool IsMatch(string input) =>
            Suffixes.Any(x => input.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));

        public string? Match(string input) =>
            Suffixes.FirstOrDefault(x => input.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
    }
}
