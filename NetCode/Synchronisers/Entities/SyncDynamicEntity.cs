using System;
using System.Collections.Generic;
using System.Linq;


namespace NetCode.Synchronisers.Entities
{
    internal class SyncDynamicEntity : Synchroniser
    { 
        protected SynchronisableEntity Entity;
        protected EntityDescriptorCache Cache;

        public SyncDynamicEntity(EntityDescriptorCache descriptorCache)
        {
            Cache = descriptorCache;
        }

        public sealed override object GetValue()
        {
            return Entity?.GetValue();
        }

        public sealed override bool ContainsRevision(uint revision)
        {
            // Note, revision must also change on deletion/creation for stability.
            if (Revision < revision)
            {
                return false;
            }
            else if (Revision == revision)
            {
                return true;
            }
            else if (Entity != null)
            {
                return Entity.ContainsRevision(revision);
            }
            return false;
        }

        public sealed override void SetSynchonised(bool sync)
        {
            Synchronised = sync;
            Entity?.SetSynchonised(sync);
        }

        public sealed override void UpdateReferences(SyncContext context)
        {
            if (Entity != null)
            {
                Entity.UpdateReferences(context);
                ReferencesPending = Entity.ReferencesPending;
            }
            else
            {
                ReferencesPending = false;
            }
        }

        private void GenerateNewEntity(RuntimeTypeHandle newType, uint revision)
        {
            Entity = Cache.GetEntityFactory(newType).ConstructNewEntity(0);
            Revision = revision;
            Synchronised = false;
            ReferencesPending = false;
        }

        private void GenerateNewEntity(ushort typeID, uint revision)
        {
            Entity = Cache.GetEntityFactory(typeID).ConstructNewEntity(0);
            Revision = revision;
            Synchronised = false;
            ReferencesPending = false;
        }

        private void ClearEntity(uint revision)
        {
            Revision = revision;
            Entity = null;
            Synchronised = false;
            ReferencesPending = false;
        }

        public override bool TrackChanges(object newValue, SyncContext context)
        {
            bool changesFound = false;
            if (Entity == null)
            {
                if (newValue == null)
                {
                    return false;
                }
                else
                {
                    RuntimeTypeHandle newType = newValue.GetType().TypeHandle;
                    GenerateNewEntity(newType, context.Revision);
                    changesFound = true;
                }
            }
            else if (Entity.Value != newValue)
            {
                if (newValue == null)
                {
                    changesFound = true;
                    ClearEntity(context.Revision);
                }
                else
                {
                    RuntimeTypeHandle newType = newValue.GetType().TypeHandle;
                    if (!Entity.TypeMatches(newType))
                    {
                        changesFound = true;
                        GenerateNewEntity(newType, context.Revision);
                    }
                }
            }

            if (Entity != null)
            {
                if (Entity.TrackChanges(newValue, context))
                {
                    changesFound = true;
                    Synchronised &= Entity.Synchronised;
                    Revision = context.Revision;
                }
            }
            return changesFound;
        }

        public sealed override void WriteToBuffer(NetBuffer buffer, SyncContext context)
        {
            if (Entity == null)
            {
                buffer.WriteVWidth(EntityDescriptor.NullTypeID);
            }
            else
            {
                buffer.WriteVWidth(Entity.TypeID);
                Entity.WriteToBuffer(buffer, context);
            }
        }

        public sealed override void WriteToBuffer(NetBuffer buffer)
        {
            if (Entity == null)
            {
                buffer.WriteVWidth(EntityDescriptor.NullTypeID);
            }
            else
            {
                buffer.WriteVWidth(Entity.TypeID);
                Entity.WriteToBuffer(buffer);
            }
        }

        public sealed override int WriteToBufferSize(uint revision)
        {
            if (Entity == null)
            {
                return NetBuffer.SizeofVWidth(EntityDescriptor.NullTypeID);
            }
            else
            {
                int size = NetBuffer.SizeofVWidth(Entity.TypeID);
                size += Entity.WriteToBufferSize(revision);
                return size;
            }
        }

        public sealed override int WriteToBufferSize()
        {
            if (Entity == null)
            {
                return NetBuffer.SizeofVWidth(EntityDescriptor.NullTypeID);
            }
            else
            {
                int size = NetBuffer.SizeofVWidth(Entity.TypeID);
                size += Entity.WriteToBufferSize();
                return size;
            }
        }

        public sealed override void ReadFromBuffer(NetBuffer buffer, SyncContext context)
        {
            ushort typeID = buffer.ReadVWidth();

            if (context.Revision > Revision)
            {
                if (typeID == EntityDescriptor.NullTypeID)
                {
                    if (Entity != null)
                    {
                        ClearEntity(context.Revision);
                    }
                }
                else if (Entity == null || Entity.TypeID != typeID)
                {
                    GenerateNewEntity(typeID, context.Revision);
                }

                if (Entity != null)
                {
                    Entity.ReadFromBuffer(buffer, context);
                    Synchronised &= Entity.Synchronised;
                    ReferencesPending |= Entity.ReferencesPending;
                    Revision = context.Revision;
                }
            }
            else
            {
                bool skip = Entity == null || Entity.TypeID != typeID;
                
                if (skip)
                {
                    if (typeID != EntityDescriptor.NullTypeID)
                    {
                        Cache.GetEntityFactory(typeID).SkipFromBuffer(buffer);
                    }
                }
                else
                { 
                    Entity.ReadFromBuffer(buffer, context);
                    Synchronised &= Entity.Synchronised;
                    ReferencesPending |= Entity.ReferencesPending;
                    Revision = context.Revision;
                }
            }
        }

        public sealed override void SkipFromBuffer(NetBuffer buffer)
        {
            ushort typeID = buffer.ReadVWidth();
            if (typeID != EntityDescriptor.NullTypeID)
            {
                Cache.GetEntityFactory(typeID).SkipFromBuffer(buffer);
            }
        }
    }
}
