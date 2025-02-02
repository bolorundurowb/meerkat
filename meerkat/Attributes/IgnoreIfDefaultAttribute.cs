using System;
using MongoDB.Bson.Serialization.Attributes;

namespace meerkat.Attributes
{
    [Obsolete("This was a thin wrapper over the BsonIgnoreIfDefault attribute. Use that instead as this will be removed in the next major release.")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreIfDefaultAttribute : BsonIgnoreIfDefaultAttribute
    {
    }
}