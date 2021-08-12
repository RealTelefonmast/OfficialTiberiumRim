using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class HediffRelativeDef : HediffDef
    {
        public List<PawnCapacityModifier> relativeCapMods = new List<PawnCapacityModifier>();
        public float relativePartEfficiency = 0;
        public float relativePainFactor = 0;
        public int capacityInterval = 750;
    }
}
