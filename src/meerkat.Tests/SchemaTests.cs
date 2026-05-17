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
        var entity = new TrackedEntity();
        entity.HandleTimestamps();
        entity.CreatedAt.Verify().NotToBeNull();
        entity.UpdatedAt.Verify().NotToBeNull();
        entity.UpdatedAt.Verify().ToBe(entity.CreatedAt);
    }

    [Fact]
    public void HandleTimestamps_ShouldOnlySetUpdatedAt_WhenTrackedAndExisting()
    {
        var entity = new TrackedEntity();
        entity.HandleTimestamps();
        var originalCreatedAt = entity.CreatedAt;
        entity.HandleTimestamps();
        entity.CreatedAt.Verify().ToBe(originalCreatedAt);
        entity.UpdatedAt.Verify().NotToBeNull();
    }

    [Fact]
    public void HandleTimestamps_ShouldDoNothing_WhenUntracked()
    {
        var entity = new UntrackedEntity();
        entity.HandleTimestamps();
        entity.CreatedAt.Verify().ToBeNull();
        entity.UpdatedAt.Verify().ToBeNull();
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldLowercaseTargetProperties()
    {
        var entity = new TrackedEntity { Email = "TEST@EXAMPLE.COM", Normal = "STAY_SAME" };
        entity.HandleLowercaseTransformations();
        entity.Email.Verify().ToBe("test@example.com");
        entity.Normal.Verify().ToBe("STAY_SAME");
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldUppercaseTargetProperties()
    {
        var entity = new TrackedEntity { Code = "abc-123", Normal = "stay_same" };
        entity.HandleUppercaseTransformations();
        entity.Code.Verify().ToBe("ABC-123");
        entity.Normal.Verify().ToBe("stay_same");
    }

    [Fact]
    public void HandleLowercaseTransformations_ShouldThrow_WhenAppliedToNonString()
    {
        var entity = new InvalidLowercaseEntity { Number = 123 };
        var act = () => entity.HandleLowercaseTransformations();
        act.Throws<InvalidAttributeException>();
    }

    [Fact]
    public void HandleUppercaseTransformations_ShouldThrow_WhenAppliedToNonString()
    {
        var entity = new InvalidUppercaseEntity { Number = 123 };
        var act = () => entity.HandleUppercaseTransformations();
        act.Throws<InvalidAttributeException>();
    }
}
