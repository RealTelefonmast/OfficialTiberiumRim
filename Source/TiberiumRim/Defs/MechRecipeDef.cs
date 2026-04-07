using System.Collections.Generic;
using TR.ThingData.Pawns.MechanicalPawns;
using Verse;

namespace TR.Defs;

public class MechRecipeDef : Def
{
    //public List<IngredientCount> costList;
    public List<ThingDefCountClass> costList;
    public string graphicPath;
    public MechanicalPawnKindDef mechDef;
    public int workCost;
}