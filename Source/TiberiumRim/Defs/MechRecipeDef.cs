using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
