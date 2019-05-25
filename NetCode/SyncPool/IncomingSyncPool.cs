using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Synchronisers;
using NetCode.Synchronisers.Entities;
using NetCode.Payloads;
using NetCode.Util;

namespace NetCode.SyncPool
{
    public class IncomingSyncPool : SynchronisablePool
    {
        /// <summary>
        /// A list of handles that have been added during the last Synchronise() call
        /// </summary>
        public IEnumerable<SyncHandle> NewHandles { get; protected set; }
        private List<SyncHandle> newHandles = new List<SyncHandle>();
        
        public IEnumerable<SyncHandle> RemovedHandles { get; protected set; }
        private List<SyncHandle> removedHandles = new List<SyncHandle>();

        /// <summary>
        /// A list of events that have been recieved.
        /// This does not require Synchronise() to be called
        /// </summary>
        public IEnumerable<SyncEvent> Events { get { return RecievedEvents; } }

        private List<SyncEvent> RecievedEvents = new List<SyncEvent>();

        public IncomingSyncPool(NetDefinitions netDefs, ushort poolID) : base(netDefs, poolID)
        {
        }

        public void Synchronise()
        {
            foreach (SyncHandle handle in SyncHandles)
            {
                if (handle.Sync.ReferencesPending)
                {
                    handle.Sync.UpdateReferences(Context);
                }
                handle.Updated = !handle.Sync.Synchronised;
                if (handle.Updated)
                {
                    handle.Obj = handle.Sync.GetValue();
                }
            }
            
            for (int i = RecievedEvents.Count - 1; i >= 0; i--)
            {
                SyncEvent evt = RecievedEvents[i];
                switch (evt.State)
                {
                    case SyncEvent.SyncState.Cleared:
                        RecievedEvents.RemoveAt(i);
                        break;
                    case SyncEvent.SyncState.PendingReferences:
                        evt.Sync.UpdateReferences(Context);
                        if (!evt.Sync.ReferencesPending) { evt.State = SyncEvent.SyncState.Ready; }
                        break;
                }
            }

            NewHandles = newHandles;
            RemovedHandles = removedHandles;
            if (newHandles.Count > 0) { newHandles = new List<SyncHandle>(); }
            if (removedHandles.Count > 0) { removedHandles = new List<SyncHandle>(); }
        }
        
        internal void RemoveEntity(ushort entityID, uint revision)
        {
            SyncHandle handle = GetHandle(entityID);
            if (handle != null && handle.Sync.Revision < revision)
            {
                // only delete if the entity is not more up to date than the deletion revision
                RemoveHandle(entityID, revision);
                removedHandles.Add(handle);
            }
        }
        
        private void SpawnEntity(ushort entityID, ushort typeID, uint revision)
        {
            SynchronisableEntity entity = EntityGenerator.GetEntityFactory(typeID).ConstructNewEntity(revision);
            SyncHandle handle = new SyncHandle( entity, entity.GetValue(), entityID );
            newHandles.Add(handle);
            AddHandle(handle);
        }

        internal void UnpackEventDatagram(PoolEventPayload payload)
        {
            ushort typeID = payload.EventData.ReadUShort();
            SynchronisableEntity entity = EntityGenerator.GetEntityFactory(typeID).ConstructNewEntity(0);
            entity.ReadFromBuffer(payload.EventData, Context);
            RecievedEvents.Add(new SyncEvent(entity, entity.GetValue()));
        }

        internal void UnpackRevisionDatagram(PoolRevisionPayload payload, long offsetMilliseconds)
        {
            Context.Revision = payload.Revision;
            Context.ConnectionTimestampOffset = offsetMilliseconds;

            NetBuffer buffer = payload.RevisionData;

            while (buffer.Remaining >= (sizeof(byte) + sizeof(ushort)))
            {
                ushort entityID = buffer.ReadUShort();
                ushort typeID = buffer.ReadVWidth(); // This could fail if not enough bytes remaining.
                
                while (entityID >= SyncSlots.Length)
                {
                    ResizeSyncHandleArray();
                }

                bool skipUpdate = false;
                 
                SyncHandle handle = GetHandle(entityID);
                if ( handle == null )
                {
                    if (SyncSlots[entityID].Revision > Context.Revision)
                    {
                        // The revision contains content for a deleted entity.
                        skipUpdate = true;
                    }
                    else
                    {
                        // We are talking about an new entity.
                        SpawnEntity(entityID, typeID, Context.Revision);
                    }
                }
                else if (handle.Sync.TypeID != typeID)
                {
                    // We have a type missmatch.
                    if (handle.Sync.Revision < Context.Revision)
                    {
                        // Entity already exists, but is incorrect type and wrong revision
                        // Assume it should have been deleted and recreate it.
                        RemoveHandle(entityID, Context.Revision);
                        SpawnEntity(entityID, typeID, Context.Revision);
                    }
                    else
                    {
                        // Entity is new type, and has a newer revision. Do not disturb it.
                        skipUpdate = true;
                    }
                }

                if (skipUpdate)
                {
                    EntityGenerator.GetEntityFactory(typeID).SkipFromBuffer(buffer);
                }
                else
                {
                    SyncSlots[entityID].Handle.Sync.ReadFromBuffer(buffer, Context);
                }
            }
        }
    }
}
