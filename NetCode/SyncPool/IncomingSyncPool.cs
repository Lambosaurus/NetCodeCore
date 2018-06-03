using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public class IncomingSyncPool : SynchronisablePool
    {
        internal IncomingSyncPool(SyncEntityGenerator generator, ushort poolID) : base(generator, poolID)
        {

        }

        public void SpawnEntity(uint entityID, ushort typeID)
        {
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(typeID);

            SyncHandles[entityID] = new SyncHandle(
                new SynchronisableEntity(descriptor, entityID),
                descriptor.ConstructObject()
                );
        }

        public void AbandonEntity(uint entityID)
        {
            SyncHandles[entityID].state = SyncHandle.SyncState.Deleted;
            SyncHandles.Remove(entityID);
        }

        public void UnpackRevisionDatagram(PoolRevisionDatagram datagram)
        {
            int end = datagram.Start + datagram.Size;
            while (datagram.Index < end)
            {
                uint entityID;
                ushort typeID;
                SynchronisableEntity.ReadHeader(datagram.Data, ref datagram.Index, out entityID, out typeID);

                if ( SyncHandles.ContainsKey(entityID) )
                {
                    // If entityID exists, but is incorrect type:
                    // Assume old entity should have been deleted, and replace it
                    if ( SyncHandles[entityID].sync.TypeID != typeID )
                    {
                        AbandonEntity(entityID);
                        SpawnEntity(entityID, typeID);
                    }
                }
                else
                {
                    // Create new entity
                    SpawnEntity(entityID, typeID);
                }
                
                SynchronisableEntity entity = SyncHandles[entityID].sync;

                entity.PullFromBuffer(datagram.Data, ref datagram.Index, datagram.Revision);
            }
        }

        public void Synchronise()
        {
            foreach (SyncHandle handle in SyncHandles.Values)
            {
                handle.sync.PushToLocal(handle.Obj);
            }
        }
    }
}
