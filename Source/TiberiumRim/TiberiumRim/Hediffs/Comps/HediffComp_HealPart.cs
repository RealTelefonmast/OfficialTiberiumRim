using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class HediffComp_HealPart : HediffComp
    {

    }

    public class HediffCompProperties_HealPart : HediffCompProperties
    {
        public HediffCompProperties_HealPart()
        {
            compClass = typeof(HediffComp_HealPart);
        }

        public float healRate = 1;
    }
}
