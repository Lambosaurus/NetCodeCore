using System;
using System.Collections.Generic;
using System.Linq;
using NetCode.SyncEntity;
using NetCode.Connection;
using NetCode.Packing;

namespace NetCode.SyncPool
{
    public class IncomingSyncPool : SynchronisablePool
    {
        NetworkClient SourceClient;

        internal IncomingSyncPool(SyncEntityGenerator generator, ushort poolID) : base(generator, poolID)
        {

        }
        
        public void SetSource(NetworkClient source)
        {
            if (SourceClient != null)
            {
                SourceClient.DetachSyncPool(this);
            }
            SourceClient = source;
            SourceClient.AttachSyncPool(this);
        }

        public void Synchronise()
        {
            foreach (SyncHandle handle in SyncHandles)
            {
                if (!handle.Sync.Synchronised)
                {
                    handle.Sync.PushChanges(handle.Obj);
                }
            }
        }


        internal void RemoveEntity(ushort entityID, uint revision)
        {
            SyncHandle handle = GetHandle(entityID);
            if (handle != null && handle.Sync.Revision < revision)
            {
                // only delete if the entity is not more up to date than the deletion revision
                RemoveHandle(entityID, revision);
            }
        }
        
        private void SpawnEntity(ushort entityID, ushort typeID, uint revision)
        {
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(typeID);

            SyncHandle handle = new SyncHandle(
                new SynchronisableEntity(descriptor, entityID, revision),
                descriptor.ConstructObject()
                );

            AddHandle(handle);
        }
        
        internal void UnpackRevisionDatagram(PoolRevisionPayload payload)
        {
            payload.GetRevisionContentBuffer(out byte[] data, out int index, out int count);
            int end = index + count;
            while (index < end)
            {
                ushort entityID;
                ushort typeID;
                SynchronisableEntity.ReadHeader(data, ref index, out entityID, out typeID);

                bool skipUpdate = false;
                
                while (entityID >= SyncSlots.Length)
                {
                    ResizeSyncHandleArray();
                }
                
                SyncHandle handle = GetHandle(entityID);
                if ( handle == null )
                {
                    if (SyncSlots[entityID].Revision > payload.Revision)
                    {
                        skipUpdate = true;
                    }
                    else
                    {
                        SpawnEntity(entityID, typeID, payload.Revision);
                    }
                }
                else
                {
                    if (handle.Sync.TypeID != typeID)
                    {
                        if (handle.Sync.Revision < payload.Revision)
                        {
                            // Entity already exists, but is incorrect type and wrong revision
                            // Assume it should have been deleted and recreate it.
                            RemoveHandle(entityID, payload.Revision);
                            SpawnEntity(entityID, typeID, payload.Revision);
                        }
                        else
                        {
                            // Entity is new type, and has a newer revision. Do not disturb it.
                            skipUpdate = true;
                        }
                    }
                }

                if (skipUpdate)
                {
                    SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(typeID);
                    SynchronisableEntity.SkipRevisionFromBuffer(data, ref index, descriptor);
                }
                else
                {
                    SynchronisableEntity entity = SyncSlots[entityID].Handle.Sync;
                    entity.ReadRevisionFromBuffer(data, ref index, payload.Revision);
                }
            }
        }
    }
}
