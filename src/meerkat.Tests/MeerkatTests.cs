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
        Assert.NotNull(Meerkat.Database);
    }

    [Fact]
    public void Database_ShouldThrowExceptionIfNotConnected()
    {
        // Arrange
        Meerkat.ResetDatabase();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => Meerkat.Database);
        Assert.Equal(
            "The database connection has not been initialized. Call Connect() before carrying out any operations.",
            exception.Message);
    }
}
