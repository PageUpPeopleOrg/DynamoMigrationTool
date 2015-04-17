using System;

namespace PageUp.Formsmith.Database.Helpers
{
    public static class TypeExtensions
    {
        public static bool Is<T>(this Type type)
        {
            return typeof(T).IsAssignableFrom(type);
        }

        public static bool IsConcrete(this Type type)
        {
            return type.IsClass && !type.IsAbstract;
        }
    }
}