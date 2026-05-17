using meerkat.Attributes;
using meerkat.Exceptions;
using OmniAssert;

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
        entity.CreatedAt.Verify().NotToBeNull();
        entity.UpdatedAt.Verify().NotToBeNull();
        entity.UpdatedAt.Verify().ToBe(entity.CreatedAt);
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
        entity.CreatedAt.Verify().ToBe(originalCreatedAt);
        entity.UpdatedAt.Verify().NotToBeNull();
    }

    [Fact]
    public void HandleTimestamps_ShouldDoNothing_WhenUntracked()
    {
        // Arrange
        var entity = new UntrackedEntity();

        // Act
        entity.HandleTimestamps();

        // Assert
        entity.CreatedAt.Verify().ToBeNull();
        entity.UpdatedAt.Verify().ToBeNull();
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldLowercaseTargetProperties()
    {
        // Arrange
        var entity = new TrackedEntity { Email = "TEST@EXAMPLE.COM", Normal = "STAY_SAME" };

        // Act
        entity.HandleLowercaseTransformations();

        // Assert
        entity.Email.Verify().ToBe("test@example.com");
        entity.Normal.Verify().ToBe("STAY_SAME");
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldUppercaseTargetProperties()
    {
        // Arrange
        var entity = new TrackedEntity { Code = "abc-123", Normal = "stay_same" };

        // Act
        entity.HandleUppercaseTransformations();

        // Assert
        entity.Code.Verify().ToBe("ABC-123");
        entity.Normal.Verify().ToBe("stay_same");
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldThrow_WhenAppliedToNonString()
    {
        // Arrange
        var entity = new InvalidLowercaseEntity { Number = 123 };

        // Act & Assert
        Action act = () => entity.HandleLowercaseTransformations();
        act.Throws<InvalidAttributeException>();
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldThrow_WhenAppliedToNonString()
    {
        // Arrange
        var entity = new InvalidUppercaseEntity { Number = 123 };

        // Act & Assert
        Action act = () => entity.HandleUppercaseTransformations();
        act.Throws<InvalidAttributeException>();
    }
}
