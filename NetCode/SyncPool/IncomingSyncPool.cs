using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public class IncomingSyncPool : SyncPool
    {
        internal IncomingSyncPool(SyncEntityGenerator generator, ushort poolID) : base(generator, poolID)
        {

        }

        public void SpawnEntity(uint entityID, ushort typeID)
        {
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(typeID);

            Handles[entityID] = new SyncHandle(
                new SynchronisableEntity(descriptor, entityID),
                descriptor.ConstructObject()
                );
        }

        public void AbandonEntity(uint entityID)
        {
            Handles[entityID].state = SyncHandle.SyncState.Deleted;
            Handles.Remove(entityID);
        }

        public void ReadDeltaPacket(byte[] data, ref int index, uint packetID)
        {
            while (index < data.Length)
            {
                uint entityID;
                ushort typeID;
                SynchronisableEntity.ReadHeader(data, ref index, out entityID, out typeID);

                if ( Handles.ContainsKey(entityID) )
                {
                    // If entityID exists, but is incorrect type:
                    // Assume old entity should have been deleted, and replace it
                    if ( Handles[entityID].sync.TypeID != typeID )
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
                
                SynchronisableEntity entity = Handles[entityID].sync;

                entity.ReadFromPacket(data, ref index, packetID);
            }
        }

        public void UpdateToLocal()
        {
            foreach (SyncHandle handle in Handles.Values)
            {
                handle.sync.UpdateToLocal(handle.Obj);
            }
        }
    }
}
