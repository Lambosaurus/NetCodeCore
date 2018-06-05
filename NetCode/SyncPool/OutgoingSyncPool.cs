using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;
using NetCode.Payloads;

namespace NetCode.SyncPool
{
    public class OutgoingSyncPool : SynchronisablePool
    {
        const uint MAX_POOL_OBJECTS = ushort.MaxValue;

        internal OutgoingSyncPool(SyncEntityGenerator generator, ushort poolID) : base(generator, poolID)
        {

        }

        /// <summary>
        /// Gets the next free object ID
        /// </summary>
        uint lastObjectID = 0;
        private uint GetNewObjectId()
        {
            uint potentialObjectID = lastObjectID + 1;

            //TODO: This search will start to choke when the dict is nearly full of keys.
            //      Somone should be informed when this happens.

            // start searching for free keys from where we found our last free key
            // This will be empty most of the time
            while (SyncHandles.ContainsKey(potentialObjectID))
            {
                potentialObjectID++;

                // Wrap search back to start of id space if we hit end of valid space
                if (potentialObjectID > MAX_POOL_OBJECTS) { potentialObjectID = 0; }

                // We hit the starting point of our search. We must be out of ID's. Time to throw an exeption.
                if (potentialObjectID == lastObjectID)
                {
                    throw new NetcodeOverloadedException(string.Format("Sync pool has been filled. The pool should not contain more than {0} items.", MAX_POOL_OBJECTS));
                }
            }

            lastObjectID = potentialObjectID;
            return potentialObjectID;
        }

        uint lastRevision = 0;
        private uint GetNewRevision()
        {
            return lastRevision++;
        }
        
        public SyncHandle RegisterEntity(object instance)
        {
            Changed = true;

            SyncHandle handle = new SyncHandle(
                new SynchronisableEntity(entityGenerator.GetEntityDescriptor(instance.GetType().TypeHandle), GetNewObjectId()),
                instance
                );

            SyncHandles[handle.sync.EntityID] = handle;

            return handle;
        }

        public void Synchronise()
        {
            foreach (SyncHandle handle in SyncHandles.Values)
            {
                handle.sync.PullFromLocal(handle.Obj);
                if (handle.sync.Changed) { Changed = true; }
            }
        }
        

        public PoolRevisionPayload GenerateRevisionDatagram()
        {
            uint revision = GetNewRevision();

            int size = 0;
            foreach (SyncHandle handle in SyncHandles.Values)
            {
                size += handle.sync.PushToBufferSize();
            }

            PoolRevisionPayload payload = new PoolRevisionPayload(PoolID, revision);
            payload.AllocateContent(size);

            foreach (SyncHandle handle in SyncHandles.Values)
            {
                handle.sync.PushToBuffer(payload.Data, ref payload.Index, revision);
            }

            Changed = false;
            
            return payload;
        }
    }
}
