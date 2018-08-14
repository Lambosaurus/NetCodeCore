using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;

using NetCode.SyncField;

namespace NetCode.SyncEntity
{
    internal class SyncEntityGenerator
    {
        List<SyncEntityDescriptor> entityDescriptors = new List<SyncEntityDescriptor>();
        Dictionary<RuntimeTypeHandle, SyncEntityDescriptor> entityDescriptorsByType = new Dictionary<RuntimeTypeHandle, SyncEntityDescriptor>();

        SyncFieldGenerator fieldGenerator;

        //TODO: We need a way of fingerprinting this class to verify that the other endpoints have
        //      the same encoding/decoding formatting.
        public SyncEntityGenerator(SyncFieldGenerator _fieldGenerator)
        {
            fieldGenerator = _fieldGenerator;
        }

        private ushort GetNewTypeID()
        {
            if (entityDescriptors.Count == ushort.MaxValue)
            {
                throw new NetcodeItemcountException(string.Format("There may not be more than {0} unique types registered.", ushort.MaxValue));
            }
            return (ushort)(entityDescriptors.Count);
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
        
        public bool TypeExists(ushort typeID)
        {
            return entityDescriptorsByType.Count > typeID;
        }
    }
}
