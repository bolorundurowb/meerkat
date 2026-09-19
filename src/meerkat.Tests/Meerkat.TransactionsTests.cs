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
    public async Task WithTransactionAsync_ShouldThrow_WhenNotConnected()
    {
        Meerkat.ResetDatabase();

        await Xunit.Assert.ThrowsAsync<InvalidOperationException>(() =>
            Meerkat.WithTransactionAsync(_ => Task.CompletedTask));
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
}
