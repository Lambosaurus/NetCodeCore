using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Runtime.InteropServices;


namespace NetCode
{
    
    [Flags]
    public enum SyncFlags { None = 0, HalfPrecisionFloats = 1 };

    public class SynchronisableAttribute : System.Attribute
    {
        public SyncFlags flags { get; private set; }
        public SynchronisableAttribute(SyncFlags _flags = SyncFlags.None)
        {
            flags = _flags;
        }
    }


    public class NetCodeManager
    {
        Dictionary<RuntimeTypeHandle, SynchronisableEntityDescriptor> entityDescriptors = new Dictionary<RuntimeTypeHandle, SynchronisableEntityDescriptor>();
        SynchronisableFieldGenerator fieldGenerator = new SynchronisableFieldGenerator();
        
        public void RegisterType(Type sync_type)
        {
            entityDescriptors[sync_type.TypeHandle] = new SynchronisableEntityDescriptor(fieldGenerator, sync_type);
        }

        public void RegisterField(Type synchronisableType, Type fieldType, SyncFlags flags = SyncFlags.None)
        {
            fieldGenerator.RegisterSynchronisableFieldType(synchronisableType, fieldType, flags);
        }

        internal SynchronisableEntityDescriptor GetDescriptor(Type type)
        {
            return entityDescriptors[type.TypeHandle];
        }

    }
}
