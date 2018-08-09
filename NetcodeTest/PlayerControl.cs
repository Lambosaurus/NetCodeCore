using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using NetCode;

namespace NetcodeTest
{
    public class PlayerControl
    {
        [Synchronisable]
        public Color ShipColor;
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public float Torque;
        [Synchronisable(SyncFlags.HalfPrecisionFloats)]
        public float Thrust;
        [Synchronisable]
        public bool Ready = false;
        [Synchronisable]
        public string PlayerName = "";
    }
}
