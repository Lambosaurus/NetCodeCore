using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncField;
using NetCode.SyncEntity;
using NetCode.Payloads;

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
                if (handle.Sync.PollingRequired) { handle.Sync.PollFields(Context); }
                handle.Updated = !handle.Sync.Synchronised;
                if (handle.Updated)
                {
                    handle.Sync.PushChanges(handle.Obj);
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
                        evt.Sync.PollFields(Context);
                        if (!evt.Sync.PollingRequired) { evt.State = SyncEvent.SyncState.Ready; }
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
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(typeID);

            SyncHandle handle = new SyncHandle(
                new SynchronisableEntity(descriptor, entityID, revision),
                descriptor.ConstructObject()
                );
            
            newHandles.Add(handle);
            AddHandle(handle);
        }

        internal void UnpackEventDatagram(PoolEventPayload payload)
        {
            payload.GetEventContentBuffer(out byte[] data, out int index, out int count);

            SynchronisableEntity.ReadHeader(data, ref index, out ushort entityID, out ushort typeID);
            SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(typeID);
            SynchronisableEntity sync = new SynchronisableEntity(descriptor, entityID, 0);
            object obj = descriptor.ConstructObject();
            sync.ReadRevisionFromBuffer(data, ref index, Context);
            sync.PushChanges(obj);
            
            RecievedEvents.Add(new SyncEvent(
                sync,
                obj
                ));
        }

        internal void UnpackRevisionDatagram(PoolRevisionPayload payload, long offsetMilliseconds)
        {
            payload.GetRevisionContentBuffer(out byte[] data, out int index, out int count);

            Context.Revision = payload.Revision;
            Context.TimestampOffset = offsetMilliseconds;

            int end = index + count;
            while (index < end)
            {
                SynchronisableEntity.ReadHeader(data, ref index, out ushort entityID, out ushort typeID);

                bool skipUpdate = false;
                
                while (entityID >= SyncSlots.Length)
                {
                    ResizeSyncHandleArray();
                }
                
                SyncHandle handle = GetHandle(entityID);
                if ( handle == null )
                {
                    if (SyncSlots[entityID].Revision > Context.Revision)
                    {
                        skipUpdate = true;
                    }
                    else
                    {
                        SpawnEntity(entityID, typeID, Context.Revision);
                    }
                }
                else
                {
                    if (handle.Sync.TypeID != typeID)
                    {
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
                }

                if (skipUpdate)
                {
                    SyncEntityDescriptor descriptor = entityGenerator.GetEntityDescriptor(typeID);
                    SynchronisableEntity.SkipRevisionFromBuffer(data, ref index, descriptor);
                }
                else
                {
                    SynchronisableEntity entity = SyncSlots[entityID].Handle.Sync;
                    entity.ReadRevisionFromBuffer(data, ref index, Context);
                }
            }
        }
    }
}
