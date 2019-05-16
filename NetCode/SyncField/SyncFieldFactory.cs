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
        public abstract SynchronisableField Construct();
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
}
