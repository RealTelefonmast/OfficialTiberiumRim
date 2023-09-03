using System.Collections.Generic;
using Verse;

namespace TR
{
    public class MechRecipeDef : Def
    {
        public MechanicalPawnKindDef mechDef;
        public List<ThingDefCountClass> costList;
        public string graphicPath;
    }
}
