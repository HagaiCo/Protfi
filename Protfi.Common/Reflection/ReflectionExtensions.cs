using System.Reflection;

namespace Protfi.Common.Reflection
{
    public static class ReflectionExtensions
    {
        public static bool HasDefaultConstructor(this Type type)
        {
            return type
                .GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null) != null;
        }

        public static bool HasAttribute<T>(this Enum value) where T : Attribute
        {
            return value.GetAttributesForEnumValue<T>() != null;
        }

        public static T GetAttributesForEnumValue<T>(this Enum value) where T : Attribute
        {
            var enumMember = value.GetType().GetField(value.ToString());
            if (enumMember == null)
                return null;

            var attr = enumMember.GetCustomAttribute<T>();
            return attr;
        }
    }
}