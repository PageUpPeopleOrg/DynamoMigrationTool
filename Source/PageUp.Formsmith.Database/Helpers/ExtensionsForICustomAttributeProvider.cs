using System;
using System.Reflection;

namespace PageUp.Formsmith.Database.Helpers
{
    public static class ExtensionsForICustomAttributeProvider
    {
        public static T GetOneAttribute<T>(this ICustomAttributeProvider member)
            where T : Attribute
        {
            return member.GetOneAttribute<T>(false);
        }

        public static T GetOneAttribute<T>(this ICustomAttributeProvider member, bool inherit)
            where T : Attribute
        {
            T[] attributes = member.GetCustomAttributes(typeof(T), inherit) as T[];

            if ((attributes == null) || (attributes.Length == 0))
                return null;
            else
                return attributes[0];
        }

        public static T[] GetAllAttributes<T>(this ICustomAttributeProvider member)
            where T : Attribute
        {
            return member.GetAllAttributes<T>(false);
        }

        public static T[] GetAllAttributes<T>(this ICustomAttributeProvider member, bool inherit)
            where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), inherit) as T[];
        }
    }
}