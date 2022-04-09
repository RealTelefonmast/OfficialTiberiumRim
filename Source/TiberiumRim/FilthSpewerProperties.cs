using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class FilthSpewerProperties
    {
        public List<DefFloat<ThingDef>> filths;
        public float spreadRadius = 1.9f;

        public void SpawnFilth(IntVec3 center, Map map)
        {
            foreach (var cell in GenRadial.RadialCellsAround(center, spreadRadius, true))
            {
                foreach (var filth in filths)
                {
                    if (TRUtils.Chance(filth.value))
                    {
                        FilthMaker.TryMakeFilth(cell, map, filth.def, 1);
                        break;
                    }
                }
            }
        }
    }
}
