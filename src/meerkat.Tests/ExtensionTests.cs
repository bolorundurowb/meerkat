using meerkat.Extensions;
using OmniAssert;

namespace meerkat.Tests;

public class ExtensionTests
{
    [Theory]
    [InlineData("User", "users")]
    [InlineData("Category", "categories")]
    [InlineData("Bus", "buses")]
    [InlineData("Box", "boxes")]
    [InlineData("Dish", "dishes")]
    [InlineData("Church", "churches")]
    [InlineData("City", "cities")]
    [InlineData("Mouse", "mice")]
    [InlineData("Leaf", "leaves")]
    [InlineData("Life", "lives")]
    [InlineData("Criterion", "criteria")]
    [InlineData("Phenomenon", "phenomena")]
    [InlineData("Thesis", "theses")]
    [InlineData("Cactus", "cacti")]
    [InlineData("Boy", "boys")]
    [InlineData("Key", "keys")]
    [InlineData("Way", "ways")]
    [InlineData("Guy", "guys")]
    public void Pluralize_ShouldReturnCorrectPluralForm(string singular, string expectedPlural)
    {
        singular.Pluralize().Verify().ToBeIgnoringCase(expectedPlural);
    }

    [Fact]
    public void Pluralize_ShouldHandleEmptyOrNullString()
    {
        ((string?)null).Pluralize().Verify().ToBeNull();
        "".Pluralize().Verify().ToBe("");
        "   ".Pluralize().Verify().ToBe("   ");
    }

    [Theory]
    [InlineData("Hello World", "World", "Everyone", "Hello Everyone")]
    [InlineData("Banana", "a", "s", "Banans")]
    [InlineData("TestTest", "Test", "Case", "TestCase")]
    [InlineData("NoMatch", "Matchless", "Something", "NoMatch")]
    public void ReplaceLastOccurrence_ShouldWorkCorrectly(string input, string oldValue, string newValue, string expected)
    {
        input.ReplaceLastOccurrence(oldValue, newValue).Verify().ToBe(expected);
    }
}
