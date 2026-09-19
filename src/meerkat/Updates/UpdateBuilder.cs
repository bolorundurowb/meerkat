using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using meerkat.Extensions;
using MongoDB.Driver;

namespace meerkat.Updates;

/// <summary>
/// Builds and executes atomic partial updates against a MongoDB collection.
/// </summary>
/// <typeparam name="TSchema">The schema type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public sealed class UpdateBuilder<TSchema, TId>
    where TSchema : Schema<TId> where TId : IEquatable<TId>
{
    private readonly IMongoCollection<TSchema> _collection;
    private readonly FilterDefinition<TSchema> _filter;
    private readonly bool _isMany;
    private readonly List<UpdateDefinition<TSchema>> _operations = new();

    internal UpdateBuilder(IMongoCollection<TSchema> collection, FilterDefinition<TSchema> filter, bool isMany)
    {
        _collection = collection;
        _filter = filter;
        _isMany = isMany;
    }

    /// <summary>
    /// Sets the specified field to the given value.
    /// </summary>
    public UpdateBuilder<TSchema, TId> Set<TField>(Expression<Func<TSchema, TField>> field, TField value)
    {
        _operations.Add(Builders<TSchema>.Update.Set(field, value));
        return this;
    }

    /// <summary>
    /// Unsets the specified field.
    /// </summary>
    public UpdateBuilder<TSchema, TId> Unset<TField>(Expression<Func<TSchema, TField>> field)
    {
        _operations.Add(Builders<TSchema>.Update.Unset(new ExpressionFieldDefinition<TSchema>(field)));
        return this;
    }

    /// <summary>
    /// Appends a value to an array field.
    /// </summary>
    public UpdateBuilder<TSchema, TId> Push<TItem>(Expression<Func<TSchema, IEnumerable<TItem>>> field, TItem value)
    {
        _operations.Add(Builders<TSchema>.Update.Push(field, value));
        return this;
    }

    /// <summary>
    /// Removes all instances of a value from an array field.
    /// </summary>
    public UpdateBuilder<TSchema, TId> Pull<TItem>(Expression<Func<TSchema, IEnumerable<TItem>>> field, TItem value)
    {
        _operations.Add(Builders<TSchema>.Update.Pull(field, value));
        return this;
    }

    /// <summary>
    /// Adds a value to an array field if it is not already present.
    /// </summary>
    public UpdateBuilder<TSchema, TId> AddToSet<TItem>(Expression<Func<TSchema, IEnumerable<TItem>>> field, TItem value)
    {
        _operations.Add(Builders<TSchema>.Update.AddToSet(field, value));
        return this;
    }

    /// <summary>
    /// Atomically increments a numeric field by the given amount.
    /// </summary>
    public UpdateBuilder<TSchema, TId> Inc<TField>(Expression<Func<TSchema, TField>> field, TField amount)
        where TField : struct
    {
        _operations.Add(Builders<TSchema>.Update.Inc(field, amount));
        return this;
    }

    /// <summary>
    /// Executes the configured update.
    /// </summary>
    public UpdateResult Execute(CancellationToken cancellationToken = default)
    {
        var update = BuildUpdate();
        return _isMany
            ? _collection.UpdateMany(_filter, update, cancellationToken: cancellationToken)
            : _collection.UpdateOne(_filter, update, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Executes the configured update asynchronously.
    /// </summary>
    public Task<UpdateResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var update = BuildUpdate();
        return _isMany
            ? _collection.UpdateManyAsync(_filter, update, cancellationToken: cancellationToken)
            : _collection.UpdateOneAsync(_filter, update, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Executes the configured update and returns the updated document.
    /// Cannot be used with <see cref="Meerkat.UpdateMany{TSchema,TId}"/>.
    /// </summary>
    public TSchema? ExecuteAndGetUpdated(CancellationToken cancellationToken = default)
    {
        EnsureSingleDocumentUpdate();
        return _collection.FindOneAndUpdate(
            _filter,
            BuildUpdate(),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);
    }

    /// <summary>
    /// Executes the configured update asynchronously and returns the updated document.
    /// Cannot be used with <see cref="Meerkat.UpdateMany{TSchema,TId}"/>.
    /// </summary>
    public Task<TSchema?> ExecuteAndGetUpdatedAsync(CancellationToken cancellationToken = default)
    {
        EnsureSingleDocumentUpdate();
        return _collection.FindOneAndUpdateAsync(
            _filter,
            BuildUpdate(),
            new FindOneAndUpdateOptions<TSchema> { ReturnDocument = ReturnDocument.After },
            cancellationToken);
    }

    private void EnsureSingleDocumentUpdate()
    {
        if (_isMany)
            throw new InvalidOperationException(
                "ExecuteAndGetUpdated cannot be used with UpdateMany. Use Execute or ExecuteAsync instead.");
    }

    private UpdateDefinition<TSchema> BuildUpdate()
    {
        if (_operations.Count == 0)
            throw new InvalidOperationException(
                "No update operations have been specified. Call at least one of Set, Unset, Push, Pull, AddToSet, or Inc before executing.");

        var operations = _operations;
        if (typeof(TSchema).ShouldTrackTimestamps())
        {
            operations = new List<UpdateDefinition<TSchema>>(operations)
            {
                Builders<TSchema>.Update.Set(x => x.UpdatedAt, DateTime.UtcNow)
            };
        }

        return operations.Count == 1
            ? operations[0]
            : Builders<TSchema>.Update.Combine(operations);
    }
}
