using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class RefineryDef : NetworkBuildingDef
    {
        public MechanicalPawnKindDef harvester;
        public float flowAmount = 0.5f;
    }
}
