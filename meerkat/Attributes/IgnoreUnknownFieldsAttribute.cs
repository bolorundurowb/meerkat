using System;
using MongoDB.Bson.Serialization.Attributes;

namespace meerkat.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class IgnoreUnknownFieldsAttribute : BsonIgnoreExtraElementsAttribute
    {
    }
}