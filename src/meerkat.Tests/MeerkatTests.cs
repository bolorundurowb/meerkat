using OmniAssert;

namespace meerkat.Tests;

public class MeerkatTests
{
    [Fact]
    public void Connect_ShouldInitializeDatabase()
    {
        var connectionString = "mongodb://localhost:27017/testdb";
        Meerkat.Connect(connectionString);
        Meerkat.Database.Verify().NotToBeNull();
    }

    [Fact]
    public void Database_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        var act = () => { _ = Meerkat.Database; };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
