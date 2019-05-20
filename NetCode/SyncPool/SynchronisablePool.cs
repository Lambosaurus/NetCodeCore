using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.SyncField.Entities;

namespace NetCode.SyncPool
{
    public abstract class SynchronisablePool
    {
        public IEnumerable<SyncHandle> Handles  { get { return SyncHandles; } }
        public ushort PoolID { get; private set; }
        protected uint Revision { get; set; }
        
        private const double PoolReallocationThreshold = 0.7;
        private const int DefaultEntityCount = 32; // This should be a power of two

        public const int MaximumEntityCount = ushort.MaxValue + 1;

        public List<SynchronisablePool> LinkedPools { get; } = new List<SynchronisablePool>();

        protected struct SyncSlot
        {
            public SyncHandle Handle;
            public uint Revision;
        }

        protected SyncSlot[] SyncSlots;
        protected List<SyncHandle> SyncHandles { get; private set; }
        
        internal EntityDescriptorCache EntityGenerator;

        protected SyncContext Context;
        
        internal SynchronisablePool(NetDefinitions netDefs, ushort poolID)
        {
            EntityGenerator = netDefs.entityGenerator;
            PoolID = poolID;

            SyncHandles = new List<SyncHandle>();
            SyncSlots = new SyncSlot[DefaultEntityCount];
            Revision = 0; // TODO: There seems to be a zero revision bug causing skips. Fix this and reset starting revision to 0.

            Context = new SyncContext(this, 0, 0);
        }

        public void Clear()
        {
            foreach (SyncHandle handle in SyncHandles)
            {
                handle.State = SyncHandle.SyncState.Deleted;
            }

            SyncHandles.Clear();
            SyncSlots = new SyncSlot[DefaultEntityCount];
            Revision = 0;
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
            //if (entityID == SyncHandle.NullEntityID) { return null; }
            if (entityID < SyncSlots.Length )
            {
                return SyncSlots[entityID].Handle;
            }
            return null;
        }

        public bool HandleExists(ushort entityID)
        {
            return     entityID < SyncSlots.Length
                    && SyncSlots[entityID].Handle != null;
        }
        
        protected bool ResizeSyncHandleArrayReccommended()
        {
            return     SyncHandles.Count < MaximumEntityCount
                    && SyncHandles.Count > SyncSlots.Length * PoolReallocationThreshold;
        }

        protected void ResizeSyncHandleArray()
        {
            int newsize = SyncSlots.Length * 2;
            if (newsize > MaximumEntityCount)
            {
                throw new NetcodeItemcountException(string.Format("May not increase SyncHandleArray to more than {0}", MaximumEntityCount));
            }
            Array.Resize(ref SyncSlots, newsize);
        }

    }
}
