using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public class MechRecipeDef : Def
    {
        public MechanicalPawnKindDef mechDef;
        public List<ThingDefCountClass> costList;
        public string graphicPath;
    }
}
