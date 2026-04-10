using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class TiberiumVein : TiberiumCrystal
    {
        public override void TickLong()
        {
            base.TickLong();
            /*
            foreach (var intVec3 in Position.CellsAdjacent8Way())
            {
                var chunk = intVec3.GetFirstHaulable(Map);
                if (chunk.IsCorruptableChunk())
                    if (TRUtils.Chance(0.019f))
                    {
                        chunk.DeSpawn();
                        var chunky = (VeinChunk) TRUtils.NewBorn(PawnKindDef.Named("VeinChunk"));
                        chunky.boundProducer = this.Parent;
                        GenSpawn.Spawn(chunky, chunk.Position, Map);
                    }
                    else
                        chunk.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 4, 2));
            }
            */
        }

    }
}
