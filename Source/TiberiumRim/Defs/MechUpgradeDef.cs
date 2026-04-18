using System.Collections.Generic;
using TiberiumRim;
using Verse;

namespace TR;

public class MechUpgradeDef : Def
{
    public List<ThingDefCountClass> costList;
    public MechanicalPawnKindDef forMech;
}