using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;
using NetCode.SyncField.Entities;

namespace NetCode.SyncField.Entities
{
    internal class EntityDescriptorCache
    {
        private List<SyncFieldEntityFactory> EntityFactories = new List<SyncFieldEntityFactory>();
        private Dictionary<RuntimeTypeHandle, SyncFieldEntityFactory> EntityFactoriesByType = new Dictionary<RuntimeTypeHandle, SyncFieldEntityFactory>();

        private FieldDescriptorCache FieldCache;

        //TODO: We need a way of fingerprinting this class to verify that the other endpoints have
        //      the same encoding/decoding formatting.
        public EntityDescriptorCache()
        {
            FieldCache = new FieldDescriptorCache(this);
        }

        private ushort GetNewTypeID()
        {
            if (EntityFactories.Count == ushort.MaxValue)
            {
                throw new NetcodeItemcountException(string.Format("There may not be more than {0} unique types registered.", ushort.MaxValue));
            }
            return (ushort)(EntityFactories.Count);
        }

        public SyncFieldEntityFactory GetEntityFactory(ushort typeID)
        {
            if (typeID < EntityFactories.Count)
            {
                return EntityFactories[typeID];
            }

            throw new NetcodeGenerationException(string.Format(
                "No definitions are found for entityID {0}. Are both SyncPools using the same NetDefinitions?",
                typeID
                ));
        }

        public SyncFieldEntityFactory GetEntityFactory(RuntimeTypeHandle typeHandle)
        {
            if (EntityFactoriesByType.TryGetValue(typeHandle, out SyncFieldEntityFactory descriptor))
            {
                return descriptor;
            }

            throw new NetcodeGenerationException(string.Format(
                "No definitions are found for type {0}. Have you enumerated this type as a SyncEntity?",
                typeHandle.GetType().FullName
                ));
        }
        
        public bool TypeExists(ushort typeID)
        {
            return EntityFactoriesByType.Count > typeID;
        }


        public void LoadEntityTypes(string[] tags)
        {
            // Create a list of Types for each tag.
            List<Type>[] entityTypesByTag = new List<Type>[tags.Length];
            for (int i = 0; i < tags.Length; i++)
            {
                entityTypesByTag[i] = new List<Type>();
            }
            
            AttributeHelper.ForAllTypesWithAttribute<EnumerateSynchEntityAttribute>(
                (type, attribute) => {
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (attribute.Tag == tags[i])
                        {
                            entityTypesByTag[i].Add(type);
                            break;
                        }
                    }
                });

            // Flatten the types into one list.
            List<Type> entityTypes = new List<Type>();
            
            // The types are loaded in order of their tags for future compatibility reasons.
            foreach (List<Type> types in entityTypesByTag)
            {
                // We sort the types in alphabetical order in case the order which they are returned from assembly.GetTypes() is not consistant.
                types.Sort((a, b) => { return a.Name.CompareTo(b.Name); });
                entityTypes.AddRange(types);
            }

            // We snapshot this, because we need to loop over all recent additions.
            int lastEntityIndex = EntityFactories.Count;

            // Create all the descriptors and give them ID's
            foreach (Type type in entityTypes)
            {
                SyncFieldEntityFactory factory = new SyncFieldEntityFactory(new EntityDescriptor(GetNewTypeID()));
                EntityFactories.Add(factory);
                EntityFactoriesByType[type.TypeHandle] = factory;
            }

            // For all the newly added descriptors, we set them up.
            foreach (Type type in entityTypes)
            {
                // We must do this AFTER all types are given EntityID's, to resolve nested entities.
                EntityFactories[lastEntityIndex++].Descriptor.GenerateFieldDescriptors(type, FieldCache);
            }
        }
    }
}
