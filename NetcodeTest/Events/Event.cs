using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetCode;

using NetcodeTest.Util;


namespace NetcodeTest.Events
{
    public abstract class Event
    {
        [NetSynchronisable(SyncFlags.Timestamp)]
        public long Timestamp { get; protected set; }
                
        public abstract bool Expired();
        public abstract void Predict(long timestamp);
        public abstract void Draw(SpriteBatch batch);
    }
}
