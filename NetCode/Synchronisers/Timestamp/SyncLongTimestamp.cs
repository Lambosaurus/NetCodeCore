using System;
using System.Collections.Generic;
using System.Linq;


namespace NetCode.Synchronisers.Timestamp
{
    public class SyncLongTimestamp : Synchroniser
    {
        protected long value;
        public sealed override object GetValue()
        {
            return value;
        }

        public sealed override void ReadFromBuffer(NetBuffer buffer, SyncContext context)
        {
            if ( Revision < context.Revision )
            {
                value = buffer.ReadLong() - context.ConnectionTimestampOffset;
                Synchronised = false;
            }
            else
            {
                SkipFromBuffer(buffer);
            }
        }

        public sealed override void SkipFromBuffer(NetBuffer buffer)
        {
            buffer.Index += sizeof(long);
        }

        public sealed override bool TrackChanges(object newValue, SyncContext context)
        {
            long v = (long)newValue;
            if (v != value)
            {
                value = v;
                Revision = context.Revision;
                Synchronised = false;
                return true;
            }
            return false;
        }

        public sealed override void WriteToBuffer(NetBuffer buffer)
        {
            buffer.WriteLong(value);
        }

        public sealed override int WriteToBufferSize()
        {
            return sizeof(long);
        }
    }
}
