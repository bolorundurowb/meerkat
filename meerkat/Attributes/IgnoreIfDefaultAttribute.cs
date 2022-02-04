using System;
using MongoDB.Bson.Serialization.Attributes;

namespace meerkat.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreIfDefaultAttribute : BsonIgnoreIfDefaultAttribute
    {
    }
}