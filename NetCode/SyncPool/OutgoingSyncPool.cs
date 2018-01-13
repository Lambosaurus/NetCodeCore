using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NetCode.SyncEntity;

namespace NetCode.SyncPool
{
    public class OutgoingSyncPool : SyncPool
    {
        const uint MAX_PACKET_LENGTH = ushort.MaxValue;

        internal OutgoingSyncPool(NetCodeManager _netcode, ushort poolID) : base(_netcode, poolID)
        {

        }

        /// <summary>
        /// Gets the next free object ID
        /// </summary>
        uint last_object_id = 0;
        private uint GetNewObjectId()
        {
            uint potential_object_id = last_object_id + 1;

            //TODO: This search will start to choke when the dict is nearly full of keys.
            // Somone should be informed when this happens.

            // start searching for free keys from where we found our last free key
            // This will be empty most of the time
            while (handles.ContainsKey(potential_object_id))
            {
                potential_object_id++;

                // Wrap search back to start of id space if we hit end of valid space
                if (potential_object_id > MAX_PACKET_LENGTH) { potential_object_id = 0; }

                // We hit the starting point of our search. We must be out of ID's. Time to throw an exeption.
                if (potential_object_id == last_object_id)
                {
                    throw new NetcodeOverloadedException(string.Format("Sync pool has been filled. The pool should not contain more than {0} items.", MAX_PACKET_LENGTH));
                }
            }

            last_object_id = potential_object_id;
            return potential_object_id;
        }


        public SyncHandle RegisterEntity(object instance)
        {
            SyncHandle handle = new SyncHandle();

            handle.obj = instance;
            handle.sync = new SynchronisableEntity(netcode.GetDescriptor(instance.GetType().TypeHandle), GetNewObjectId());
            handles[handle.sync.Uuid] = handle;

            return handle;
        }

        public void UpdateFromLocal()
        {
            foreach (SyncHandle handle in handles.Values)
            {
                handle.sync.UpdateFromLocal(handle.obj);
            }
        }

        public byte[] GenerateDeltaPacket(uint packet_id)
        {
            int packetsize = HeaderSize();
            foreach (SyncHandle handle in handles.Values)
            {
                packetsize += handle.sync.WriteSize();
            }

            int index = 0;
            byte[] data = new byte[packetsize];

            WriteHeader(data, ref index);

            foreach (SyncHandle handle in handles.Values)
            {
                handle.sync.WriteToPacket(data, ref index, packet_id);
            }

            return data;
        }
    }
}
