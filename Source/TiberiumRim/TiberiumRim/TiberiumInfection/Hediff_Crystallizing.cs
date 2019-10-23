using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Hediff_Crystallizing : HediffWithComps
    {
        private bool wandered;
        private bool removedSample;
        private bool triedToMutate;
        private int ticksLeftCrystallized = 3000;
        private float PONRMax;
        private float PONRMin;

        public enum CrystallizingStage
        {
            Fusing,
            Crystallizing,
            Mutation,
            Crystallized,
            Reversing,
            Halted
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref removedSample, "removedSample");
            Scribe_Values.Look(ref wandered, "wandered");
            Scribe_Values.Look(ref ticksLeftCrystallized, "ticksLeftCrystallized");
            Scribe_Values.Look(ref triedToMutate, "triedToMutate");
            Scribe_Values.Look(ref PONRMax, "pointOfNewReturnMax");
            Scribe_Values.Look(ref PONRMin, "pointOfNewReturnMin");
            base.ExposeData();
        }

        public override void PostMake()
        {
            base.PostMake();
            PONRMax = Mathf.Lerp(0.7f, 0.9f, 1f - pawn.Health());
            PONRMin = Mathf.Pow(PONRMax, 2);
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            RemoveCrystallizingChildren();
        }

        public override float SummaryHealthPercentImpact => (float)Part.def.hitPoints / (75f * this.pawn.HealthScale);

        public override float Severity
        {
            get => severityInt;
            set => severityInt = Mathf.Clamp01(value);
        }

        public IEnumerable<Hediff_Crystallizing> Hediffs => pawn.health.hediffSet.GetHediffs<Hediff_Crystallizing>();

        public override void Tick()
        {
            base.Tick();
            var severityRate = Severity / Mathf.Clamp(Hediffs.Count(), 1, float.MaxValue);
            if (CurrentStage == CrystallizingStage.Reversing)
                Severity -= severityRate * 2;
            if (wandered || CurrentStage == CrystallizingStage.Halted || !pawn.IsHashIntervalTick(750)) return;

            if (CurrentStage == CrystallizingStage.Mutation)
            {
                TryMutate();
                triedToMutate = true;
            }

            if (CurrentStage == CrystallizingStage.Crystallized)
            {
                pawn.health.Notify_HediffChanged(this);
                if (Part.IsCorePart) return;
                if (ticksLeftCrystallized > 0)
                {
                    ticksLeftCrystallized -= 750;
                    return;
                }

                /* TODO: Add Pawn ground-fusion effect and rescue jobd
                    if(pawn.GetPosture() == PawnPosture.Standing && Part.height == BodyPartHeight.Bottom && TRUtils.Chance(GroundFusionChance()))
                    {

                    }
                */

                var initSeverity = Hediffs.Where(h => Part.parent.parts.Contains(h.Part)).Sum(h => Severity) / Part.parent.parts.Count;
                HediffUtils.TryInfect(pawn, Part.parent, initSeverity);
                wandered = true;
                Log.Message("Wandered from " + Part.LabelCap + " to " + Part.parent.LabelCap);
                return;
            }
           
            if (TRUtils.Chance(BloodInfectionChance()))
                AffectOrganViaBlood();
            Severity += severityRate;
        }

        private void TryMutate()
        {
            var chance = MutationChance;
            Log.Message("Trying to mutate " + chance);
            if (!TRUtils.Chance(chance)) return;
            Log.Message("Mutation Start");
            pawn.health.AddHediff(TRHediffDefOf.TiberiumMutation);
        }

        private void AffectOrganViaBlood()
        {
            BodyPartRecord organ = pawn.AllVitalOrgans().Where(p => !pawn.health.hediffSet.PartIsCrystallizing(p)).RandomElement();
            HediffUtils.TryInfect(pawn, organ, 0.01f);
        }

        public void RemoveSample()
        {
            if (TRUtils.Chance(0.25f))
                severityInt += (1 - severityInt) * severityInt;
            removedSample = true;
        }

        public CrystallizingStage CurrentStage
        {
            get
            {
                var stage = CrystallizingStage.Crystallizing;
                if (Severity >= 1f)
                    stage = CrystallizingStage.Crystallized;
                else if (Reversing)
                    stage = CrystallizingStage.Reversing;
                else if (Halted)
                    stage = CrystallizingStage.Halted;
                else if (Severity <= 0.1f)
                    stage = CrystallizingStage.Fusing;
                else if (!triedToMutate && Part.IsCorePart && Severity < PONRMax && Severity > PONRMin)
                    stage = CrystallizingStage.Mutation;
                return stage;
            }
        }

        public override bool ShouldRemove => Severity <= 0f;
        public bool Reversing => pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberiumImmunity) || pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberBlockHediff);
        public bool Halted => pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberiumMutation);
        public bool HasAvailableSample => !removedSample;

        public void RemoveCrystallizingChildren()
        {
            float severity = 0;
            int parts = 0;
            var childParts = Part.ChildParts(false);
            if (childParts.NullOrEmpty()) return;
            foreach (var hediff in Hediffs)
            {
                if (hediff == null || !childParts.Contains(hediff.Part)) continue;
                severity += hediff.Severity;
                parts++;
                pawn.health.RemoveHediff(hediff);
            }
            Severity += severity / ((float)parts / childParts.Count);
        }

        public override Color LabelColor
        {
            get
            {
                Color color = Color.white;
                switch (CurrentStage)
                {
                    case CrystallizingStage.Fusing:
                        color = new ColorInt(255, 100, 100).ToColor; break;
                    case CrystallizingStage.Crystallizing:
                        color = new ColorInt(139, 229, 138).ToColor; break;
                    case CrystallizingStage.Crystallized:
                        color = new ColorInt(77, 128, 77).ToColor; break;
                    case CrystallizingStage.Reversing:
                        color = new ColorInt(138, 229, 226).ToColor; break;
                    case CrystallizingStage.Halted:
                        color = new ColorInt(175, 255, 0).ToColor; break;
                    case CrystallizingStage.Mutation:
                        color = new ColorInt(200, 100, 255).ToColor; break;
                }
                return color;
            }
        }

        public override string LabelBase => base.LabelBase;
        public override string SeverityLabel => base.SeverityLabel;

        public override string LabelInBrackets
        {
            get
            {
                string label = "";
                switch (CurrentStage)
                {
                    case CrystallizingStage.Fusing:
                        label = "TR_HediffFusing".Translate(); break;
                    case CrystallizingStage.Crystallizing:
                        label = "TR_HediffCrystallizing".Translate() + " " + Severity.ToStringPercent(); break;
                    case CrystallizingStage.Crystallized:
                        label = "TR_HediffCrystallized".Translate(); break;
                    case CrystallizingStage.Reversing:
                        label = "TR_HediffReverse".Translate(); break;
                    case CrystallizingStage.Halted:
                        label = "TR_HediffHalted".Translate(); break;
                    case CrystallizingStage.Mutation:
                        label = "TR_HediffMutation".Translate() + " " + MutationChance.ToStringPercent(); break;
                }
                return label;
            }
        }

        public float GroundFusionChance()
        {
            float val = 0;

            return val;
        }       

        public float MutationChance
        {
            get
            {
                float num = 1f - pawn.Health();
                num *= TechMultiplier;
                num /= Mathf.Clamp(Part.DistanceToCore(), 1, int.MaxValue);
                return num * 0.5f;
            }
        }

        private float TechMultiplier
        {
            get
            {
                if (pawn.Faction == null)
                    return 2;

                switch (pawn.Faction.def.techLevel)
                {
                    case TechLevel.Animal:
                        return 2;
                    case TechLevel.Neolithic:
                        return 3;
                    case TechLevel.Medieval:
                        return 1.5f;
                    default:
                        return 1;
                }
            }
        }

        public float BloodInfectionChance()
        {
            float val = Severity * 0.01f;
            var caps = pawn.health.capacities;
            val *= caps.GetLevel(PawnCapacityDefOf.BloodPumping);
            val *= 1.5f - caps.GetLevel(PawnCapacityDefOf.BloodFiltration);
            return val;
        }

        public override bool CauseDeathNow()
        {
            return Part.IsCorePart && CurrentStage == CrystallizingStage.Crystallized;
        }

        public override bool TryMergeWith(Hediff other)
        {
            return false;
        }
    }
}