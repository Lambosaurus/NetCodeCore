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

        public void ReadDeltaPacket(byte[] data, ref int index)
        {
            while (index < data.Length)
            {
                SynchronisableEntity.ReadHeader(data, ref index, out uint entityID, out ushort typeID);

                if ( Handles.ContainsKey(entityID) )
                {
                    if ( Handles[entityID].sync.TypeID != typeID )
                    {
                        AbandonEntity(entityID);
                        SpawnEntity(entityID, typeID);
                    }
                }
                else
                {
                    SpawnEntity(entityID, typeID);
                }

                SynchronisableEntity entity = Handles[entityID].sync;

                //entity.ReadFromPacket();
            }
        }
    }
}
