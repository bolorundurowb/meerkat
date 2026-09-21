using System;
using System.IO;
using meerkat.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using OmniAssert;
using Xunit;

namespace meerkat.Tests;

[Xunit.Collection("MeerkatUnitTests")]
public class UtcNullableDateTimeOffsetSerializerTests
{
    private readonly UtcNullableDateTimeOffsetSerializer _serializer = new();

    [Fact]
    public void Serialize_ShouldWriteNull_WhenValueIsNull()
    {
        using var stream = new MemoryStream();
        using var writer = new BsonBinaryWriter(stream);
        writer.WriteStartDocument();
        writer.WriteName("date");
        var context = BsonSerializationContext.CreateRoot(writer);
        _serializer.Serialize(context, default, null);
        writer.WriteEndDocument();

        var bson = stream.ToArray();
        var doc = BsonSerializer.Deserialize<BsonDocument>(bson);
        doc["date"].BsonType.Must().Be(BsonType.Null);
    }

    [Fact]
    public void Serialize_ShouldWriteUtcDateTime_WhenValueIsNotNull()
    {
        var dateTimeOffset = new DateTimeOffset(2026, 9, 20, 15, 30, 45, 123, TimeSpan.FromHours(2));
        using var stream = new MemoryStream();
        using var writer = new BsonBinaryWriter(stream);
        writer.WriteStartDocument();
        writer.WriteName("date");
        var context = BsonSerializationContext.CreateRoot(writer);
        _serializer.Serialize(context, default, dateTimeOffset);
        writer.WriteEndDocument();

        var bson = stream.ToArray();
        var doc = BsonSerializer.Deserialize<BsonDocument>(bson);
        doc["date"].BsonType.Must().Be(BsonType.DateTime);
        doc["date"].AsBsonDateTime.MillisecondsSinceEpoch.Must().Be(dateTimeOffset.ToUniversalTime().ToUnixTimeMilliseconds());
    }

    [Fact]
    public void Deserialize_ShouldReturnNull_WhenBsonTypeIsNull()
    {
        var doc = new BsonDocument("date", BsonNull.Value);
        var bson = doc.ToBson();
        using var stream = new MemoryStream(bson);
        using var reader = new BsonBinaryReader(stream);
        reader.ReadStartDocument();
        reader.ReadName();
        var context = BsonDeserializationContext.CreateRoot(reader);

        var result = _serializer.Deserialize(context, default);
        result.Must().BeNull();
    }

    [Fact]
    public void Deserialize_ShouldReturnDateTimeOffset_WhenBsonTypeIsDateTime()
    {
        var original = DateTimeOffset.UtcNow;
        var truncated = DateTimeOffset.FromUnixTimeMilliseconds(original.ToUnixTimeMilliseconds());
        var doc = new BsonDocument("date", new BsonDateTime(truncated.ToUnixTimeMilliseconds()));
        var bson = doc.ToBson();
        using var stream = new MemoryStream(bson);
        using var reader = new BsonBinaryReader(stream);
        reader.ReadStartDocument();
        reader.ReadName();
        var context = BsonDeserializationContext.CreateRoot(reader);

        var result = _serializer.Deserialize(context, default);
        result.Must().NotBeNull();
        result.Value.ToUnixTimeMilliseconds().Must().Be(truncated.ToUnixTimeMilliseconds());
    }

    [Fact]
    public void Deserialize_ShouldThrowFormatException_WhenBsonTypeIsInvalid()
    {
        var doc = new BsonDocument("date", "not-a-datetime");
        var bson = doc.ToBson();
        using var stream = new MemoryStream(bson);
        using var reader = new BsonBinaryReader(stream);
        reader.ReadStartDocument();
        reader.ReadName();
        var context = BsonDeserializationContext.CreateRoot(reader);

        Action act = () => _serializer.Deserialize(context, default);
        act.Throws<FormatException>()
            .WithMessage("Cannot deserialise DateTimeOffset? from BsonType String.");
    }
}
