using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetCode.Util
{
    public static class AttributeHelper
    {
        private const BindingFlags FieldSearchFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static void ForAllTypesWithAttribute<T>( Action<Type, T> action ) where T : Attribute
        {
            string definedIn = typeof(T).Assembly.GetName().Name;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if ((!assembly.GlobalAssemblyCache) && ((assembly.GetName().Name == definedIn) || assembly.GetReferencedAssemblies().Any(a => a.Name == definedIn)))
                {
                    // We only sort through assemblies that include a reference to our attribute to speed up searching.
                    foreach (Type type in assembly.GetTypes())
                    {
                        // Find all types with the given attribute.
                        object[] attributes = type.GetCustomAttributes(typeof(T), false);
                        if (attributes.Length > 0)
                        {
                            T attribute = (T)attributes[0];
                            action(type, attribute);
                        }
                    }
                }
            }
        }

        public static void ForAllPropertiesWithAttribute<T>( Type type, Action<PropertyInfo, T> action ) where T : Attribute
        {
            foreach (PropertyInfo propInfo in type.GetProperties(FieldSearchFlags))
            {
                object[] attributes = propInfo.GetCustomAttributes(typeof(T), true);
                if (attributes.Length > 0)
                {
                    T attribute = (T)attributes[0];
                    action(propInfo, attribute);
                }
            }
        }

        public static void ForAllFieldsWithAttribute<T>(Type type, Action<FieldInfo, T> action) where T : Attribute
        {
            foreach (FieldInfo fieldInfo in type.GetFields(FieldSearchFlags))
            {
                object[] attributes = fieldInfo.GetCustomAttributes(typeof(T), true);
                if (attributes.Length > 0)
                {
                    T attribute = (T)attributes[0];
                    action(fieldInfo, attribute);
                }
            }
        }
    }
}
