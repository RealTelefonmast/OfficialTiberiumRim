using System.Collections.Generic;
using Verse;

namespace TR;

public class MechRecipeDef : Def
{
    //public List<IngredientCount> costList;
    public List<ThingDefCountClass> costList;
    public string graphicPath;
    public MechanicalPawnKindDef mechDef;
    public int workCost;
}