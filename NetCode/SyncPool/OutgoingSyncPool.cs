using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncField;
using NetCode.Connection;
using NetCode.SyncEntity;
using NetCode.Payloads;
using NetCode.Util;

namespace NetCode.SyncPool
{
    public class OutgoingSyncPool : SynchronisablePool
    {
        private List<NetworkClient> Subscribers = new List<NetworkClient>();
        private ushort lastEntityID = 0;

        public List<OutgoingSyncPool> LinkedPools { get; private set; } = new List<OutgoingSyncPool>();
        internal override IEnumerable<SynchronisablePool> ResourceSyncPools { get { return LinkedPools; } }

        public OutgoingSyncPool(NetDefinitions netDefs, ushort poolID) : base(netDefs, poolID)
        {
        }
        
        internal void Subscribe(NetworkClient destination)
        {
            Subscribers.Add(destination);
        }
        
        internal void Unsubscribe(NetworkClient destination)
        {
            Subscribers.Remove(destination);
        }
        
        /// <summary>
        /// Gets the next free object ID
        /// </summary>
        private ushort GetNextEntityID()
        {
            if (ResizeSyncHandleArrayReccommended())
            {
                ResizeSyncHandleArray();
            }

            ushort potentialEntityID = (ushort)(lastEntityID + 1);
            if (potentialEntityID >= SyncSlots.Length)
            {
                // This is not zero, to respect SyncHandle.NullEntityID = 0
                potentialEntityID = 1;
            }
            
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
                    throw new NetcodeItemcountException(string.Format("Sync pool has been filled. The pool should not contain more than {0} entities.", MaximumEntityCount));
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

        public void RegisterEvent(object instance, bool acknowledgementRequired = true, bool immediateTransmitRequired = true)
        {
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(instance.GetType().TypeHandle);
            SynchronisableEntity sync = new SynchronisableEntity(descriptor, 0);

            sync.TrackChanges(instance, Context);
            int size = sync.WriteAllToBufferSize();
            PoolEventPayload payload = PoolEventPayload.Generate(PoolID, size, acknowledgementRequired, immediateTransmitRequired);
            payload.GetEventContentBuffer(out byte[] data, out int index, out int count);
            sync.WriteAllToBuffer(data, ref index);

            BroadcastPayload(payload);
        }

        public void Synchronise()
        {
            uint candidateRevision = Revision + 1;
            
            Context.Revision = candidateRevision;
            bool changesFound = TrackChanges(Context, out List<ushort> deletedEntityIDs);

            if (changesFound)
            {
                Revision = candidateRevision;
                Payload payload = GenerateRevisionPayload(Context.Revision);
                BroadcastPayload(payload);
            }

            if (deletedEntityIDs.Count > 0)
            {
                Revision = candidateRevision;
                foreach ( ushort[] deletedIDs in deletedEntityIDs.Segment(PoolDeletionPayload.MAX_ENTITY_IDS))
                {
                    Payload payload = PoolDeletionPayload.Generate(PoolID, Revision, deletedIDs);
                    BroadcastPayload(payload);
                }
            }
        }

        private void BroadcastPayload(Payload payload)
        {
            foreach (NetworkClient clients in Subscribers)
            {
                clients.Enqueue(payload);
            }
        }
        
        private bool TrackChanges(SyncContext context, out List<ushort> deletedEntities)
        {
            bool changesFound = false;
            deletedEntities = new List<ushort>();

            foreach (SyncHandle handle in SyncHandles)
            {
                handle.Updated = false;
                switch (handle.State)
                {
                    case SyncHandle.SyncState.SyncOnce:
                        handle.State = SyncHandle.SyncState.Suspended;
                        goto case SyncHandle.SyncState.Live; // I wish fallthrough was supported.

                    case SyncHandle.SyncState.Live:
                        if (handle.Sync.TrackChanges(handle.Obj, context))
                        {
                            handle.Updated = true;
                            changesFound = true;
                        }
                        break;

                    case SyncHandle.SyncState.Deleted:
                        deletedEntities.Add(handle.EntityID);
                        break;
                }
            }
                
            foreach (ushort entityID in deletedEntities)
            {
                RemoveHandle(entityID, context.Revision);
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
                PoolRevisionPayload payload = PoolRevisionPayload.Generate(this, revision, size);

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

        internal Payload GenerateCompleteStatePayload()
        {
            int size = 0;
            foreach (SyncHandle handle in SyncHandles)
            {
                size += handle.Sync.WriteAllToBufferSize();
            }
            
            PoolRevisionPayload payload = PoolRevisionPayload.Generate(this, Revision, size);

            payload.GetRevisionContentBuffer(out byte[] data, out int index, out int count);

            foreach (SyncHandle handle in SyncHandles)
            {
                handle.Sync.WriteAllToBuffer(data, ref index);
            }

            return payload;
        }
    }
}
