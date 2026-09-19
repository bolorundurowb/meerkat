using MongoDB.Bson;
using OmniAssert;

namespace meerkat.Tests;

[Attributes.Collection(Name = "txn_orders")]
public class TxnOrder : Schema<ObjectId>
{
    public string Number { get; set; }

    public TxnOrder()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Attributes.Collection(Name = "txn_lines")]
public class TxnLine : Schema<ObjectId>
{
    public ObjectId OrderId { get; set; }
    public int Qty { get; set; }

    public TxnLine()
    {
        Id = ObjectId.GenerateNewId();
    }
}

[Collection("MeerkatIntegrationTests")]
[Trait("Category", "Integration")]
public class MeerkatTransactionsIT
{
    [Fact]
    public async Task WithTransactionAsync_CommitsBothWrites()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);
        var orderId = ObjectId.GenerateNewId();

        await Meerkat.WithTransactionAsync(async _ =>
        {
            var order = new TxnOrder { Id = orderId, Number = "TXN-1" };
            order.Save();

            var line = new TxnLine { OrderId = orderId, Qty = 2 };
            await line.SaveAsync();
        });

        var order = Meerkat.FindById<TxnOrder, ObjectId>(orderId);
        order.Must().NotBeNull();
        Meerkat.Count<TxnLine, ObjectId>(x => x.OrderId == orderId).Must().Be(1);
    }

    [Fact]
    public async Task WithTransactionAsync_RollsBackOnException()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);
        var orderId = ObjectId.GenerateNewId();

        await Xunit.Assert.ThrowsAnyAsync<Exception>(() => Meerkat.WithTransactionAsync(async _ =>
        {
            var order = new TxnOrder { Id = orderId, Number = "TXN-ROLLBACK" };
            order.Save();
            throw new InvalidOperationException("boom");
        }));

        Meerkat.FindById<TxnOrder, ObjectId>(orderId).Must().BeNull();
    }

    [Fact]
    public async Task WithTransactionAsync_ReadsUncommittedWithinCallback()
    {
        Meerkat.ResetDatabase();
        Meerkat.Connect(TestDatabase.ConnectionString);
        var orderId = ObjectId.GenerateNewId();

        var seen = await Meerkat.WithTransactionAsync(async _ =>
        {
            var order = new TxnOrder { Id = orderId, Number = "TXN-READ" };
            order.Save();
            return await Meerkat.FindByIdAsync<TxnOrder, ObjectId>(orderId);
        });

        seen.Must().NotBeNull();
        seen.Number.Must().Be("TXN-READ");
    }
}
