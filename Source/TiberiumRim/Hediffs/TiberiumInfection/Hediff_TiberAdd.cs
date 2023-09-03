using Verse;

namespace TR
{
    public class Hediff_Tiberadd : HediffWithComps
    {
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            if (!pawn.needs.AllNeeds.Any(n => n is Need_Tiberium))
            {
                pawn.health.RemoveHediff(this);
                pawn.health.AddHediff(TRHediffDefOf.TiberAddSide);
            }
        }
    }
}
