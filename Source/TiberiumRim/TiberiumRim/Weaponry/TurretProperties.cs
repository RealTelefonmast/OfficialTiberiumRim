using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class TurretProperties
    {
        public List<TRThingDef> turrets = new List<TRThingDef>();
        public Vector3 drawOffset;
        public Vector3 barrelOffset;

        public IntRange idleDuration = new IntRange(50, 200);
        public IntRange idleInterval = new IntRange(150, 350);
    }
}
