using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public enum TNWBIOMode
    {
        ForceInput,
        ForceOutput,
        Dynamic,
        Static,
        None
    } 

    public class NetworkBuildingDef : FXBuildingDef
    {
        public TNWBIOMode IOMode = TNWBIOMode.Dynamic;
        public List<IntVec3> InputCells = new List<IntVec3>();
        public List<IntVec3> OutputCells = new List<IntVec3>();


    }
}
