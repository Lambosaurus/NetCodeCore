using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Reflection.Emit;

namespace NetCode
{
    internal static class DelegateGenerator
    {
        private const bool FIND_NONPUBLIC_ACCESSORS = true;

        public static Func<object> GenerateConstructor(Type type)
        {
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new NotSupportedException(string.Format("Type {0} must provide a constructor with zero arguments.", type.Name));
            }
            
            DynamicMethod method =
                new DynamicMethod(
                    string.Format("{0}.new", constructor.DeclaringType.Name),
                    constructor.DeclaringType,
                    Type.EmptyTypes,
                    true
                    );

            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);

            return (Func<object>)method.CreateDelegate(typeof(Func<object>));
        }

        public static Func<object, object> GenerateGetter(FieldInfo field)
        {
            DynamicMethod method = new DynamicMethod(
                string.Format("{0}.{1}.get", field.DeclaringType.Name, field.Name),
                typeof(object),
                new Type[] { typeof(object) },
                field.DeclaringType,
                true
                );

            ILGenerator gen = method.GetILGenerator();
            
            gen.Emit(OpCodes.Ldarg_0); // Load the target object onto the stack
            gen.Emit(OpCodes.Ldfld, field); // Load the field from the object
            
            if (field.FieldType != typeof(object))
            {
                // If the field is a valuetype, we need to box it into an object
                gen.Emit(OpCodes.Box, field.FieldType);
            }

            gen.Emit(OpCodes.Ret);

            return (Func<object, object>)(method.CreateDelegate(typeof(Func<object, object>)));
        }

        public static Action<object, object> GenerateSetter(FieldInfo field)
        {
            DynamicMethod method = new DynamicMethod(
                string.Format("{0}.{1}.set", field.DeclaringType.Name, field.Name),
                null,
                new Type[] { typeof(object), typeof(object) },
                field.DeclaringType,
                true
                );

            ILGenerator gen = method.GetILGenerator();
            
            gen.Emit(OpCodes.Ldarg_0); // Load the target object onto the stack
            gen.Emit(OpCodes.Ldarg_1); // Load the field value

            if (field.FieldType != typeof(object))
            {
                // If the field is a valuetype, we need to box it into an object, otherwise it acts as a object pointer.
                gen.Emit(OpCodes.Unbox_Any, field.FieldType);
            }
            
            gen.Emit(OpCodes.Stfld, field); // set the field
            gen.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        public static Func<object, object> GenerateGetter(PropertyInfo property)
        {
            DynamicMethod method = new DynamicMethod(
                string.Format("{0}.{1}.get", property.DeclaringType.Name, property.Name),
                typeof(object),
                new Type[] { typeof(object) },
                property.DeclaringType,
                true
                );

            ILGenerator gen = method.GetILGenerator();
            
            gen.Emit(OpCodes.Ldarg_0); // Load the target object onto the stack
            gen.Emit(OpCodes.Call, property.GetGetMethod(FIND_NONPUBLIC_ACCESSORS)); // load the field

            if (property.PropertyType != typeof(object))
            {
                // If the field is a valuetype, we need to box it into an object, otherwise it acts as a object pointer.
                gen.Emit(OpCodes.Box, property.PropertyType);
            }

            gen.Emit(OpCodes.Ret); // return the value on the top of the stack

            return (Func<object, object>)(method.CreateDelegate(typeof(Func<object, object>)));
        }

        public static Action<object, object> GenerateSetter(PropertyInfo property)
        {
            DynamicMethod method = new DynamicMethod(
                string.Format("{0}.{1}.set", property.DeclaringType.Name, property.Name),
                null,
                new Type[] { typeof(object), typeof(object) },
                property.DeclaringType,
                true
                );

            ILGenerator gen = method.GetILGenerator();
            
            gen.Emit(OpCodes.Ldarg_0); // Load the target object onto the stack
            gen.Emit(OpCodes.Ldarg_1); // Load the field value

            if (property.PropertyType != typeof(object))
            {
                // If the field is a valuetype, we need to box it into an object, otherwise it acts as a object pointer.
                gen.Emit(OpCodes.Unbox_Any, property.PropertyType);
            }
            
            gen.Emit(OpCodes.Call, property.GetSetMethod(FIND_NONPUBLIC_ACCESSORS)); // Set the field
            gen.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }
    }
}
