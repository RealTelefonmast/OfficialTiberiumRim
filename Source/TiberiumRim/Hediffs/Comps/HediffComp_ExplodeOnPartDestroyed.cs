using RimWorld;
using Verse;

namespace TiberiumRim
{
    //TODO!! because neph wanted it make an explosion property container to re-use in all other explosion related effects, could create a method in the container to call the explosion

    public class HediffComp_ExplodeOnPartDestroyed : HediffComp
    {
        public HediffCompProperties_ExplodeOnPartDestroyed Props => (HediffCompProperties_ExplodeOnPartDestroyed) base.props;

        public override string CompLabelInBracketsExtra => isRuptered ? "TR_PartRuptured".Translate() : null;

        private bool isRuptered;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref isRuptered, "ruptured");
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            if (Pawn.health.hediffSet.PartIsMissing(dinfo.HitPart))
                Rupture(1);
            else 
                Rupture(0.5f);

            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
        }

        public override void Notify_PawnKilled()
        {
            Rupture(0.5f, false);
        }

        private void Rupture(float intensity, bool dealDamage = true)
        {
            if (isRuptered) return;
            isRuptered = true;
            Props.explosionProps.DoExplosion(Pawn.Position, Pawn.Map, this.Pawn);
            if(dealDamage) 
                Pawn.TakeDamage(new DamageInfo(DamageDefOf.Bomb, parent.Part.def.hitPoints, 1));
        }
    }

    public class HediffCompProperties_ExplodeOnPartDestroyed : HediffCompProperties
    {
        public HediffCompProperties_ExplodeOnPartDestroyed()
        {
            this.compClass = typeof(HediffComp_ExplodeOnPartDestroyed);
        }

        //public bool destroyGear;
        //public bool destroyBody;
        public ExplosionProperties explosionProps;
    }
}
