using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using NetCode;

namespace NetcodeTest
{
    [NetSynchronisableEntity]
    public class PlayerControl
    {
        [NetSynchronisable]
        public Color ShipColor;
        [NetSynchronisable(SyncFlags.HalfPrecision)]
        public float Torque;
        [NetSynchronisable(SyncFlags.HalfPrecision)]
        public float Thrust;
        [NetSynchronisable]
        public bool Ready = false;
        [NetSynchronisable]
        public string PlayerName = "";
        [NetSynchronisable]
        public bool Firing = false;
    }
}
