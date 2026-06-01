using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace meerkat;

/// <summary>
/// Provides atomic increment and decrement update operations for MongoDB using generic schemas.
/// </summary>
public static partial class Meerkat
{
    /// <summary>
    /// Atomically increments a numeric field on the document with the specified ID.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static void IncrementById<TSchema, TId, TField>(TId id,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOne(
            x => x.Id.Equals(id),
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on the document with the specified ID asynchronously.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static Task IncrementByIdAsync<TSchema, TId, TField>(TId id,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOneAsync(
            x => x.Id.Equals(id),
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on the document with the specified ID.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static void DecrementById<TSchema, TId, TField>(TId id,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOne(
            x => x.Id.Equals(id),
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on the document with the specified ID asynchronously.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static Task DecrementByIdAsync<TSchema, TId, TField>(TId id,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOneAsync(
            x => x.Id.Equals(id),
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on documents matching the given filter.
    /// </summary>
    /// <param name="filter">The MongoDB filter definition.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static void IncrementByFilter<TSchema, TId, TField>(FilterDefinition<TSchema> filter,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOne(
            filter,
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on documents matching the given filter asynchronously.
    /// </summary>
    /// <param name="filter">The MongoDB filter definition.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static Task IncrementByFilterAsync<TSchema, TId, TField>(FilterDefinition<TSchema> filter,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOneAsync(
            filter,
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on documents matching the given filter.
    /// </summary>
    /// <param name="filter">The MongoDB filter definition.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static void DecrementByFilter<TSchema, TId, TField>(FilterDefinition<TSchema> filter,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOne(
            filter,
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on documents matching the given filter asynchronously.
    /// </summary>
    /// <param name="filter">The MongoDB filter definition.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static Task DecrementByFilterAsync<TSchema, TId, TField>(FilterDefinition<TSchema> filter,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOneAsync(
            filter,
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on the first document matching the predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static void IncrementOne<TSchema, TId, TField>(Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOne(
            predicate,
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on the first document matching the predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static Task IncrementOneAsync<TSchema, TId, TField>(Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOneAsync(
            predicate,
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on the first document matching the predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static void DecrementOne<TSchema, TId, TField>(Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOne(
            predicate,
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on the first document matching the predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static Task DecrementOneAsync<TSchema, TId, TField>(Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateOneAsync(
            predicate,
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on all documents matching the predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static void IncrementMany<TSchema, TId, TField>(Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateMany(
            predicate,
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on all documents matching the predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static Task IncrementManyAsync<TSchema, TId, TField>(Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateManyAsync(
            predicate,
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on all documents matching the predicate.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static void DecrementMany<TSchema, TId, TField>(Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateMany(
            predicate,
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on all documents matching the predicate asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    public static Task DecrementManyAsync<TSchema, TId, TField>(Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().UpdateManyAsync(
            predicate,
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            cancellationToken: cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on the document with the specified ID and returns the updated document.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    /// <returns>The updated document, or null if not found.</returns>
    public static TSchema? IncrementByIdAndGetUpdated<TSchema, TId, TField>(TId id,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().FindOneAndUpdate(
            x => x.Id.Equals(id),
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on the document with the specified ID and returns the updated document asynchronously.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    /// <returns>The updated document, or null if not found.</returns>
    public static Task<TSchema?> IncrementByIdAndGetUpdatedAsync<TSchema, TId, TField>(TId id,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().FindOneAndUpdateAsync(
            x => x.Id.Equals(id),
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on the document with the specified ID and returns the updated document.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    /// <returns>The updated document, or null if not found.</returns>
    public static TSchema? DecrementByIdAndGetUpdated<TSchema, TId, TField>(TId id,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().FindOneAndUpdate(
            x => x.Id.Equals(id),
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on the document with the specified ID and returns the updated document asynchronously.
    /// </summary>
    /// <param name="id">The document's unique identifier.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    /// <returns>The updated document, or null if not found.</returns>
    public static Task<TSchema?> DecrementByIdAndGetUpdatedAsync<TSchema, TId, TField>(TId id,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().FindOneAndUpdateAsync(
            x => x.Id.Equals(id),
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on the first document matching the predicate and returns the updated document.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    /// <returns>The updated document, or null if not found.</returns>
    public static TSchema? IncrementOneAndGetUpdated<TSchema, TId, TField>(
        Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().FindOneAndUpdate(
            predicate,
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

    /// <summary>
    /// Atomically increments a numeric field on the first document matching the predicate and returns the updated document asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to increment.</param>
    /// <param name="amount">The amount to increment by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    /// <returns>The updated document, or null if not found.</returns>
    public static Task<TSchema?> IncrementOneAndGetUpdatedAsync<TSchema, TId, TField>(
        Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().FindOneAndUpdateAsync(
            predicate,
            Builders<TSchema>.Update.Inc(field, ResolveAmount(amount)),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on the first document matching the predicate and returns the updated document.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    /// <returns>The updated document, or null if not found.</returns>
    public static TSchema? DecrementOneAndGetUpdated<TSchema, TId, TField>(
        Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().FindOneAndUpdate(
            predicate,
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

    /// <summary>
    /// Atomically decrements a numeric field on the first document matching the predicate and returns the updated document asynchronously.
    /// </summary>
    /// <param name="predicate">A function to test each element.</param>
    /// <param name="field">An expression identifying the field to decrement.</param>
    /// <param name="amount">The amount to decrement by. Defaults to 1.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <typeparam name="TSchema">The schema type.</typeparam>
    /// <typeparam name="TId">The identifier type.</typeparam>
    /// <typeparam name="TField">The numeric field type.</typeparam>
    /// <returns>The updated document, or null if not found.</returns>
    public static Task<TSchema?> DecrementOneAndGetUpdatedAsync<TSchema, TId, TField>(
        Expression<Func<TSchema, bool>> predicate,
        Expression<Func<TSchema, TField>> field, TField amount = default,
        CancellationToken cancellationToken = default)
        where TSchema : Schema<TId> where TId : IEquatable<TId> where TField : struct =>
        GetCollectionForType<TSchema, TId>().FindOneAndUpdateAsync(
            predicate,
            Builders<TSchema>.Update.Inc(field, NegateAmount(amount)),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

    private static TField ResolveAmount<TField>(TField amount) where TField : struct
    {
        if (amount.Equals(default(TField)))
            return (TField)Convert.ChangeType(1, typeof(TField));
        return amount;
    }

    private static TField NegateAmount<TField>(TField amount) where TField : struct
    {
        var resolved = ResolveAmount(amount);
        return (TField)Convert.ChangeType(-Convert.ToDouble(resolved), typeof(TField));
    }
}
