using meerkat.Attributes;
using MongoDB.Bson;

namespace meerkat.Tests;

[meerkat.Attributes.Collection(Name = "update_test_counters")]
public class CounterTestModel : Schema<ObjectId>
{
    public string Name { get; set; }
    public int Value { get; set; }
    public long Total { get; set; }
    public double Score { get; set; }

    public CounterTestModel()
    {
        Id = ObjectId.GenerateNewId();
    }
}
