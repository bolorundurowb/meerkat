using System;
using MongoDB.Bson.Serialization.Attributes;

namespace meerkat.Attributes;

[Obsolete(
    "This was a thin wrapper over the BsonIgnoreExtraElements attribute. Use that instead as this will be removed in the next major release.")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class IgnoreUnknownFieldsAttribute : BsonIgnoreExtraElementsAttribute { }
