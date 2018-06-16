using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.Connection;
using NetCode.SyncEntity;
using NetCode.Packing;

namespace NetCode.SyncPool
{
    public class OutgoingSyncPool : SynchronisablePool
    {
        const uint MAX_POOL_OBJECTS = ushort.MaxValue;

        internal OutgoingSyncPool(SyncEntityGenerator generator, ushort poolID) : base(generator, poolID)
        {

        }

        private List<NetworkConnection> Destinations = new List<NetworkConnection>();

        public void AddDestination(NetworkConnection connection)
        {
            Destinations.Add(connection);
        }

        /// <summary>
        /// Gets the next free object ID
        /// </summary>
        private uint lastObjectID = 0;
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
        
        public SyncHandle RegisterEntity(object instance)
        {
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(instance.GetType().TypeHandle);
            SyncHandle handle = new SyncHandle(
                new SynchronisableEntity(descriptor, (ushort)GetNewObjectId()),
                instance
                );

            SyncHandles[handle.sync.EntityID] = handle;

            return handle;
        }

        public void Synchronise()
        {
            uint candidateRevision = Revision + 1;
            bool changesFound = TrackChanges(candidateRevision);

            if (changesFound)
            {
                Revision = candidateRevision;
                Payload payload = GenerateRevisionPayload(Revision);

                foreach (NetworkConnection destination in Destinations)
                {
                    destination.Enqueue(payload);
                }
            }
        }
        
        public bool TrackChanges(uint revision)
        {
            bool changesFound = false;

            foreach (SyncHandle handle in SyncHandles.Values)
            {
                bool entityChanged = handle.sync.TrackChanges(handle.Obj, revision);
                if (entityChanged)
                {
                    changesFound = true;
                }
            }

            return changesFound;
        }
        
        public Payload GenerateRevisionPayload(uint revision)
        {
            List<uint> updatedEntities = new List<uint>();
            
            int size = 0;
            foreach ( SyncHandle handle in SyncHandles.Values )
            {
                if (handle.sync.ContainsRevision(revision))
                {
                    size += handle.sync.WriteRevisionToBufferSize(revision);
                    updatedEntities.Add(handle.sync.EntityID);
                }
            }

            if (updatedEntities.Count > 0)
            {
                PoolRevisionPayload payload = new PoolRevisionPayload(this, revision);
                payload.AllocateContent(size);

                foreach (ushort entityID in updatedEntities)
                {
                    SyncHandle handle = SyncHandles[entityID];
                    handle.sync.WriteRevisionToBuffer(payload.Data, ref payload.DataIndex, revision);
                }
                
                return payload;
            }
            return null;
        }
    }
}
