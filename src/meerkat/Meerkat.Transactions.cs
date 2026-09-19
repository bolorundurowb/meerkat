using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace meerkat;

public static partial class Meerkat
{
    /// <summary>
    /// Executes the given asynchronous action inside a MongoDB transaction.
    /// Requires a replica set or sharded cluster. Nested transactions are not supported.
    /// </summary>
    public static Task WithTransactionAsync(Func<CancellationToken, Task> action,
        TransactionOptions? options = null, CancellationToken cancellationToken = default)
    {
        return WithTransactionCoreAsync<object?>(
            async ct =>
            {
                await action(ct).ConfigureAwait(false);
                return null;
            },
            options,
            cancellationToken);
    }

    /// <summary>
    /// Executes the given asynchronous function inside a MongoDB transaction and returns its result.
    /// Requires a replica set or sharded cluster. Nested transactions are not supported.
    /// </summary>
    public static Task<TResult> WithTransactionAsync<TResult>(Func<CancellationToken, Task<TResult>> action,
        TransactionOptions? options = null, CancellationToken cancellationToken = default)
    {
        return WithTransactionCoreAsync(action, options, cancellationToken);
    }

    /// <summary>
    /// Executes the given action inside a MongoDB transaction.
    /// Requires a replica set or sharded cluster. Nested transactions are not supported.
    /// </summary>
    public static void WithTransaction(Action action, TransactionOptions? options = null)
    {
        WithTransactionCore(
            () =>
            {
                action();
                return true;
            },
            options);
    }

    /// <summary>
    /// Executes the given function inside a MongoDB transaction and returns its result.
    /// Requires a replica set or sharded cluster. Nested transactions are not supported.
    /// </summary>
    public static TResult WithTransaction<TResult>(Func<TResult> action, TransactionOptions? options = null)
    {
        return WithTransactionCore(action, options);
    }

    private static async Task<TResult> WithTransactionCoreAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action, TransactionOptions? options, CancellationToken cancellationToken)
    {
        EnsureNotNested();

        var client = Client;
        using var session = await client.StartSessionAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        CurrentSession.Value = session;
        try
        {
            return await session
                .WithTransactionAsync((_, ct) => action(ct), options, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            CurrentSession.Value = null;
        }
    }

    private static TResult WithTransactionCore<TResult>(Func<TResult> action, TransactionOptions? options)
    {
        EnsureNotNested();

        var client = Client;
        using var session = client.StartSession();
        CurrentSession.Value = session;
        try
        {
            return session.WithTransaction((_, _) => action(), options);
        }
        finally
        {
            CurrentSession.Value = null;
        }
    }

    private static void EnsureNotNested()
    {
        if (CurrentSession.Value != null)
            throw new InvalidOperationException(
                "A transaction is already in progress. Nested transactions are not supported.");
    }

    internal static void WithAmbientSession(Action<IClientSessionHandle> withSession, Action withoutSession)
    {
        var session = CurrentSession.Value;
        if (session != null)
            withSession(session);
        else
            withoutSession();
    }

    internal static TResult WithAmbientSession<TResult>(Func<IClientSessionHandle, TResult> withSession,
        Func<TResult> withoutSession)
    {
        var session = CurrentSession.Value;
        return session != null ? withSession(session) : withoutSession();
    }

    internal static Task WithAmbientSessionAsync(Func<IClientSessionHandle, Task> withSession, Func<Task> withoutSession)
    {
        var session = CurrentSession.Value;
        return session != null ? withSession(session) : withoutSession();
    }

    internal static Task<TResult> WithAmbientSessionAsync<TResult>(Func<IClientSessionHandle, Task<TResult>> withSession,
        Func<Task<TResult>> withoutSession)
    {
        var session = CurrentSession.Value;
        return session != null ? withSession(session) : withoutSession();
    }
}
