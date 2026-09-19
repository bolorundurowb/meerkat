using System;
using meerkat.Services;
using OmniAssert;
using Xunit;

namespace meerkat.Tests;

public class DurationParserTests
{
    [Theory]
    [InlineData("30d", 30, 0, 0, 0)]
    [InlineData("12h", 0, 12, 0, 0)]
    [InlineData("45m", 0, 0, 45, 0)]
    [InlineData("90s", 0, 0, 0, 90)]
    [InlineData("500ms", 0, 0, 0, 0, 500)]
    [InlineData("2w", 14, 0, 0, 0)]
    [InlineData("1d 12h", 1, 12, 0, 0)]
    [InlineData("1d12h", 1, 12, 0, 0)]
    [InlineData("2h 30m 15s", 0, 2, 30, 15)]
    [InlineData("10D", 10, 0, 0, 0)]
    [InlineData("5H", 0, 5, 0, 0)]
    [InlineData("20M", 0, 0, 20, 0)]
    [InlineData("30S", 0, 0, 0, 30)]
    [InlineData("100MS", 0, 0, 0, 0, 100)]
    [InlineData("1W", 7, 0, 0, 0)]
    public void ParseDuration_ShouldParseCompactTokens(string input, int days, int hours, int minutes, int seconds, int milliseconds = 0)
    {
        var expected = new TimeSpan(days, hours, minutes, seconds, milliseconds);
        var result = DurationParser.ParseDuration(input);
        result.Must().Be(expected);
    }

    [Theory]
    [InlineData("P30D", 30, 0, 0, 0)]
    [InlineData("PT12H", 0, 12, 0, 0)]
    [InlineData("PT45M", 0, 0, 45, 0)]
    [InlineData("PT90S", 0, 0, 0, 90)]
    [InlineData("P1DT12H", 1, 12, 0, 0)]
    [InlineData("P2W", 14, 0, 0, 0)]
    public void ParseDuration_ShouldParseIso8601Durations(string input, int days, int hours, int minutes, int seconds)
    {
        var expected = new TimeSpan(days, hours, minutes, seconds);
        var result = DurationParser.ParseDuration(input);
        result.Must().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseDuration_ShouldThrow_WhenNullOrEmpty(string? input)
    {
        Action act = () => DurationParser.ParseDuration(input);
        act.Throws<ArgumentException>();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("abc")]
    [InlineData("30")]
    [InlineData("-5s")]
    [InlineData("10x")]
    [InlineData("Pinvalid")]
    public void ParseDuration_ShouldThrow_WhenFormatIsInvalid(string input)
    {
        Action act = () => DurationParser.ParseDuration(input);
        act.Throws<FormatException>();
    }
}
