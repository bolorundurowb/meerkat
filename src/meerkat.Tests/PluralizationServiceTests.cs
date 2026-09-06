using meerkat.Services;
using OmniAssert;

namespace meerkat.Tests;

[Collection("MeerkatUnitTests")]
public class PluralizationServiceTests
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
        PluralizationService.Pluralize(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Theory]
    [InlineData("Louse", "lice")]
    [InlineData("Mongoose", "mongooses")]
    public void Pluralize_OuseRule_ShouldWorkCorrectly(string singular, string expectedPlural)
    {
        PluralizationService.Pluralize(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Theory]
    [InlineData("Hero", "heroes")]
    [InlineData("Tomato", "tomatoes")]
    [InlineData("Wish", "wishes")]
    [InlineData("Kiss", "kisses")]
    [InlineData("Fox", "foxes")]
    public void Pluralize_EsSuffix_ShouldApplyToEndings(string singular, string expectedPlural)
    {
        PluralizationService.Pluralize(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Theory]
    [InlineData("Knife", "knives")]
    [InlineData("Wolf", "wolves")]
    [InlineData("Shelf", "shelves")]
    public void Pluralize_VesSuffix_ShouldReplaceFOrFe(string singular, string expectedPlural)
    {
        PluralizationService.Pluralize(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Fact]
    public void Pluralize_ShortUsWord_ShouldAddEs()
    {
        PluralizationService.Pluralize("bus").Must().Be("buses");
    }

    [Fact]
    public void Pluralize_NoMatchingRule_ShouldAddS()
    {
        PluralizationService.Pluralize("dog").Must().Be("dogs");
        PluralizationService.Pluralize("cat").Must().Be("cats");
        PluralizationService.Pluralize("computer").Must().Be("computers");
    }

    [Fact]
    public void Pluralize_IsCaseInsensitive()
    {
        PluralizationService.Pluralize("city").Must().Be("cities");
        PluralizationService.Pluralize("City").Must().Be("Cities");
        PluralizationService.Pluralize("CITY").Must().Be("CITies");
    }

    [Theory]
    [InlineData("smith", "smiths")]
    [InlineData("pharaoh", "pharaohs")]
    [InlineData("holiday", "holidays")]
    [InlineData("envoy", "envoys")]
    [InlineData("buy", "buys")]
    [InlineData("donkey", "donkeys")]
    public void Pluralize_SimpleS_ShouldApplyToEndings(string singular, string expectedPlural)
    {
        PluralizationService.Pluralize(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Theory]
    [InlineData("Analysis", "analyses")]
    [InlineData("Diagnosis", "diagnoses")]
    public void Pluralize_IsToEs_ShouldReplaceIsEnding(string singular, string expectedPlural)
    {
        PluralizationService.Pluralize(singular).Must().BeIgnoringCase(expectedPlural);
    }
}
