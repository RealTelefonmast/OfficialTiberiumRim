﻿using System;
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
        private bool wandered = false;
        private bool removedSample = false;
        private int ticksLeftCrystallized = 3000;
        private bool triedToMutate = false;
        private float pointOfNewReturnMax = 0;
        private float pointOfNewReturnMin = 0;

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
            Scribe_Values.Look(ref pointOfNewReturnMax, "pointOfNewReturnMax");
            Scribe_Values.Look(ref pointOfNewReturnMin, "pointOfNewReturnMin");
            base.ExposeData();
        }

        public override void PostMake()
        {
            base.PostMake();
            pointOfNewReturnMax = Mathf.Lerp(0.7f, 0.9f, 1f - pawn.Health());
            pointOfNewReturnMin = Mathf.Pow(pointOfNewReturnMax, 2);
        }

        public override float SummaryHealthPercentImpact => (float)Part.def.hitPoints / (75f * this.pawn.HealthScale);

        public override float Severity
        {
            get => severityInt;
            set => severityInt = Mathf.Clamp01(value);
        }        

        public float SeverityRate
        {
            get
            {
                float value = 1f * Severity;
                value /= Mathf.Clamp(Part.parts.Count(p => pawn.health.hediffSet.hediffs.Any(h => h.def == this.def)), 1, float.MaxValue);

                return value;
            }
        }

        public float InitSeverity
        {
            get
            {
                return pawn.health.hediffSet.GetHediffs<Hediff_Crystallizing>().Where(h => Part.parent.parts.Contains(h.Part)).Sum(h => Severity) / Part.parent.parts.Count;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (!wandered && pawn.IsHashIntervalTick(750))
            {
                if (Halted)              
                    return;

                if (CurrentStage == CrystallizingStage.Mutation)
                {
                    TryMutate();
                    triedToMutate = true;
                }

                if (CurrentStage == CrystallizingStage.Crystallized && !Part.IsCorePart && !HasCrystallizingParent(out BodyPartRecord part))
                {
                    if (ticksLeftCrystallized > 0)
                    {
                        ticksLeftCrystallized -= 750;
                        return;
                    }

                    HediffUtils.TryInfect(pawn, Part.parent, InitSeverity);
                    wandered = true;
                    Log.Message("Wandered from " + Part.LabelCap + " to " + Part.parent.LabelCap);
                    return;
                }
                /* TODO: Add Pawn ground-fusion effect and rescue jobd
                if(pawn.GetPosture() == PawnPosture.Standing && Part.height == BodyPartHeight.Bottom && TRUtils.Chance(GroundFusionChance()))
                {

                }
                */
                if (TRUtils.Chance(BloodInfectionChance()))
                    AffectOrganViaBlood();
                Severity += SeverityRate;
                pawn.health.Notify_HediffChanged(this);
            }

            if (CurrentStage == CrystallizingStage.Reversing)
                Severity -= SeverityRate * 2;
        }

        private void TryMutate()
        {
            var chance = MutationChance;
            Log.Message("Trying to mutate " + chance);
            if (TRUtils.Chance(chance))
            {
                Log.Message("Mutation Start");
                pawn.health.AddHediff(TRHediffDefOf.TiberiumMutation);
            }
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
                else if (Part.IsCorePart && Severity < pointOfNewReturnMax && Severity > pointOfNewReturnMin  && !triedToMutate)
                    stage = CrystallizingStage.Mutation;
                return stage;
            }
        }

        public override bool ShouldRemove => Severity <= 0f;
        public bool Reversing => pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberiumImmunity) || pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberBlockHediff);
        public bool Halted => pawn.health.hediffSet.hediffs.Any(h => h is Hediff_Mutation);
        public bool HasAvailableSample => !removedSample;

        public bool HasCrystallizingParent(out BodyPartRecord parent)
        {
            BodyPartRecord record = null;
            parent = record;
            var hediffs = pawn.health.hediffSet.GetHediffs<Hediff_Crystallizing>();
            for (record = Part.parent; !record.IsCorePart; record = record.parent)
            {
                parent = record;
                if (hediffs.Any(h => h.Part == record))
                    return true;
            }
            return false;
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