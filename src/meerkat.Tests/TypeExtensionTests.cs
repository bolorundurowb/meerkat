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
        typeof(Product).GetCollectionName().Must().Be("products");
    }

    [Fact]
    public void GetCollectionName_ShouldReturnAttributeName_WhenAttributePresent()
    {
        typeof(CustomUser).GetCollectionName().Must().Be("custom_users");
    }

    [Fact]
    public void GetCollectionName_ShouldHandleWhitespaceAndLowercase_WhenAttributePresent()
    {
        typeof(Order).GetCollectionName().Must().Be("orders");
    }

    [Fact]
    public void ShouldTrackTimestamps_ShouldReturnTrue_WhenAttributeTracks()
    {
        typeof(CustomUser).ShouldTrackTimestamps().Must().BeTrue();
    }

    [Fact]
    public void ShouldTrackTimestamps_ShouldReturnFalse_WhenAttributeDoesNotTrackOrMissing()
    {
        typeof(Product).ShouldTrackTimestamps().Must().BeFalse();
        typeof(Order).ShouldTrackTimestamps().Must().BeFalse();
    }

    [Attributes.Collection(SoftDelete = true)]
    private class SoftDeletedUser : Schema<Guid> { }

    [Fact]
    public void ShouldSoftDelete_ShouldReturnTrue_WhenAttributeEnablesSoftDelete()
    {
        typeof(SoftDeletedUser).ShouldSoftDelete().Must().BeTrue();
    }

    [Fact]
    public void ShouldSoftDelete_ShouldReturnFalse_WhenAttributeMissingOrDisabled()
    {
        typeof(Product).ShouldSoftDelete().Must().BeFalse();
        typeof(CustomUser).ShouldSoftDelete().Must().BeFalse();
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
        lowercaseProps.Must().HaveCount(1);
        lowercaseProps[0].Name.Must().Be("Name");
        uppercaseProps.Must().HaveCount(1);
        uppercaseProps[0].Name.Must().Be("Sku");
    }
}
