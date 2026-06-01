using meerkat.Attributes;
using meerkat.Extensions;
using OmniAssert;

namespace meerkat.Tests;

public class TypeExtensionTests
{
    [Attributes.Collection(Name = "custom_users", TrackTimestamps = true)]
    private class CustomUser : Schema<Guid> { }

    private class Product : Schema<Guid> { }

    [Attributes.Collection(Name = "  Orders  ")]
    private class Order : Schema<Guid> { }

    [Fact]
    public void GetCollectionName_ShouldReturnPluralizedLowercaseName_WhenNoAttribute()
    {
        typeof(Product).GetCollectionName().Verify().ToBe("products");
    }

    [Fact]
    public void GetCollectionName_ShouldReturnAttributeName_WhenAttributePresent()
    {
        typeof(CustomUser).GetCollectionName().Verify().ToBe("custom_users");
    }

    [Fact]
    public void GetCollectionName_ShouldHandleWhitespaceAndLowercase_WhenAttributePresent()
    {
        typeof(Order).GetCollectionName().Verify().ToBe("orders");
    }

    [Fact]
    public void ShouldTrackTimestamps_ShouldReturnTrue_WhenAttributeTracks()
    {
        typeof(CustomUser).ShouldTrackTimestamps().Verify().ToBeTrue();
    }

    [Fact]
    public void ShouldTrackTimestamps_ShouldReturnFalse_WhenAttributeDoesNotTrackOrMissing()
    {
        typeof(Product).ShouldTrackTimestamps().Verify().ToBeFalse();
        typeof(Order).ShouldTrackTimestamps().Verify().ToBeFalse();
    }

    private class AttributedClass
    {
        [Lowercase]
        public string Name { get; set; }

        [Uppercase]
        public string Sku { get; set; }

        public int Age { get; set; }
    }

    [Fact]
    public void AttributedWith_ShouldReturnCorrectProperties()
    {
        var lowercaseProps = typeof(AttributedClass).AttributedWith<LowercaseAttribute>().ToList();
        var uppercaseProps = typeof(AttributedClass).AttributedWith<UppercaseAttribute>().ToList();
        lowercaseProps.Verify().ToHaveCount(1);
        lowercaseProps[0].Name.Verify().ToBe("Name");
        uppercaseProps.Verify().ToHaveCount(1);
        uppercaseProps[0].Name.Verify().ToBe("Sku");
    }
}
