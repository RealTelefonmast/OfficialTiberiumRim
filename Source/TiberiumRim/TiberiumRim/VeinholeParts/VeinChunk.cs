using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class VeinChunk : TiberiumPawn
    {
        public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("VeinTiberiumChunk"), Position, Map);
            DeSpawn();
        }
    }
}
