using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.SyncPool;

namespace NetCode.SyncField
{
    public class SyncFieldList : SynchronisableField
    {
        private List<SynchronisableField> SyncFields;
        private object ListOfFields;
        private Type t;

        public override object GetValue()
        {
            return ListOfFields;
        }

        public override void Read(byte[] data, ref int index)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object newValue)
        {
            throw new NotImplementedException();
        }

        public override void Skip(byte[] data, ref int index)
        {
            throw new NotImplementedException();
        }

        public override bool ValueEqual(object newValue)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] data, ref int index)
        {
            throw new NotImplementedException();
        }

        public override int WriteToBufferSize()
        {
            throw new NotImplementedException();
        }

        public override void PeriodicProcess(SyncContext context)
        {
            base.PeriodicProcess(context);
        }

        public override void PostProcess(SyncContext context)
        {
            base.PostProcess(context);
        }

        public override void PreProcess(SyncContext context)
        {
            base.PreProcess(context);
        }
    }
}
