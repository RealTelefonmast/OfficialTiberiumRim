using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
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
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);
            Rupture(0.5f);

            if (Pawn.health.hediffSet.PartIsMissing(dinfo.HitPart))
                Rupture(1);
        }

        public override void Notify_PawnKilled()
        {
            Rupture(0.5f);
        }

        private void Rupture(float intensity)
        {
            if (isRuptered) return;
            isRuptered = true;
            Pawn.TakeDamage(new DamageInfo(DamageDefOf.Bomb, parent.Part.def.hitPoints, 1));
            GenExplosion.DoExplosion(base.Pawn.Position, base.Pawn.Map, this.Props.explosionRadius * intensity, this.Props.damageDef, base.Pawn, this.Props.damageAmount / 2, -1f, null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
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
        public float explosionRadius;
        public DamageDef damageDef;
        public int damageAmount = -1;
    }
}
