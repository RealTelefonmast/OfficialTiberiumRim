using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace TiberiumRim
{
    public class Comp_TRHealthCheck : ThingComp
    {
        public List<BodyPartRecord> PartsForInfection = new List<BodyPartRecord>();
        public List<BodyPartRecord> PartsForGas = new List<BodyPartRecord>();
        public List<BodyPartRecord> PartsForMutation = new List<BodyPartRecord>();
        public List<BodyPartRecord> NonMissingParts = new List<BodyPartRecord>();

        public List<BodyPartRecord> OutsideParts = new List<BodyPartRecord>();
        public List<BodyPartRecord> OrgansInside = new List<BodyPartRecord>();

        public int NonMisingPartsCount = 0;

        private int ticker = 0;

        private Pawn Pawn => parent as Pawn;

        private RadiationInfectionGrid Grid => Pawn.MapHeld.Tiberium().TiberiumAffecter.HediffGrid;
        public bool IsInTiberium => Grid.IsAffected(Pawn.Position);

        public bool IsTiberiumImmune => Pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance) >= 1 &&
                                        Pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance)       >= 1 &&
                                        Pawn.GetStatValue(TiberiumDefOf.TiberiumRadiationResistance) >= 1;

        public bool HasGeiger => Pawn.IsColonist;

        public override void PostPostMake()
        {
            base.PostPostMake();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            UpdateParts();
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!Pawn.Spawned || IsTiberiumImmune) return;
            if (ticker <= 0)
            {
                if (IsInTiberium)
                {
                    //Player's pawns should cause a notification
                    if (Pawn.Faction?.IsPlayer ?? false)
                        GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.TiberiumExposure);

                    if (HediffUtils.TryIrradiatePawn(Pawn, Grid.RadiationAt(Pawn.Position), 250, out float rads))
                    {
                        if(HasGeiger && !Pawn.Dead)
                            DoRadiationClick(rads);
                    }
                    HediffUtils.TryInfectPawn(Pawn, Grid.InfectionAt(Pawn.Position), false, 250);
                }
                ticker = 250;
            }
            ticker--;
        }

        private void DoRadiationClick(float rads)
        {
            MoteMaker.ThrowText(this.parent.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), this.parent.Map, ((float)Math.Round(rads, 3)).ToString("0.###"), Color.white, -1f);
            TiberiumDefOf.RadiationClick.PlayOneShot(SoundInfo.InMap(parent));
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!Pawn.Spawned || IsTiberiumImmune) return;
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
        }

        public void UpdateParts()
        { 
            NonMissingParts = Pawn.health.hediffSet.GetNotMissingParts().ToList();
            NonMisingPartsCount = NonMissingParts.Count();
            OutsideParts        = NonMissingParts.Where(p => p.depth == BodyPartDepth.Outside).ToList();
            OrgansInside        = NonMissingParts.Where(p => p.depth == BodyPartDepth.Inside && p.IsOrgan()).ToList();
            PartsForGas         = NonMissingParts.Where(p => p.def.tags.Any(t => t == BodyPartTagDefOf.BreathingPathway || t == BodyPartTagDefOf.BreathingSource)).ToList();
            PartsForInfection   = NonMissingParts.Where(p => p.height == BodyPartHeight.Bottom && p.depth == BodyPartDepth.Outside).ToList(); ;
            PartsForMutation    = NonMissingParts.ToList();
            //PartsForMutation = Pawn.health.hediffSet.GetWanderParts(Pawn.health.hediffSet.GetHediffs<Hediff_Mutation>().First()).ToList();
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
