using System.Collections.Generic;
using TR.ThingData.Pawns.MechanicalPawns;
using Verse;

namespace TR.Defs;

public class MechUpgradeDef : Def
{
    public List<ThingDefCountClass> costList;
    public MechanicalPawnKindDef forMech;
}