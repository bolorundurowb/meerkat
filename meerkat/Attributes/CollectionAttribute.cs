using System;

namespace Meerkat.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionAttribute : Attribute
    {
        public string Name { get; private set; }

        public bool TrackTimestamps { get; private set; }

        public CollectionAttribute(string collectionName = null, bool trackTimestamps = false)
        {
            Name = collectionName;
            TrackTimestamps = trackTimestamps;
        }
    }
}