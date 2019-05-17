using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncField.Implementations;
using NetCode.Util;

namespace NetCode.SyncField
{
    public abstract class SyncFieldFactory
    {
        SynchronisableField StaticField;
        public abstract SynchronisableField Construct();

        public void SkipFromBuffer(NetBuffer buffer)
        {
            if (StaticField == null)
            {
                StaticField = Construct();
            }
            StaticField.SkipFromBuffer(buffer);
        }
    }

    public class SyncFieldValueFactory : SyncFieldFactory
    {
        Func<SynchronisableField> Constructor;
        public SyncFieldValueFactory(Func<SynchronisableField> constructor)
        {
            Constructor = constructor;
        }

        public SyncFieldValueFactory(Type syncFieldType)
        {
            Constructor = DelegateGenerator.GenerateConstructor<SynchronisableField>(syncFieldType);
        }

        public sealed override SynchronisableField Construct()
        {
            return Constructor.Invoke();
        }
    }

    public class SyncFieldReferenceFactory : SyncFieldFactory
    {
        Type ReferenceType;
        public SyncFieldReferenceFactory(Type refType)
        {
            ReferenceType = refType;
        }

        public sealed override SynchronisableField Construct()
        {
            return new SyncFieldReference(ReferenceType);
        }
    }

    public class SyncFieldLinkedReferenceFactory : SyncFieldFactory
    {
        Type ReferenceType;
        public SyncFieldLinkedReferenceFactory(Type refType)
        {
            ReferenceType = refType;
        }
        public sealed override SynchronisableField Construct()
        {
            return new SyncFieldReference(ReferenceType);
        }
    }

    public class SyncFieldArrayFactory<T> : SyncFieldFactory
    {
        SyncFieldFactory ElementFactory;
        public SyncFieldArrayFactory(SyncFieldFactory elementFactory)
        {
            ElementFactory = elementFactory;
        }

        public sealed override SynchronisableField Construct()
        {
            return new SynchronisableArray<T>(ElementFactory);
        }
    }

    public class SyncFieldListFactory<T> : SyncFieldFactory
    {
        SyncFieldFactory ElementFactory;
        public SyncFieldListFactory(SyncFieldFactory elementFactory)
        {
            ElementFactory = elementFactory;
        }

        public sealed override SynchronisableField Construct()
        {
            return new SynchronisableList<T>(ElementFactory);
        }
    }
}
