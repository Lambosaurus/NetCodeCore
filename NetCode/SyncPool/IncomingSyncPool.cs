﻿using System;
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

        public override void PullFromBuffer(byte[] data, ref int index, uint revision)
        {
            while (index < data.Length)
            {
                uint entityID;
                ushort typeID;
                SynchronisableEntity.ReadHeader(data, ref index, out entityID, out typeID);

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

                entity.PullFromBuffer(data, ref index, revision);
            }
        }

        public void Synchronise()
        {
            foreach (SyncHandle handle in SyncHandles.Values)
            {
                handle.sync.PushToLocal(handle.Obj);
            }
        }


        public override void PushToBuffer(byte[] data, ref int index, uint revision)
        {
            throw new NotImplementedException("IncomingSyncPools may not write");
        }

        public override int PushToBufferSize()
        {
            throw new NotImplementedException("IncomingSyncPools may not write");
        }
    }
}
