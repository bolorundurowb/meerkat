using System;
using System.Collections.Generic;
using System.Reflection;

namespace meerkat.Extensions
{
    internal static class ReflectionExtensions
    {
        public static IEnumerable<PropertyInfo> AttributedWith<TAttribute>(this object instance)
            where TAttribute : Attribute => instance.GetType().AttributedWith<TAttribute>();
    }
}