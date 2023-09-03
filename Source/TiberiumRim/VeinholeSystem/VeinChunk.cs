using Verse;

namespace TR
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
