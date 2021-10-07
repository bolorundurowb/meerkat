using System;

namespace meerkat.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionAttribute : Attribute
    {
        public string Name { get; set; }

        public bool TrackTimestamps { get; set; }
    }
}