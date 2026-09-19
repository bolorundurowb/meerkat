using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace meerkat.Serialization;

/// <summary>
/// Serialises <see cref="DateTimeOffset?"/> as a UTC BSON DateTime so equality and index scans stay on a single field.
/// </summary>
internal sealed class UtcNullableDateTimeOffsetSerializer : SerializerBase<DateTimeOffset?>
{
    public override DateTimeOffset? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();
        switch (bsonType)
        {
            case BsonType.DateTime:
                return DateTimeOffset.FromUnixTimeMilliseconds(context.Reader.ReadDateTime());
            case BsonType.Null:
                context.Reader.ReadNull();
                return null;
            default:
                throw new FormatException($"Cannot deserialise DateTimeOffset? from BsonType {bsonType}.");
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTimeOffset? value)
    {
        if (value == null)
        {
            context.Writer.WriteNull();
            return;
        }

        context.Writer.WriteDateTime(value.Value.ToUniversalTime().ToUnixTimeMilliseconds());
    }
}
