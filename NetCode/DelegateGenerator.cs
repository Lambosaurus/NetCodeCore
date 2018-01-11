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
        public static Func<object> GenerateConstructor(Type type)
        {
            ConstructorInfo constructor = type.GetConstructor(new Type[0]);
            if (constructor == null)
            {
                throw new NotSupportedException(string.Format("Type {0} must provide a constructor with zero arguments.", type.Name));
            }

            // Create the dynamic method
            DynamicMethod method =
                new DynamicMethod(
                    string.Format("{0}__{1}", constructor.DeclaringType.Name, Guid.NewGuid().ToString().Replace("-", "")),
                    constructor.DeclaringType,
                    new Type[0],
                    true
                    );

            // Create the il
            ILGenerator gen = method.GetILGenerator();
            gen.Emit(OpCodes.Newobj, constructor);
            gen.Emit(OpCodes.Ret);

            // Return the delegate
            return (Func<object>)method.CreateDelegate(typeof(Func<object>));
        }

        public static Func<object, object> GenerateGetter(FieldInfo field)
        {
            DynamicMethod method = new DynamicMethod(
                "Get" + field.Name,
                field.FieldType,
                new Type[] { typeof(object) },
                field.DeclaringType,
                true
                );

            ILGenerator gen = method.GetILGenerator();
            // Load the instance of the object (argument 0) onto the stack
            gen.Emit(OpCodes.Ldarg_0);
            // Load the value of the object's field (fi) onto the stack
            gen.Emit(OpCodes.Ldfld, field);
            // return the value on the top of the stack
            gen.Emit(OpCodes.Ret);

            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        public static Action<object, object> GenerateSetter(FieldInfo field)
        {
            DynamicMethod method = new DynamicMethod(
                "Set" + field.Name,
                null,
                new Type[] { typeof(object), typeof(object) },
                field.DeclaringType,
                true
                );

            ILGenerator gen = method.GetILGenerator();
            // Load the instance of the object (argument 0) onto the stack
            gen.Emit(OpCodes.Ldarg_0);
            // Load the field value
            gen.Emit(OpCodes.Ldarg_1);
            // Set the field
            gen.Emit(OpCodes.Stfld, field);
            // return the value on the top of the stack
            gen.Emit(OpCodes.Ret);

            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }
    }
}
