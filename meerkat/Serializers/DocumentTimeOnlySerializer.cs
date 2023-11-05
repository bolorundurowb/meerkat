using MongoDB.Bson.Serialization.Serializers;
using System;
using MongoDB.Bson.Serialization;

namespace meerkat.Serializers
{
#if NET6_0 || NET7_0 || NET8_0
    internal class DocumentTimeOnlySerializer : StructSerializerBase<TimeOnly>
    {
        private readonly IBsonSerializer<TimeSpan> _innerSerializer = new TimeSpanSerializer();

        public override TimeOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var timespan = _innerSerializer.Deserialize(context, args);
            return TimeOnly.FromTimeSpan(timespan);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeOnly value) =>
            _innerSerializer.Serialize(context, args, value.ToTimeSpan());
    }
#endif
}
