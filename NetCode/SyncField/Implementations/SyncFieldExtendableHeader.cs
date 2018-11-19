using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCode.SyncField.Implementations
{
    public abstract class SyncFieldExtendableHeader : SynchronisableField
    {
        protected byte SizeOfLengthHeader;

        internal override void Initialise(SyncFieldDescriptor descriptor, byte elementDepth)
        {
            Flags = descriptor.Flags;
            SizeOfLengthHeader = (byte)(((Flags & SyncFlags.ExtendedLength) != 0) ? sizeof(ushort) : sizeof(byte));
        }
    }
}
