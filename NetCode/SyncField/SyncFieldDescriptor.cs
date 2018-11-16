using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.SyncField
{
    internal class SyncFieldDescriptor
    {
        protected SyncFlags flags;

        public Type FieldType;
        public Func<object, object> Getter;
        public Action<object, object> Setter;
        
        private Func<object>[] constructor;

        public SyncFieldDescriptor(Func<object>[] fieldConstructor, Func<object, object> fieldGetter, Action<object, object> fieldSetter, SyncFlags syncFlags, Type fieldType)
        {
            constructor = fieldConstructor;
            Getter = fieldGetter;
            Setter = fieldSetter;
            flags = syncFlags;
            FieldType = fieldType;
        }

        public SynchronisableField GenerateField(byte elementDepth = 0)
        {
            SynchronisableField field = (SynchronisableField)(constructor[elementDepth].Invoke());
            field.Flags = flags;
            field.Descriptor = this;
            field.ElementDepth = elementDepth;
            return field;
        }
    }
}
