using meerkat.Attributes;
using meerkat.Extensions;

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
        Assert.Equal("products", name);
    }

    [Fact]
    public void GetCollectionName_ShouldReturnAttributeName_WhenAttributePresent()
    {
        // Act
        var name = typeof(CustomUser).GetCollectionName();

        // Assert
        Assert.Equal("custom_users", name);
    }

    [Fact]
    public void GetCollectionName_ShouldHandleWhitespaceAndLowercase_WhenAttributePresent()
    {
        // Act
        var name = typeof(Order).GetCollectionName();

        // Assert
        Assert.Equal("orders", name);
    }

    [Fact]
    public void ShouldTrackTimestamps_ShouldReturnTrue_WhenAttributeTracks()
    {
        // Act & Assert
        Assert.True(typeof(CustomUser).ShouldTrackTimestamps());
    }

    [Fact]
    public void ShouldTrackTimestamps_ShouldReturnFalse_WhenAttributeDoesNotTrackOrMissing()
    {
        // Act & Assert
        Assert.False(typeof(Product).ShouldTrackTimestamps());
        Assert.False(typeof(Order).ShouldTrackTimestamps());
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
        Assert.Single(lowercaseProps);
        Assert.Equal("Name", lowercaseProps[0].Name);
        Assert.Single(uppercaseProps);
        Assert.Equal("Sku", uppercaseProps[0].Name);
    }
}
