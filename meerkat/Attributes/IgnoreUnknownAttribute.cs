using System;
using MongoDB.Bson.Serialization.Attributes;

namespace meerkat.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreUnknownAttribute : BsonIgnoreExtraElementsAttribute
    {
    }
}