using meerkat.Extensions;

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
        // Act
        var result = singular.Pluralize();

        // Assert
        Assert.Equal(expectedPlural, result, ignoreCase: true);
    }

    [Fact]
    public void Pluralize_ShouldHandleEmptyOrNullString()
    {
        // Act & Assert
        Assert.Null(((string?)null).Pluralize());
        Assert.Equal("", "".Pluralize());
        Assert.Equal("   ", "   ".Pluralize());
    }

    [Theory]
    [InlineData("Hello World", "World", "Everyone", "Hello Everyone")]
    [InlineData("Banana", "a", "s", "Banans")] // Wait, ReplaceLastOccurrence for Banana 'a' -> 's' should be 'Banans'? Let's check logic.
    [InlineData("TestTest", "Test", "Case", "TestCase")]
    [InlineData("NoMatch", "Matchless", "Something", "NoMatch")]
    public void ReplaceLastOccurrence_ShouldWorkCorrectly(string input, string oldValue, string newValue, string expected)
    {
        // Act
        var result = input.ReplaceLastOccurrence(oldValue, newValue);

        // Assert
        Assert.Equal(expected, result);
    }
}
