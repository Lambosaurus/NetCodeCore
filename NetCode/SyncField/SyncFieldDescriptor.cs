using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCode.SyncField
{
    internal class SyncFieldDescriptor
    {
        protected SyncFlags flags;

        public Type ReferenceType;
        public Func<object, object> Getter { get; private set; }
        public Action<object, object> Setter { get; private set; }

        private Func<SynchronisableField>[] Constructors;

        public SyncFieldDescriptor(Func<SynchronisableField> fieldConstructor, SyncFlags syncFlags, Type referenceType = null)
        {
            Constructors = new Func<SynchronisableField>[] { fieldConstructor };
            flags = syncFlags;
            ReferenceType = referenceType;
        }
        
        public void InsertParentConstructor(Func<SynchronisableField> fieldConstructor)
        {
            Func<SynchronisableField>[] newConstructors = new Func<SynchronisableField>[Constructors.Length + 1];
            newConstructors[0] = fieldConstructor;
            Constructors.CopyTo(newConstructors, 1);
            Constructors = newConstructors;
        }

        public void SetAccessors(Func<object, object> fieldGetter, Action<object, object> fieldSetter)
        {
            Getter = fieldGetter;
            Setter = fieldSetter;
        }

        public SynchronisableField GenerateField(byte elementDepth = 0)
        {
            SynchronisableField field = (Constructors[elementDepth].Invoke());
            field.Flags = flags;
            field.Descriptor = this;
            field.ElementDepth = elementDepth;
            return field;
        }
    }
}
