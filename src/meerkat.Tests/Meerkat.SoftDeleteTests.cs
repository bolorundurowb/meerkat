using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using OmniAssert;
using Xunit;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatSoftDeleteTests
{
    private class NonSoftDeleteEntity : Schema<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    [meerkat.Attributes.Collection(SoftDelete = true, TrackTimestamps = false)]
    private class SoftDeleteUntrackedEntity : Schema<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    [meerkat.Attributes.Collection(SoftDelete = true, TrackTimestamps = true)]
    private class SoftDeleteTrackedEntity : Schema<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void ApplySoftDeleteFilter_ShouldReturnOriginalFilter_WhenSoftDeleteDisabled()
    {
        var filter = Builders<NonSoftDeleteEntity>.Filter.Eq(x => x.Name, "test");
        var result = Meerkat.ApplySoftDeleteFilter<NonSoftDeleteEntity, Guid>(filter);
        result.Must().Be(filter);
    }

    [Fact]
    public void ApplySoftDeleteFilter_ShouldReturnEmpty_WhenSoftDeleteDisabledAndFilterIsNull()
    {
        var result = Meerkat.ApplySoftDeleteFilter<NonSoftDeleteEntity, Guid>(null!);
        result.Must().Be(FilterDefinition<NonSoftDeleteEntity>.Empty);
    }

    [Fact]
    public void ApplySoftDeleteFilter_ShouldReturnOriginalFilter_WhenIncludeDeletedIsTrue()
    {
        var filter = Builders<SoftDeleteUntrackedEntity>.Filter.Eq(x => x.Name, "test");
        var result = Meerkat.ApplySoftDeleteFilter<SoftDeleteUntrackedEntity, Guid>(filter, includeDeleted: true);
        result.Must().Be(filter);
    }

    [Fact]
    public void ApplySoftDeleteFilter_ShouldReturnNotDeletedFilter_WhenFilterIsNull()
    {
        var result = Meerkat.ApplySoftDeleteFilter<SoftDeleteUntrackedEntity, Guid>(null!, includeDeleted: false);
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var serializer = serializerRegistry.GetSerializer<SoftDeleteUntrackedEntity>();
        var rendered = result.Render(new RenderArgs<SoftDeleteUntrackedEntity>(serializer, serializerRegistry));

        rendered.Contains("DeletedAt").Must().BeTrue();
        rendered["DeletedAt"].BsonType.Must().Be(BsonType.Null);
    }

    [Fact]
    public void ApplySoftDeleteFilter_ShouldCombineWithNotDeletedFilter_WhenFilterIsNotNull()
    {
        var filter = Builders<SoftDeleteUntrackedEntity>.Filter.Eq(x => x.Name, "test");
        var result = Meerkat.ApplySoftDeleteFilter<SoftDeleteUntrackedEntity, Guid>(filter, includeDeleted: false);
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var serializer = serializerRegistry.GetSerializer<SoftDeleteUntrackedEntity>();
        var rendered = result.Render(new RenderArgs<SoftDeleteUntrackedEntity>(serializer, serializerRegistry));

        rendered.Contains("Name").Must().BeTrue();
        rendered["Name"].AsString.Must().Be("test");
        rendered.Contains("DeletedAt").Must().BeTrue();
        rendered["DeletedAt"].BsonType.Must().Be(BsonType.Null);
    }

    [Fact]
    public void BuildSoftDeleteUpdate_ShouldIncludeOnlyDeletedAt_WhenNotTracked()
    {
        var update = Meerkat.BuildSoftDeleteUpdate<SoftDeleteUntrackedEntity, Guid>();
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var serializer = serializerRegistry.GetSerializer<SoftDeleteUntrackedEntity>();
        var rendered = update.Render(new RenderArgs<SoftDeleteUntrackedEntity>(serializer, serializerRegistry)).AsBsonDocument;

        rendered.Contains("$set").Must().BeTrue();
        var setDoc = rendered["$set"].AsBsonDocument;
        setDoc.Contains("DeletedAt").Must().BeTrue();
        setDoc.Contains("UpdatedAt").Must().BeFalse();
    }

    [Fact]
    public void BuildSoftDeleteUpdate_ShouldIncludeDeletedAtAndUpdatedAt_WhenTracked()
    {
        var update = Meerkat.BuildSoftDeleteUpdate<SoftDeleteTrackedEntity, Guid>();
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var serializer = serializerRegistry.GetSerializer<SoftDeleteTrackedEntity>();
        var rendered = update.Render(new RenderArgs<SoftDeleteTrackedEntity>(serializer, serializerRegistry)).AsBsonDocument;

        rendered.Contains("$set").Must().BeTrue();
        var setDoc = rendered["$set"].AsBsonDocument;
        setDoc.Contains("DeletedAt").Must().BeTrue();
        setDoc.Contains("UpdatedAt").Must().BeTrue();
    }

    [Fact]
    public void BuildRestoreUpdate_ShouldUnsetDeletedAt_WhenNotTracked()
    {
        var update = Meerkat.BuildRestoreUpdate<SoftDeleteUntrackedEntity, Guid>();
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var serializer = serializerRegistry.GetSerializer<SoftDeleteUntrackedEntity>();
        var rendered = update.Render(new RenderArgs<SoftDeleteUntrackedEntity>(serializer, serializerRegistry)).AsBsonDocument;

        rendered.Contains("$unset").Must().BeTrue();
        var unsetDoc = rendered["$unset"].AsBsonDocument;
        unsetDoc.Contains("DeletedAt").Must().BeTrue();
        rendered.Contains("$set").Must().BeFalse();
    }

    [Fact]
    public void BuildRestoreUpdate_ShouldUnsetDeletedAtAndSetUpdatedAt_WhenTracked()
    {
        var update = Meerkat.BuildRestoreUpdate<SoftDeleteTrackedEntity, Guid>();
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var serializer = serializerRegistry.GetSerializer<SoftDeleteTrackedEntity>();
        var rendered = update.Render(new RenderArgs<SoftDeleteTrackedEntity>(serializer, serializerRegistry)).AsBsonDocument;

        rendered.Contains("$unset").Must().BeTrue();
        var unsetDoc = rendered["$unset"].AsBsonDocument;
        unsetDoc.Contains("DeletedAt").Must().BeTrue();
        rendered.Contains("$set").Must().BeTrue();
        var setDoc = rendered["$set"].AsBsonDocument;
        setDoc.Contains("UpdatedAt").Must().BeTrue();
    }
}
