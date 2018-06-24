using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public abstract class SynchronisablePool
    {
        public IEnumerable<SyncHandle> Handles  { get {  return SyncHandles; } }
        public ushort PoolID { get; private set; }
        public uint Revision { get; protected set; }
        
        private const double POOL_REALLOCATION_THRESHOLD = 0.8;
        public const int MAX_SYNCHANDLE_COUNT = ushort.MaxValue + 1;
        private const int DEFAULT_SYNCHANDLE_COUNT = 32; // This should be a power of two


        
        protected struct SyncSlot
        {
            public SyncHandle Handle;
            public uint Revision;
        }

        protected SyncSlot[] SyncSlots;
        protected List<SyncHandle> SyncHandles { get; private set; }
        

        internal SyncEntityGenerator entityGenerator;

        internal SynchronisablePool(SyncEntityGenerator generator, ushort poolID)
        {
            entityGenerator = generator;
            PoolID = poolID;

            SyncHandles = new List<SyncHandle>();
            SyncSlots = new SyncSlot[DEFAULT_SYNCHANDLE_COUNT];
        }

        public SyncHandle GetHandleByObject(object obj)
        {
            foreach ( SyncHandle handle in SyncHandles)
            {
                if (handle.Obj == obj)
                {
                    return handle;
                }
            }
            return null;
        }

        protected void AddHandle(SyncHandle handle)
        {
            SyncSlots[handle.EntityID].Handle = handle;
            SyncHandles.Add(handle);
        }

        protected void RemoveHandle(ushort entityID, uint revision)
        {
            SyncHandle handle = SyncSlots[entityID].Handle;
            handle.State = SyncHandle.SyncState.Deleted;
            SyncHandles.Remove(handle);
            SyncSlots[entityID].Handle = null;
            SyncSlots[entityID].Revision = revision;
        }

        public SyncHandle GetHandle(ushort entityID)
        {
            if (entityID < SyncSlots.Length && SyncSlots[entityID].Handle != null)
            {
                return SyncSlots[entityID].Handle;
            }
            return null;
        }

        public bool HandleExists(ushort entityID)
        {
            return entityID < SyncSlots.Length && SyncSlots[entityID].Handle != null;
        }
        
        protected bool ResizeSyncHandleArrayReccommended()
        {
            return SyncHandles.Count < MAX_SYNCHANDLE_COUNT
                && SyncHandles.Count > SyncSlots.Length * POOL_REALLOCATION_THRESHOLD;
        }

        protected void ResizeSyncHandleArray()
        {
            int newsize = SyncSlots.Length * 2;
            if (newsize > MAX_SYNCHANDLE_COUNT)
            {
                throw new NetcodeOverloadedException(string.Format("May not increase SyncHandleArray to more than {0}", MAX_SYNCHANDLE_COUNT));
            }
            Array.Resize(ref SyncSlots, newsize);
        }
    }
}
