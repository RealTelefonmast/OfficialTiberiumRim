using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class Comp_TRHealthCheck : ThingComp
    {
        public List<BodyPartRecord> partsForInfection = new List<BodyPartRecord>();
        public List<BodyPartRecord> partsForGas = new List<BodyPartRecord>();
        public List<BodyPartRecord> partsForMutation = new List<BodyPartRecord>();

        private int ticker = 0;
        private bool canBeAffected = true;

        private Pawn Pawn => parent as Pawn;

        private TiberiumHediffGrid Grid => Pawn.MapHeld.Tiberium().TiberiumAffecter.hediffGrid;
        
        public bool IsTiberiumImmune => false;

        //TODO: reduce calls
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            UpdateParts();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!Pawn.Spawned || !canBeAffected) return;
            if (ticker <= 0)
            {
                if (Grid.IsAffected(Pawn.Position))
                {
                    //Player's pawns should cause a notification
                    if (Pawn.Faction?.IsPlayer ?? false)
                        GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.TiberiumExposure);

                    HediffUtils.TryIrradiatePawn(Pawn, Grid.RadiationAt(Pawn.Position), 250);
                    HediffUtils.TryInfectPawn(Pawn, Grid.InfectivityAt(Pawn.Position), false, 250);
                }
                ticker = 250;
            }
            ticker--;
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!Pawn.Spawned || !canBeAffected) return;
            if (Pawn.ParentHolder is Corpse corpse)
            {
                var tib = corpse.Position.GetTiberium(corpse.Map);
                if (tib != null && tib is TiberiumVein vein)
                    WrapCorpse(corpse, vein);
            }
        }

        private VeinholeFood WrapCorpse(Corpse pawn, TiberiumVein vein)
        {
            ThingDef veinCorpse = pawn.InnerPawn.VeinCorpseDef();
            VeinholeFood corpse = (VeinholeFood) ThingMaker.MakeThing(veinCorpse, null); 
            GenSpawn.Spawn(corpse, pawn.Position, pawn.Map);
            corpse.AddCorpse(pawn, vein.Parent as Veinhole);
            return corpse;
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
            //partsForMutation = Pawn.health.hediffSet.GetWanderParts(Pawn.health.hediffSet.GetHediffs<Hediff_Mutation>().First()).ToList();
        }
    }

    public class CompProperties_TiberiumCheck : CompProperties
    {
        public CompProperties_TiberiumCheck()
        {
            this.compClass = typeof(Comp_TRHealthCheck);
        }
    }
}
