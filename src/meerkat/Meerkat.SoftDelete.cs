using System;
using System.Collections.Generic;
using meerkat.Extensions;
using MongoDB.Driver;

namespace meerkat;

public static partial class Meerkat
{
    /// <summary>
    /// Applies the not-deleted filter when the schema uses soft delete and <paramref name="includeDeleted"/> is false.
    /// </summary>
    internal static FilterDefinition<TSchema> ApplySoftDeleteFilter<TSchema, TId>(
        FilterDefinition<TSchema> filter, bool includeDeleted = false)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        if (!typeof(TSchema).ShouldSoftDelete() || includeDeleted)
            return filter ?? FilterDefinition<TSchema>.Empty;

        var notDeleted = Builders<TSchema>.Filter.Eq(x => x.DeletedAt, null);
        if (filter == null)
            return notDeleted;

        return Builders<TSchema>.Filter.And(filter, notDeleted);
    }

    internal static UpdateDefinition<TSchema> BuildSoftDeleteUpdate<TSchema, TId>(Type? runtimeType = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var type = runtimeType ?? typeof(TSchema);
        var updates = new List<UpdateDefinition<TSchema>>
        {
            Builders<TSchema>.Update.Set(x => x.DeletedAt, DateTimeOffset.UtcNow)
        };

        if (type.ShouldTrackTimestamps())
            updates.Add(Builders<TSchema>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow));

        return Builders<TSchema>.Update.Combine(updates);
    }

    internal static UpdateDefinition<TSchema> BuildRestoreUpdate<TSchema, TId>(Type? runtimeType = null)
        where TSchema : Schema<TId> where TId : IEquatable<TId>
    {
        var type = runtimeType ?? typeof(TSchema);
        var updates = new List<UpdateDefinition<TSchema>>
        {
            Builders<TSchema>.Update.Unset(x => x.DeletedAt)
        };

        if (type.ShouldTrackTimestamps())
            updates.Add(Builders<TSchema>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow));

        return Builders<TSchema>.Update.Combine(updates);
    }
}
