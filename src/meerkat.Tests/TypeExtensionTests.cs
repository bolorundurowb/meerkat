using meerkat.Attributes;
using meerkat.Extensions;
using OmniAssert;

namespace meerkat.Tests;

public class TypeExtensionTests
{
    [meerkat.Attributes.Collection(Name = "custom_users", TrackTimestamps = true)]
    private class CustomUser : Schema<Guid> { }

    private class Product : Schema<Guid> { }

    [meerkat.Attributes.Collection(Name = "  Orders  ")]
    private class Order : Schema<Guid> { }

    [Fact]
    public void GetCollectionName_ShouldReturnPluralizedLowercaseName_WhenNoAttribute()
    {
        // Act
        var name = typeof(Product).GetCollectionName();

        // Assert
        name.Verify().ToBe("products");
    }

    [Fact]
    public void GetCollectionName_ShouldReturnAttributeName_WhenAttributePresent()
    {
        // Act
        var name = typeof(CustomUser).GetCollectionName();

        // Assert
        name.Verify().ToBe("custom_users");
    }

    [Fact]
    public void GetCollectionName_ShouldHandleWhitespaceAndLowercase_WhenAttributePresent()
    {
        // Act
        var name = typeof(Order).GetCollectionName();

        // Assert
        name.Verify().ToBe("orders");
    }

    [Fact]
    public void ShouldTrackTimestamps_ShouldReturnTrue_WhenAttributeTracks()
    {
        // Act & Assert
        typeof(CustomUser).ShouldTrackTimestamps().Verify().ToBeTrue();
    }

    [Fact]
    public void ShouldTrackTimestamps_ShouldReturnFalse_WhenAttributeDoesNotTrackOrMissing()
    {
        // Act & Assert
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
        // Act
        var lowercaseProps = typeof(AttributedClass).AttributedWith<LowercaseAttribute>().ToList();
        var uppercaseProps = typeof(AttributedClass).AttributedWith<UppercaseAttribute>().ToList();

        // Assert
        lowercaseProps.Verify().HasCount(1);
        lowercaseProps[0].Name.Verify().ToBe("Name");
        uppercaseProps.Verify().HasCount(1);
        uppercaseProps[0].Name.Verify().ToBe("Sku");
    }
}
