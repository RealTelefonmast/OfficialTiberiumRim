using RimWorld;
using Verse;

namespace TR
{
    public class Comp_SonicInhibitor : ThingComp
    {
        public CompProperties_SonicInhibitor Props => (CompProperties_SonicInhibitor) base.props;

        public override void CompTickRare()
        {
            MoteMaker.MakeStaticMote(parent.Position, parent.Map, ThingDefOf.Mote_CastPsycast, Props.radius * (0.35f));
            MoteMaker.MakeStaticMote(parent.Position,parent.Map, ThingDefOf.Mote_CastPsycast, Props.radius);
            foreach (var intVec3 in GenRadial.RadialCellsAround(parent.Position, Props.radius, true))
            {
                var tib = intVec3.GetTiberium(parent.Map);
                tib?.TakeDamage(new DamageInfo(TRDamageDefOf.TRSonic, TRandom.Range(Props.damageRange)));
            }
        }

        private void StartEmission()
        {

        }

        
    }

    public class CompProperties_SonicInhibitor : CompProperties
    {
        public float radius = 10;
        public FloatRange damageRange = new FloatRange(2, 10);
        public CompProperties_SonicInhibitor()
        {
            compClass = typeof(Comp_SonicInhibitor);
        }
    }
}
