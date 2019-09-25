using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class ThingComp_TiberiumCheck : ThingComp
    {
        public List<BodyPartRecord> partsForInfection = new List<BodyPartRecord>();
        public List<BodyPartRecord> partsForGas = new List<BodyPartRecord>();

        private int ticker = 0;
        private Pawn Pawn => parent as Pawn;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            UpdateParts();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!Pawn.Spawned)
                return;
            if (ticker <= 0)
            {
                var tib = Pawn.Position.GetTiberium(Pawn.Map);
                if (tib?.def.IsInfective ?? false)
                    HediffUtils.TryAffectPawn(Pawn, false, 250);
                ticker = 250;
            }
            ticker--;
        }

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            if (Pawn.health.hediffSet.PartIsMissing(dinfo.HitPart))
                UpdateParts();
        }

        public void UpdateParts()
        {
            partsForGas = Pawn.health.hediffSet.GetNotMissingParts().Where(p => p.def.tags.Any(t => t == BodyPartTagDefOf.BreathingPathway || t == BodyPartTagDefOf.BreathingSource)).ToList();
            partsForInfection = Pawn.health.hediffSet.GetNotMissingParts().Where(p => p.height == BodyPartHeight.Bottom && p.depth == BodyPartDepth.Outside).ToList(); ;
        }
    }

    public class CompProperties_TiberiumCheck : CompProperties
    {
        public CompProperties_TiberiumCheck()
        {
            this.compClass = typeof(ThingComp_TiberiumCheck);
        }
    }
}
