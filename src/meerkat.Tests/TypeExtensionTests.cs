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

    [Attributes.Collection(Name = " Order   Item   Details ")]
    private class MultiWordItem : Schema<Guid> { }

    [Fact]
    public void GetCollectionName_ShouldReplaceInternalWhitespaceWithUnderscore()
    {
        typeof(MultiWordItem).GetCollectionName().Must().Be("order_item_details");
    }

    [Fact]
    public void TypeExtensions_ShouldReturnCachedValuesOnSubsequentCalls()
    {
        var first = typeof(MultiWordItem).GetCollectionName();
        var second = typeof(MultiWordItem).GetCollectionName();
        first.Must().Be(second);

        var trackFirst = typeof(CustomUser).ShouldTrackTimestamps();
        var trackSecond = typeof(CustomUser).ShouldTrackTimestamps();
        trackFirst.Must().Be(trackSecond);

        var softDeleteFirst = typeof(SoftDeletedUser).ShouldSoftDelete();
        var softDeleteSecond = typeof(SoftDeletedUser).ShouldSoftDelete();
        softDeleteFirst.Must().Be(softDeleteSecond);
    }

    private class AttributedFieldsAndProps
    {
        [Lowercase]
        public string PropertyField = string.Empty;

        [Lowercase]
        public string Name { get; set; } = string.Empty;

        [Uppercase]
        public string Sku = string.Empty;

        public int Age = 25;
    }

    [Fact]
    public void GetAttributedMembers_ShouldReturnBothPropertiesAndFields()
    {
        var lowercaseMembers = typeof(AttributedFieldsAndProps).GetAttributedMembers<LowercaseAttribute>();
        lowercaseMembers.Must().HaveCount(2);
        lowercaseMembers.Any(x => x.Value.Name == "PropertyField" && x.Key is LowercaseAttribute).Must().BeTrue();
        lowercaseMembers.Any(x => x.Value.Name == "Name" && x.Key is LowercaseAttribute).Must().BeTrue();

        var uppercaseMembers = typeof(AttributedFieldsAndProps).GetAttributedMembers<UppercaseAttribute>();
        uppercaseMembers.Must().HaveCount(1);
        uppercaseMembers[0].Value.Name.Must().Be("Sku");
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
