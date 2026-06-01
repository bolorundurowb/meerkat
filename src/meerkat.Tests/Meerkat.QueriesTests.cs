using OmniAssert;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class MeerkatQueriesTests
{
    private class TestEntity : Schema<string>
    {
        public string Name { get; set; }
    }

    [Fact]
    public void FindById_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.FindById<TestEntity, string>("123");
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void FindByIdAsync_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => { _ = Meerkat.FindByIdAsync<TestEntity, string>("123"); };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void FindOne_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.FindOne<TestEntity, string>(x => x.Name == "test");
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void FindOneAsync_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => { _ = Meerkat.FindOneAsync<TestEntity, string>(x => x.Name == "test"); };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void FindOne_WithNoPredicate_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.FindOne<TestEntity, string>();
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void Find_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.Find<TestEntity, string>(x => x.Name == "test");
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void Find_WithNoPredicate_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => Meerkat.Find<TestEntity, string>();
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }

    [Fact]
    public void FindAsync_ShouldThrowExceptionIfNotConnected()
    {
        Meerkat.ResetDatabase();
        Action act = () => { _ = Meerkat.FindAsync<TestEntity, string>(x => x.Name == "test"); };
        act.Throws<InvalidOperationException>()
            .WithMessage("The database connection has not been initialized. Call Connect() before carrying out any operations.");
    }
}
