using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using  Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class VeinGasCloud : HomingThing
    {
        public override void Tick()
        {
            base.Tick();
            foreach (var intVec3 in Position.CellsAdjacent8Way())
            {
                var pawn = intVec3.GetFirstPawn(Map);
                if(pawn != null) 
                    HediffUtils.TryAffectPawn(pawn, true, 1);
            }
        }
    }
}
