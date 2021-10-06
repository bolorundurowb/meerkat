using System;
using System.Collections.Generic;
using System.Linq;
using Meerkat.Extensions;

namespace Meerkat.Services
{
    internal static class PluralizationService
    {
        private static readonly List<PluralizationRule> Rules;

        static PluralizationService()
        {
            Rules = new List<PluralizationRule>
            {
                new PluralizationRule("s", false, "th", "ph", "ey"),
                new PluralizationRule("es", false, "o"),
                new PluralizationRule("ves", true, "f", "fe"),
                new PluralizationRule("na", true, "non"),
                new PluralizationRule("ia", true, "ion"),
                new PluralizationRule("es", true, "is"),
                new PluralizationRule("ies", true, "y"),
                new PluralizationRule("i", true, "us"),
                new PluralizationRule("ice", true, "ouse")
            };
        }

        public static string Pluralize(string singular)
        {
            // find matching rule
            var rule = Rules.FirstOrDefault(x => x.IsMatch(singular));

            if (rule == null)
                return $"{singular}s";

            var suffix = rule.Match(singular);
            return rule.Pluralize(suffix, singular);
        }

        private class PluralizationRule
        {
            private string[] Suffixes { get; set; }

            private string PluralizedSuffix { get; set; }

            private bool ReplaceSuffix { get; set; }

            public PluralizationRule(string pluralizedSuffix, bool replaceSuffix, params string[] suffixes)
            {
                PluralizedSuffix = pluralizedSuffix;
                ReplaceSuffix = replaceSuffix;
                Suffixes = suffixes;
            }

            public string Pluralize(string suffix, string input) => ReplaceSuffix
                ? input.ReplaceLastOccurrence(suffix, PluralizedSuffix)
                : $"{input}{PluralizedSuffix}";

            public bool IsMatch(string input) =>
                Suffixes?.Any(x => input.EndsWith(x, StringComparison.InvariantCultureIgnoreCase)) ?? false;

            public string Match(string input) =>
                Suffixes?.FirstOrDefault(x => input.EndsWith(x, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}