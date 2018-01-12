using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NetCode
{
    public class SyncPool
    {
        protected NetCodeManager netcode;

        public List<SyncHandle> handles { get; private set; } = new List<SyncHandle>();

        public SyncPool(NetCodeManager _netcode)
        {
            netcode = _netcode;
        }

        
        protected int HeaderSize()
        {
            return sizeof(uint);
        }

    }

    public class IncomingSyncPool : SyncPool
    {
        public IncomingSyncPool(NetCodeManager _netcode) : base(_netcode)
        {

        }
        
        private void ReadHeader(byte[] data, ref int index, out uint packet_id)
        {
            packet_id = PrimitiveSerialiser.ReadUInt(data, ref index);
        }
        
        public void ParseDeltaPacket(byte[] data, int index)
        {
            uint packet_id;
            ReadHeader(data, ref index, out packet_id);

            
        }
    }

    public class OutgoingSyncPool : SyncPool
    {
        public OutgoingSyncPool(NetCodeManager _netcode) : base(_netcode)
        {

        }
        
        uint last_object_id = 0;
        private uint GetNewObjectId()
        {
            return last_object_id++;
        }

        uint last_packet_id = 0;
        private uint GetNewPacketId()
        {
            return last_packet_id++;
        }
        
        public SyncHandle RegisterEntity(object instance)
        {
            SyncHandle handle = new SyncHandle();

            handle.obj = instance;
            handle.sync = new SyncEntity( netcode.GetDescriptor(instance.GetType()), GetNewObjectId());
            handles.Add(handle);

            return handle;
        }

        private void WriteHeader(byte[] data, ref int index, uint packet_id)
        {
            PrimitiveSerialiser.Write(data, ref index, packet_id);
        }

        public void UpdateFromLocal()
        {
            foreach (SyncHandle handle in handles)
            {
                handle.sync.UpdateFromLocal(handle.obj);
            }
        }
        
        public byte[] GenerateDeltaPacket()
        {
            int packetsize = HeaderSize();
            foreach (SyncHandle handle in handles)
            {
                packetsize += handle.sync.WriteSize();
            }

            int index = 0;
            byte[] data = new byte[packetsize];
            
            uint packet_id = GetNewPacketId();
            WriteHeader(data, ref index, packet_id);

            foreach (SyncHandle handle in handles)
            {
                handle.sync.WriteToPacket(data, ref index, packet_id);
            }

            return data;
        }
    }
}
