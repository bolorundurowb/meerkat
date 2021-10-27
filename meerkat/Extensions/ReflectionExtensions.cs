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
            where TAttribute : Attribute => instance.GetType().AttributedWith<TAttribute>();
    }
}