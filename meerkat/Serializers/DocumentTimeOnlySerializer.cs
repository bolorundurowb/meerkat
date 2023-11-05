using MongoDB.Bson.Serialization.Serializers;
using System;
using MongoDB.Bson.Serialization;

namespace meerkat.Serializers
{
#if NET6_0 || NET7_0 || NET8_0
    internal class DocumentTimeOnlySerializer : StructSerializerBase<TimeOnly>
    {
        private IBsonSerializer<TimeSpan> InnerSerializer = new TimeSpanSerializer();

        public override TimeOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var timespan = InnerSerializer.Deserialize(context, args);
            return TimeOnly.FromTimeSpan(timespan);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeOnly value) =>
            InnerSerializer.Serialize(context, args, value.ToTimeSpan());
    }
#endif
}
