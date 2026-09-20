using meerkat.Services;
using OmniAssert;

namespace meerkat.Tests;

[Collection("MeerkatUnitTests")]
public class PluralisationServiceTests
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
    public void Pluralise_ShouldReturnCorrectPluralForm(string singular, string expectedPlural)
    {
        PluralisationService.Pluralise(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Theory]
    [InlineData("Louse", "lice")]
    [InlineData("Mongoose", "mongooses")]
    public void Pluralise_OuseRule_ShouldWorkCorrectly(string singular, string expectedPlural)
    {
        PluralisationService.Pluralise(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Theory]
    [InlineData("Hero", "heroes")]
    [InlineData("Tomato", "tomatoes")]
    [InlineData("Wish", "wishes")]
    [InlineData("Kiss", "kisses")]
    [InlineData("Fox", "foxes")]
    public void Pluralise_EsSuffix_ShouldApplyToEndings(string singular, string expectedPlural)
    {
        PluralisationService.Pluralise(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Theory]
    [InlineData("Knife", "knives")]
    [InlineData("Wolf", "wolves")]
    [InlineData("Shelf", "shelves")]
    public void Pluralise_VesSuffix_ShouldReplaceFOrFe(string singular, string expectedPlural)
    {
        PluralisationService.Pluralise(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Fact]
    public void Pluralise_ShortUsWord_ShouldAddEs()
    {
        PluralisationService.Pluralise("bus").Must().Be("buses");
    }

    [Fact]
    public void Pluralise_NoMatchingRule_ShouldAddS()
    {
        PluralisationService.Pluralise("dog").Must().Be("dogs");
        PluralisationService.Pluralise("cat").Must().Be("cats");
        PluralisationService.Pluralise("computer").Must().Be("computers");
    }

    [Fact]
    public void Pluralise_IsCaseInsensitive()
    {
        PluralisationService.Pluralise("city").Must().Be("cities");
        PluralisationService.Pluralise("City").Must().Be("Cities");
        PluralisationService.Pluralise("CITY").Must().Be("CITies");
    }

    [Theory]
    [InlineData("smith", "smiths")]
    [InlineData("pharaoh", "pharaohs")]
    [InlineData("holiday", "holidays")]
    [InlineData("envoy", "envoys")]
    [InlineData("buy", "buys")]
    [InlineData("donkey", "donkeys")]
    public void Pluralise_SimpleS_ShouldApplyToEndings(string singular, string expectedPlural)
    {
        PluralisationService.Pluralise(singular).Must().BeIgnoringCase(expectedPlural);
    }

    [Theory]
    [InlineData("Analysis", "analyses")]
    [InlineData("Diagnosis", "diagnoses")]
    public void Pluralise_IsToEs_ShouldReplaceIsEnding(string singular, string expectedPlural)
    {
        PluralisationService.Pluralise(singular).Must().BeIgnoringCase(expectedPlural);
    }
}
