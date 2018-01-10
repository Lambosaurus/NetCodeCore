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
        Dictionary<string, SynchronisableEntityDescriptor> entityDescriptors = new Dictionary<string, SynchronisableEntityDescriptor>();
        SynchronisableFieldGenerator fieldGenerator = new SynchronisableFieldGenerator();
        
        public void RegisterType(Type sync_type)
        {
            string name = sync_type.Name;
            entityDescriptors[name] = new SynchronisableEntityDescriptor(fieldGenerator, sync_type);
        }

        public void RegisterField(Type synchronisableType, Type fieldType, SyncFlags flags = SyncFlags.None)
        {
            fieldGenerator.RegisterSynchronisableField(synchronisableType, fieldType, flags);
        }

        public SynchronisableEntityDescriptor GetDescriptor(Type type)
        {
            string name = type.Name;
            return entityDescriptors[name];
        }

    }
}
