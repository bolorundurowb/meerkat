using OmniAssert;

namespace meerkat.Tests;

public class MeerkatTests
{
    [Fact]
    public void Connect_ShouldInitializeDatabase()
    {
        // Arrange
        var connectionString = "mongodb://localhost:27017/testdb";

        // Act
        Meerkat.Connect(connectionString);

        // Assert
        Meerkat.Database.Verify().NotToBeNull();
    }

    [Fact]
    public void Database_ShouldThrowExceptionIfNotConnected()
    {
        // Arrange
        Meerkat.ResetDatabase();

        // Act & Assert
        Action act = () => { _ = Meerkat.Database; };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
