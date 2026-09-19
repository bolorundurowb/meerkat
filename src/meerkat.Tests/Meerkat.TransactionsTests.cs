using MongoDB.Driver;
using Moq;
using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatTransactionsTests
{
    [Fact]
    public void WithTransaction_ShouldThrow_WhenNotConnected()
    {
        Meerkat.ResetDatabase();

        var act = () => Meerkat.WithTransaction(() => { });

        act.Throws<InvalidOperationException>();
    }

    [Fact]
    public void WithTransactionGeneric_ShouldThrow_WhenNotConnected()
    {
        Meerkat.ResetDatabase();

        Action act = () => Meerkat.WithTransaction(() => 42);

        act.Throws<InvalidOperationException>();
    }

    [Fact]
    public async Task WithTransactionAsync_ShouldThrow_WhenNotConnected()
    {
        Meerkat.ResetDatabase();

        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
            Meerkat.WithTransactionAsync(_ => Task.CompletedTask));
    }

    [Fact]
    public async Task WithTransactionAsyncGeneric_ShouldThrow_WhenNotConnected()
    {
        Meerkat.ResetDatabase();

        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
            Meerkat.WithTransactionAsync(_ => Task.FromResult(42)));
    }

    [Fact]
    public void WithTransaction_ShouldThrow_WhenNested()
    {
        Meerkat.ResetDatabase();
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            var act = () => Meerkat.WithTransaction(() => { });

            act.Throws<InvalidOperationException>();
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void WithTransactionGeneric_ShouldThrow_WhenNested()
    {
        Meerkat.ResetDatabase();
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            Action act = () => Meerkat.WithTransaction(() => 42);

            act.Throws<InvalidOperationException>();
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task WithTransactionAsync_ShouldThrow_WhenNested()
    {
        Meerkat.ResetDatabase();
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
                Meerkat.WithTransactionAsync(_ => Task.CompletedTask));
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task WithTransactionAsyncGeneric_ShouldThrow_WhenNested()
    {
        Meerkat.ResetDatabase();
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
                Meerkat.WithTransactionAsync(_ => Task.FromResult(42)));
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void WithAmbientSession_Action_ShouldInvokeWithoutSession_WhenCurrentSessionIsNull()
    {
        Meerkat.CurrentSession.Value = null;
        var withSessionInvoked = false;
        var withoutSessionInvoked = false;

        Meerkat.WithAmbientSession(
            _ => withSessionInvoked = true,
            () => withoutSessionInvoked = true);

        withSessionInvoked.Must().BeFalse();
        withoutSessionInvoked.Must().BeTrue();
    }

    [Fact]
    public void WithAmbientSession_Action_ShouldInvokeWithSession_WhenCurrentSessionIsNotNull()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            IClientSessionHandle? capturedSession = null;
            var withoutSessionInvoked = false;

            Meerkat.WithAmbientSession(
                s => capturedSession = s,
                () => withoutSessionInvoked = true);

            Xunit.Assert.Same(mockSession, capturedSession);
            withoutSessionInvoked.Must().BeFalse();
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public void WithAmbientSession_Func_ShouldInvokeWithoutSession_WhenCurrentSessionIsNull()
    {
        Meerkat.CurrentSession.Value = null;

        var result = Meerkat.WithAmbientSession(
            _ => "with_session",
            () => "without_session");

        result.Must().Be("without_session");
    }

    [Fact]
    public void WithAmbientSession_Func_ShouldInvokeWithSession_WhenCurrentSessionIsNotNull()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            IClientSessionHandle? capturedSession = null;

            var result = Meerkat.WithAmbientSession(
                s =>
                {
                    capturedSession = s;
                    return "with_session";
                },
                () => "without_session");

            Xunit.Assert.Same(mockSession, capturedSession);
            result.Must().Be("with_session");
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task WithAmbientSessionAsync_Task_ShouldInvokeWithoutSession_WhenCurrentSessionIsNull()
    {
        Meerkat.CurrentSession.Value = null;
        var withSessionInvoked = false;
        var withoutSessionInvoked = false;

        await Meerkat.WithAmbientSessionAsync(
            _ => { withSessionInvoked = true; return Task.CompletedTask; },
            () => { withoutSessionInvoked = true; return Task.CompletedTask; });

        withSessionInvoked.Must().BeFalse();
        withoutSessionInvoked.Must().BeTrue();
    }

    [Fact]
    public async Task WithAmbientSessionAsync_Task_ShouldInvokeWithSession_WhenCurrentSessionIsNotNull()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            IClientSessionHandle? capturedSession = null;
            var withoutSessionInvoked = false;

            await Meerkat.WithAmbientSessionAsync(
                s => { capturedSession = s; return Task.CompletedTask; },
                () => { withoutSessionInvoked = true; return Task.CompletedTask; });

            Xunit.Assert.Same(mockSession, capturedSession);
            withoutSessionInvoked.Must().BeFalse();
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }

    [Fact]
    public async Task WithAmbientSessionAsync_TaskGeneric_ShouldInvokeWithoutSession_WhenCurrentSessionIsNull()
    {
        Meerkat.CurrentSession.Value = null;

        var result = await Meerkat.WithAmbientSessionAsync(
            _ => Task.FromResult(100),
            () => Task.FromResult(200));

        result.Must().Be(200);
    }

    [Fact]
    public async Task WithAmbientSessionAsync_TaskGeneric_ShouldInvokeWithSession_WhenCurrentSessionIsNotNull()
    {
        var mockSession = new Mock<IClientSessionHandle>().Object;
        Meerkat.CurrentSession.Value = mockSession;
        try
        {
            IClientSessionHandle? capturedSession = null;

            var result = await Meerkat.WithAmbientSessionAsync(
                s =>
                {
                    capturedSession = s;
                    return Task.FromResult(100);
                },
                () => Task.FromResult(200));

            Xunit.Assert.Same(mockSession, capturedSession);
            result.Must().Be(100);
        }
        finally
        {
            Meerkat.CurrentSession.Value = null;
        }
    }
}
