using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Connection;
using NetCode.SyncEntity;
using NetCode.Packing;
using NetCode.Util;

namespace NetCode.SyncPool
{
    public class OutgoingSyncPool : SynchronisablePool
    {
        const uint MAX_ENTITIES = ushort.MaxValue;
        
        private List<NetworkConnection> Destinations = new List<NetworkConnection>();
        
        internal OutgoingSyncPool(SyncEntityGenerator generator, ushort poolID) : base(generator, poolID)
        {

        }
        
        public void AddDestination(NetworkConnection connection)
        {
            Destinations.Add(connection);
        }

        /// <summary>
        /// Gets the next free object ID
        /// </summary>
        private ushort lastEntityID = 0;
        private ushort GetNextEntityID()
        {
            ushort potentialEntityID = (ushort)(lastEntityID + 1);

            //TODO: This search will start to choke when the dict is nearly full of keys.
            //      Somone should be informed when this happens.

            // start searching for free keys from where we found our last free key
            // This will be empty most of the time
            while (SyncHandles.ContainsKey(potentialEntityID))
            {
                // We rely on ushort overflow to wrap search around to 0 when we hit the last value.
                potentialEntityID++;
                
                // We hit the starting point of our search. We must be out of ID's. Time to throw an exeption.
                if (potentialEntityID == lastEntityID)
                {
                    throw new NetcodeOverloadedException(string.Format("Sync pool has been filled. The pool should not contain more than {0} entities.", MAX_ENTITIES));
                }
            }
            
            lastEntityID = potentialEntityID;
            return potentialEntityID;
        }
        
        public SyncHandle RegisterEntity(object instance)
        {
            ushort entityID = GetNextEntityID();
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(instance.GetType().TypeHandle);
            SyncHandle handle = new SyncHandle(
                new SynchronisableEntity(descriptor, entityID),
                instance
                );

            SyncHandles[handle.EntityID] = handle;

            return handle;
        }

        public void Synchronise()
        {
            uint candidateRevision = Revision + 1;
            bool changesFound = TrackChanges(candidateRevision, out List<ushort> deletedEntityIDs);

            if (changesFound)
            {
                Revision = candidateRevision;
                Payload payload = GenerateRevisionPayload(Revision);
                BroadcastPayload(payload);
            }

            if (deletedEntityIDs.Count > 0)
            {
                Revision = candidateRevision;
                foreach ( ushort[] deletedIDs in deletedEntityIDs.Segment(PoolDeletionPayload.MAX_ENTITY_IDS))
                {
                    Payload payload = new PoolDeletionPayload(PoolID, Revision, deletedIDs);
                    BroadcastPayload(payload);
                }
            }
        }

        private void BroadcastPayload(Payload payload)
        {
            foreach (NetworkConnection destination in Destinations)
            {
                destination.Enqueue(payload);
            }
        }
        
        private bool TrackChanges(uint revision, out List<ushort> deletedEntities)
        {
            bool changesFound = false;
            deletedEntities = new List<ushort>();

            foreach (SyncHandle handle in SyncHandles.Values)
            {
                if (handle.State == SyncHandle.SyncState.Live)
                {
                    bool entityChanged = handle.Sync.TrackChanges(handle.Obj, revision);
                    if (entityChanged)
                    {
                        changesFound = true;
                    }
                }
                else if (handle.State == SyncHandle.SyncState.Deleted)
                {
                    deletedEntities.Add(handle.EntityID);
                }
                // SyncState.Suspended is ignored
            }
            
            foreach (ushort entityID in deletedEntities)
            {
                SyncHandles.Remove(entityID);
            }

            return changesFound;
        }
        
        internal Payload GenerateRevisionPayload(uint revision)
        {
            List<uint> updatedEntities = new List<uint>();
            
            int size = 0;
            foreach ( SyncHandle handle in SyncHandles.Values )
            {
                if (handle.Sync.ContainsRevision(revision))
                {
                    size += handle.Sync.WriteRevisionToBufferSize(revision);
                    updatedEntities.Add(handle.EntityID);
                }
            }

            if (updatedEntities.Count > 0)
            {
                PoolRevisionPayload payload = new PoolRevisionPayload(this, revision, size);
                payload.AllocateAndWrite();

                payload.GetRevisionContentBuffer(out byte[] data, out int index, out int count);
                
                foreach (ushort entityID in updatedEntities)
                {
                    SyncHandle handle = SyncHandles[entityID];
                    handle.Sync.WriteRevisionToBuffer(data, ref index, revision);
                }
                
                return payload;
            }
            return null;
        }
    }
}
