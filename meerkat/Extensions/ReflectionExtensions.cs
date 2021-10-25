using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace meerkat.Extensions
{
    internal static class ReflectionExtensions
    {
        public static readonly Type StringType = typeof(string);

        public static IEnumerable<PropertyInfo> AttributedWith<TAttribute>(this object instance)
            where TAttribute : Attribute
        {
            var attributeType = typeof(TAttribute);
            return instance.GetType().GetProperties()
                .Where(x => x.CustomAttributes.Any(y => y.AttributeType == attributeType));
        }
    }
}