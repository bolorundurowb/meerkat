using MongoDB.Bson.Serialization.Serializers;
using System;
using MongoDB.Bson.Serialization;

namespace meerkat.Serializers
{
#if NET6_0 || NET7_0 || NET8_0
    internal class DocumentDateOnlySerializer : StructSerializerBase<DateOnly>
    {
        private const string SerializationFormat = "yyyy-MM-dd";

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value) => context.Writer.WriteString(value.ToString(SerializationFormat));

        public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) => DateOnly.ParseExact(context.Reader.ReadString(), SerializationFormat);
    }
#endif
}
