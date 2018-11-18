using System;
using System.Collections.Generic;
using System.Linq;

using NetCode.Util;

namespace NetCode.SyncField
{
    public abstract class SyncFieldVariableLength : SynchronisableField
    {
        protected int SizeOfLengthHeader
        { get
            {
                return ((Flags & SyncFlags.ExtendedLength) != 0) ? sizeof(ushort) : sizeof(byte);
            }
        }

        protected void WriteLengthHeader( byte[] data, ref int index, int length )
        {
            Primitive.WriteNBytes(data, ref index, length, SizeOfLengthHeader);
        }

        protected int ReadLengthHeader( byte[] data, ref int index )
        {
            return Primitive.ReadNBytes(data, ref index, SizeOfLengthHeader);
        }
    }

}
