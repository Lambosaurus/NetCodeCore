using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;
using NetCode.Packet;

namespace NetCode.SyncPool
{
    public class OutgoingSyncPool : SynchronisablePool, IVersionable
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
        

        public PoolRevisionDatagram GenerateRevisionDatagram()
        {
            //TODO: Finish this.
            uint revision = GetNewRevision();
            PoolRevisionDatagram datagram = new PoolRevisionDatagram(PoolID, revision);

            byte[] data = new byte[PushToBufferSize()];
            int index = 0;
            PushToBuffer(data, ref index, revision);

            datagram.data = data;

            return datagram;
        }

        public override int PushToBufferSize()
        {
            //TODO: can possibly return a constant value here which is updated by Synchronise
            //      However, that possibly relies on Synchronisation and calls to PushToBuffer
            //      being in step.
            int size = HeaderSize();
            foreach (SyncHandle handle in SyncHandles.Values)
            {
                size += handle.sync.PushToBufferSize();
            }
            return size;
        }

        public override void PushToBuffer(byte[] data, ref int index, uint revision)
        {
            WriteHeader(data, ref index);

            foreach (SyncHandle handle in SyncHandles.Values)
            {
                handle.sync.PushToBuffer(data, ref index, revision);
            }

            Changed = false;
        }
        
        public override void PullFromBuffer(byte[] data, ref int index, uint revision)
        {
            throw new NotImplementedException("OutgoingSyncPools may not read");
        }
    }
}
