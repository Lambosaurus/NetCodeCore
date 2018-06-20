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
        NetworkConnection SourceConnection;

        internal IncomingSyncPool(SyncEntityGenerator generator, ushort poolID) : base(generator, poolID)
        {

        }
        
        public void SetSource(NetworkConnection connection)
        {
            if (SourceConnection != null)
            {
                SourceConnection.DetachSyncPool(this);
            }
            SourceConnection = connection;
            SourceConnection.AttachSyncPool(this);
        }

        public void Synchronise()
        {
            foreach (SyncHandle handle in SyncHandles.Values)
            {
                if (!handle.Sync.Synchronised)
                {
                    handle.Sync.PushChanges(handle.Obj);
                }
            }
        }

        internal void RemoveEntity(ushort entityID, uint revision)
        {
            if (SyncHandles.ContainsKey(entityID))
            {
                SyncHandle handle = SyncHandles[entityID];
                if (handle.Sync.Revision < revision)
                {
                    // Only abandon the entity if it has more recent content.
                    AbandonEntity(entityID);
                }
            }
        }

        private void AbandonEntity(ushort entityID)
        {
            SyncHandles[entityID].State = SyncHandle.SyncState.Deleted;
            SyncHandles.Remove(entityID);
        }

        private void SpawnEntity(ushort entityID, ushort typeID, uint revision)
        {
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(typeID);

            SyncHandles[entityID] = new SyncHandle(
                new SynchronisableEntity(descriptor, entityID, revision),
                descriptor.ConstructObject()
                );
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

                if ( SyncHandles.ContainsKey(entityID) )
                {
                    // If entityID exists, but is incorrect type:
                    // Assume old entity should have been deleted, and replace it
                    if ( SyncHandles[entityID].Sync.TypeID != typeID )
                    {
                        AbandonEntity(entityID);
                        SpawnEntity(entityID, typeID, payload.Revision);
                    }
                }
                else
                {
                    // Create new entity
                    SpawnEntity(entityID, typeID, payload.Revision);
                }
                
                SynchronisableEntity entity = SyncHandles[entityID].Sync;

                entity.ReadRevisionFromBuffer(data, ref index, payload.Revision);
            }
        }
    }
}
