using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;

using NetCode.SyncField;

namespace NetCode.SyncEntity
{
    internal class SyncEntityGenerator
    {
        List<SyncEntityDescriptor> entityDescriptors = new List<SyncEntityDescriptor>();
        Dictionary<RuntimeTypeHandle, SyncEntityDescriptor> entityDescriptorsByType = new Dictionary<RuntimeTypeHandle, SyncEntityDescriptor>();

        SyncFieldGenerator fieldGenerator;

        public SyncEntityGenerator(SyncFieldGenerator _fieldGenerator)
        {
            fieldGenerator = _fieldGenerator;
        }

        private ushort GetNewTypeID()
        {
            if (entityDescriptors.Count == ushort.MaxValue)
            {
                throw new NetcodeOverloadedException(string.Format("There may not be more than {0} unique types registered.", ushort.MaxValue));
            }
            return (ushort)(entityDescriptors.Count + 1);
        }

        public void RegisterEntityType(Type entityType)
        {
            SyncEntityDescriptor descriptor = new SyncEntityDescriptor(fieldGenerator, entityType, GetNewTypeID());
            entityDescriptors.Add(descriptor);
            entityDescriptorsByType[entityType.TypeHandle] = descriptor;
        }

        public SyncEntityDescriptor GetEntityDescriptor(ushort typeID)
        {
            return entityDescriptors[typeID];
        }

        public SyncEntityDescriptor GetEntityDescriptor(RuntimeTypeHandle typeHandle)
        {
            return entityDescriptorsByType[typeHandle];
        }
    }
}
