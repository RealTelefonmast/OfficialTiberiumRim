using System.Collections.Generic;
using Verse;

namespace TR;

public class MechUpgradeDef : Def
{
    public List<ThingDefCountClass> costList;
    public MechanicalPawnKindDef forMech;
}