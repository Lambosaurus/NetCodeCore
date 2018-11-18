using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;

namespace NetCode.SyncEntity
{
    internal class SyncEntityGenerator
    {
        List<SyncEntityDescriptor> entityDescriptors = new List<SyncEntityDescriptor>();
        Dictionary<RuntimeTypeHandle, SyncEntityDescriptor> entityDescriptorsByType = new Dictionary<RuntimeTypeHandle, SyncEntityDescriptor>();

        //TODO: We need a way of fingerprinting this class to verify that the other endpoints have
        //      the same encoding/decoding formatting.
        public SyncEntityGenerator()
        {
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
            SyncEntityDescriptor descriptor = new SyncEntityDescriptor(entityType, GetNewTypeID());
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


        public void LoadEntityTypes(string[] tags)
        {
            // Create a list of Types for each tag.
            List<Type>[] entityTypes = new List<Type>[tags.Length];
            for (int i = 0; i < tags.Length; i++)
            {
                entityTypes[i] = new List<Type>();
            }

            AttributeHelper.ForAllTypesWithAttribute<EnumerateSynchEntityAttribute>(
                (type, attribute) => {
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (attribute.Tag == tags[i])
                        {
                            entityTypes[i].Add(type);
                            break;
                        }
                    }
                });

            // The types are loaded in order of their tags for future compatibility reasons.
            foreach (List<Type> types in entityTypes)
            {
                // We add the found types in alphabetical order in case the order which they are returned from assembly.GetTypes() is not reliable.
                types.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
                foreach (Type type in types)
                {
                    RegisterEntityType(type);
                }
            }
        }
    }
}
