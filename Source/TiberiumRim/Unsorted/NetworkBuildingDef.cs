using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

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
    public List<IntVec3> InputCells = new();
    public TNWBIOMode IOMode = TNWBIOMode.Dynamic;
    public List<IntVec3> OutputCells = new();
}