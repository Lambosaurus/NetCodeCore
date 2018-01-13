using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NetCode.SyncPool
{
    public class SyncPool
    {
        protected NetCodeManager netcode;

        public Dictionary<uint, SyncHandle> handles { get; private set; } = new Dictionary<uint, SyncHandle>();
        public ushort PoolID { get; private set; }

        
        public SyncPool(NetCodeManager _netcode, ushort poolID)
        {
            netcode = _netcode;
            PoolID = poolID;
        }
        
        protected int HeaderSize()
        {
            return sizeof(ushort);
        }

        internal static void ReadHeader(byte[] data, ref int index, out ushort poolID)
        {
            poolID = PrimitiveSerialiser.ReadUShort(data, ref index);
        }

        protected void WriteHeader(byte[] data, ref int index)
        {
            PrimitiveSerialiser.WriteUShort(data, ref index, PoolID);
        }
    }
}
