using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetCode.SyncPool;

using System.Reflection;

namespace NetCode.SyncField.Implementations
{
    public abstract class Boxer
    {
        public abstract List<object> Unbox( object ob );
        public abstract object Box(List<object> ob);
    }

    public class BoxerTyped<T> : Boxer
    {
        public override List<object> Unbox( object ob )
        {
            List<T> items = (List<T>)(ob);
            List<object> objs = new List<object>();
            foreach(T item in items)
            {
                objs.Add(item);
            }
            return objs;
        }

        public override object Box(List<object> objs)
        {
            List<T> items = new List<T>();
            foreach( object ob in objs )
            {
                items.Add((T)ob);
            }
            return items;
        }
    }
    
    public class SynchronisableList : SynchronisableField
    {
        private List<SynchronisableField> SyncFields;
        private List<object> values;

        private Boxer boxer;

        public void GetType( Type t )
        {
            Type subtype = t.GetGenericArguments()[0];
            Type boxertype = typeof(BoxerTyped<>).MakeGenericType(new Type[] { subtype });
            boxer = (Boxer)Activator.CreateInstance(boxertype);


            /*
            Type t = typeof(List<int>);
            PropertyInfo pi = t.GetProperty("Item");
            MethodInfo method = pi.GetGetMethod();
            object[] pars = new object[1]
            {
                0
            };
            object valOut = method.Invoke(list, pars);
            */
        }

        public override object GetValue()
        {
            return null;
        }

        public override void SetValue(object newValue)
        {
        }
        
        public override void Read(byte[] data, ref int index)
        {
            Skip(data, ref index);
        }
        
        public override void Skip(byte[] data, ref int index)
        {
            index += sizeof(byte);
        }

        public override bool ValueEqual(object newValue)
        {
            return true;
        }

        public override void Write(byte[] data, ref int index)
        {
            Skip(data, ref index);
        }

        public override int WriteToBufferSize()
        {
            return sizeof(byte);
        }
    }
}
