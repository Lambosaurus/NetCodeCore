using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using NetCode;

namespace NetcodeTest
{
    [EnumerateSynchEntity]
    public class PlayerControl
    {
        [Synchronisable]
        public Color ShipColor;
        [Synchronisable(SyncFlags.HalfPrecision)]
        public float Torque;
        [Synchronisable(SyncFlags.HalfPrecision)]
        public float Thrust;
        [Synchronisable]
        public bool Ready = false;
        [Synchronisable]
        public string PlayerName = "";
        [Synchronisable]
        public bool Firing = false;
    }
}
