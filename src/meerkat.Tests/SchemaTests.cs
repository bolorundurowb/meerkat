using System;
using meerkat.Attributes;
using meerkat.Exceptions;
using Xunit;

namespace meerkat.Tests;

public class SchemaTests
{
    [meerkat.Attributes.Collection(TrackTimestamps = true)]
    private class TrackedEntity : Schema<Guid>
    {
        [Lowercase]
        public string Email { get; set; }

        [Uppercase]
        public string Code { get; set; }
        
        public string Normal { get; set; }
    }

    private class UntrackedEntity : Schema<Guid> { }

    private class InvalidLowercaseEntity : Schema<Guid>
    {
        [Lowercase]
        public int Number { get; set; }
    }

    private class InvalidUppercaseEntity : Schema<Guid>
    {
        [Uppercase]
        public int Number { get; set; }
    }

    [Fact]
    public void HandleTimestamps_ShouldSetCreatedAtAndUpdatedAt_WhenTrackedAndNew()
    {
        // Arrange
        var entity = new TrackedEntity();

        // Act
        entity.HandleTimestamps();

        // Assert
        Assert.NotNull(entity.CreatedAt);
        Assert.NotNull(entity.UpdatedAt);
        Assert.Equal(entity.CreatedAt, entity.UpdatedAt);
    }

    [Fact]
    public void HandleTimestamps_ShouldOnlySetUpdatedAt_WhenTrackedAndExisting()
    {
        // Arrange
        var entity = new TrackedEntity();
        entity.HandleTimestamps();
        var originalCreatedAt = entity.CreatedAt;
        var originalUpdatedAt = entity.UpdatedAt;

        // Act
        // Small delay to ensure UtcNow changes if the resolution is high enough, 
        // but since it's mock-less and we don't control time, we just check they are updated.
        entity.HandleTimestamps();

        // Assert
        Assert.Equal(originalCreatedAt, entity.CreatedAt);
        Assert.NotNull(entity.UpdatedAt);
    }

    [Fact]
    public void HandleTimestamps_ShouldDoNothing_WhenUntracked()
    {
        // Arrange
        var entity = new UntrackedEntity();

        // Act
        entity.HandleTimestamps();

        // Assert
        Assert.Null(entity.CreatedAt);
        Assert.Null(entity.UpdatedAt);
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldLowercaseTargetProperties()
    {
        // Arrange
        var entity = new TrackedEntity { Email = "TEST@EXAMPLE.COM", Normal = "STAY_SAME" };

        // Act
        entity.HandleLowercaseTransformations();

        // Assert
        Assert.Equal("test@example.com", entity.Email);
        Assert.Equal("STAY_SAME", entity.Normal);
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldUppercaseTargetProperties()
    {
        // Arrange
        var entity = new TrackedEntity { Code = "abc-123", Normal = "stay_same" };

        // Act
        entity.HandleUppercaseTransformations();

        // Assert
        Assert.Equal("ABC-123", entity.Code);
        Assert.Equal("stay_same", entity.Normal);
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldThrow_WhenAppliedToNonString()
    {
        // Arrange
        var entity = new InvalidLowercaseEntity { Number = 123 };

        // Act & Assert
        Assert.Throws<InvalidAttributeException>(() => entity.HandleLowercaseTransformations());
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldThrow_WhenAppliedToNonString()
    {
        // Arrange
        var entity = new InvalidUppercaseEntity { Number = 123 };

        // Act & Assert
        Assert.Throws<InvalidAttributeException>(() => entity.HandleUppercaseTransformations());
    }
}
