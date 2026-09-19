using System;
using System.Text.RegularExpressions;
using NodaTime;
using NodaTime.Text;

namespace meerkat.Services;

internal static class DurationParser
{
    private static readonly Regex CompactDurationRegex = new(
        @"^(?:\s*(\d+)\s*(ms|w|d|h|m|s)\s*)+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex TokenRegex = new(
        @"(\d+)\s*(ms|w|d|h|m|s)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static TimeSpan ParseDuration(string? duration)
    {
        if (string.IsNullOrWhiteSpace(duration))
            throw new ArgumentException("Duration cannot be null or empty.", nameof(duration));

        var trimmed = duration!.Trim();

        if (trimmed.StartsWith("P", StringComparison.OrdinalIgnoreCase))
        {
            var isoResult = PeriodPattern.NormalizingIso.Parse(trimmed);
            if (isoResult.Success)
            {
                return isoResult.Value.ToDuration().ToTimeSpan();
            }

            var roundtripResult = PeriodPattern.Roundtrip.Parse(trimmed);
            if (roundtripResult.Success)
            {
                return roundtripResult.Value.ToDuration().ToTimeSpan();
            }

            throw new FormatException($"Invalid ISO-8601 duration format: '{duration}'.");
        }

        if (!CompactDurationRegex.IsMatch(trimmed))
            throw new FormatException($"Invalid duration format: '{duration}'. Supported tokens are ms, s, m, h, d, w or ISO-8601 duration.");

        var totalDuration = Duration.Zero;
        var matches = TokenRegex.Matches(trimmed);

        foreach (Match match in matches)
        {
            var value = long.Parse(match.Groups[1].Value);
            var unit = match.Groups[2].Value.ToLowerInvariant();

            totalDuration += unit switch
            {
                "ms" => Duration.FromMilliseconds(value),
                "s" => Duration.FromSeconds(value),
                "m" => Duration.FromMinutes(value),
                "h" => Duration.FromHours(value),
                "d" => Duration.FromDays(value),
                "w" => Duration.FromDays(value * 7),
                _ => throw new FormatException($"Unsupported duration unit: '{unit}'.")
            };
        }

        return totalDuration.ToTimeSpan();
    }
}
