using System;
using System.Linq.Expressions;
using meerkat.Updates;
using MongoDB.Driver;

namespace meerkat;

public static partial class Meerkat
{
    /// <summary>
    /// Starts a fluent partial update for the document with the specified ID.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static UpdateBuilder<TSchema, TId> Update<TSchema, TId>(TId id)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        new(GetCollectionForType<TSchema, TId>(),
            Builders<TSchema>.Filter.Where(x => x.Id.Equals(id)),
            isMany: false);

    /// <summary>
    /// Starts a fluent partial update for the first document matching the predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static UpdateBuilder<TSchema, TId> UpdateOne<TSchema, TId>(Expression<Func<TSchema, bool>> predicate)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        new(GetCollectionForType<TSchema, TId>(), predicate, isMany: false);

    /// <summary>
    /// Starts a fluent partial update for all documents matching the predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static UpdateBuilder<TSchema, TId> UpdateMany<TSchema, TId>(Expression<Func<TSchema, bool>> predicate)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        new(GetCollectionForType<TSchema, TId>(), predicate, isMany: true);

    /// <summary>
    /// Starts a fluent partial update for documents matching the given filter.
    /// </summary>
    /// <param name="filter">The MongoDB filter definition.</param>
    /// <param name="many">When true, applies the update to all matching documents.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    public static UpdateBuilder<TSchema, TId> UpdateByFilter<TSchema, TId>(FilterDefinition<TSchema> filter,
        bool many = false)
        where TSchema : Schema<TId> where TId : IEquatable<TId> =>
        new(GetCollectionForType<TSchema, TId>(), filter, many);
}
