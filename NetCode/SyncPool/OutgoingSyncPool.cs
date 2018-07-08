using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Connection;
using NetCode.SyncEntity;
using NetCode.Payloads;
using NetCode.Util;

namespace NetCode.SyncPool
{
    public class OutgoingSyncPool : SynchronisablePool
    {

        private List<NetworkClient> Destinations = new List<NetworkClient>();

        internal OutgoingSyncPool(SyncEntityGenerator generator, ushort poolID) : base(generator, poolID)
        {

        }
        
        public void AddDestination(NetworkClient destination)
        {
            Destinations.Add(destination);
        }
        
        
        /// <summary>
        /// Gets the next free object ID
        /// </summary>
        private ushort lastEntityID = 0;
        private ushort GetNextEntityID()
        {
            if (ResizeSyncHandleArrayReccommended())
            {
                ResizeSyncHandleArray();
            }

            ushort potentialEntityID = (ushort)(lastEntityID + 1);
            if (potentialEntityID >= SyncSlots.Length) { potentialEntityID = 0; }
            
            //TODO: This search will start to choke when the dict is nearly full of keys.
            //      Somone should be informed when this happens.

            // start searching for free keys from where we found our last free key
            // This will be empty most of the time
            while (SyncSlots[potentialEntityID].Handle != null)
            {
                potentialEntityID++;

                if (potentialEntityID >= SyncSlots.Length)
                {
                    // Wrap potentialID
                    potentialEntityID = 0;
                }
                
                // We hit the starting point of our search. We must be out of ID's. Time to throw an exeption.
                if (potentialEntityID == lastEntityID)
                {
                    throw new NetcodeOverloadedException(string.Format("Sync pool has been filled. The pool should not contain more than {0} entities.", MAX_SYNCHANDLE_COUNT));
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

            AddHandle(handle);
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
            foreach (NetworkClient destination in Destinations)
            {
                destination.Enqueue(payload);
            }
        }
        
        private bool TrackChanges(uint revision, out List<ushort> deletedEntities)
        {
            bool changesFound = false;
            deletedEntities = new List<ushort>();

            foreach (SyncHandle handle in SyncHandles)
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
                RemoveHandle(entityID, revision);
            }

            return changesFound;
        }
        
        internal Payload GenerateRevisionPayload(uint revision)
        {
            List<uint> updatedEntities = new List<uint>();
            
            int size = 0;
            foreach ( SyncHandle handle in SyncHandles )
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
                    SyncHandle handle = SyncSlots[entityID].Handle;
                    handle.Sync.WriteRevisionToBuffer(data, ref index, revision);
                }
                
                return payload;
            }
            return null;
        }
    }
}
