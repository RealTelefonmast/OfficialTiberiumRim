using RimWorld;
using UnityEngine;
using Verse;

namespace TR
{
    public class Hediff_CrystallizingPart : Hediff_CauseToxemia
    {
        private Hediff_TiberiumToxemia Parent;
        private bool removedSample = false;

        public enum InfectionStage
        {
            Fusing,
            Crystallizing,
            Crystallized,
            Reversing,
            Halted
        }

        public InfectionStage CurrentStage
        {
            get
            {
                var stage = InfectionStage.Crystallizing;
                if (Reversing)
                    stage = InfectionStage.Reversing;
                else if (Halted)
                    stage = InfectionStage.Halted;
                else if (Severity <= 0.1)
                    stage = InfectionStage.Fusing;
                else if (Severity >= 1f)
                    stage = InfectionStage.Crystallized;
                return stage;
            }
        }

        public override Color LabelColor
        {
            get
            {
                Color color = Color.white;
                switch (CurrentStage)
                {
                    case InfectionStage.Fusing:
                        color = new ColorInt(255, 100, 100).ToColor; break;
                    case InfectionStage.Crystallizing:
                        color = new ColorInt(139, 229, 138).ToColor; break;
                    case InfectionStage.Reversing:
                        color = new ColorInt(138, 229, 226).ToColor; break;
                    case InfectionStage.Halted:
                        color = new ColorInt(175, 255, 0).ToColor; break;
                    case InfectionStage.Crystallized:
                        color = new ColorInt(77, 128, 77).ToColor; break;
                }
                return color;
            }
        }

        public override string LabelInBrackets
        {
            get
            {
                string label = "";
                switch (CurrentStage)
                {
                    case InfectionStage.Fusing:
                        label = "TR_HediffFusing".Translate(); break;
                    case InfectionStage.Crystallizing:
                        label = "TR_HediffCrystallizing".Translate() + " " + Severity.ToStringPercent(); break;
                    case InfectionStage.Crystallized:
                        label = "TR_HediffCrystallized".Translate(); break;
                    case InfectionStage.Reversing:
                        label = "TR_HediffReverse".Translate(); break;
                    case InfectionStage.Halted:
                        label = "TR_HediffHalted".Translate(); break;
                }
                return label;
            }
        }

        public override float Severity
        {
            get => severityInt;
            /*
            set
            {
                bool flag = false;
                if (def.lethalSeverity > 0f && value >= def.lethalSeverity)
                {
                    value = def.lethalSeverity;
                    flag = true;
                }
                int oldIndex = CurStageIndex;
                severityInt = Mathf.Clamp(value, def.minSeverity, def.maxSeverity);
                if (CurStageIndex != oldIndex || flag)
                {
                    if (pawn.Dead) return;
                    pawn.health.Notify_HediffChanged(this);
                    pawn.needs.mood?.thoughts.situational.Notify_SituationalThoughtsDirty();
                }
            }
            */
        }

        private static SimpleCurve SeverityCurve
        {
            get
            {
                var curve = new SimpleCurve();
                curve.Add(0    , 1);
                curve.Add(0.25f, 0.3f);
                curve.Add(0.5f , 0.5f);
                curve.Add(0.75f, 0.8f);
                curve.Add(1    , 1);
                return curve;
            }
        }


        public static float SeverityPerDayFunc(float curSev)
        {
            return Mathf.Lerp(0.21f, 0.84f, SeverityCurve.Evaluate(curSev));
        }

        public float SeverityPerDay => Mathf.Lerp(0.21f, 0.84f, SeverityCurve.Evaluate(Severity));

        public override bool ShouldRemove => Severity <= 0f;

        private bool Halted => false;//pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberiumMutation);
        //TODO: Does Healing needs check?
        //private bool BeingHealed => 
        private bool Reversing => pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberiumImmunity) || pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberBlockHediff);
        private bool CanFuseToGround => Part.IsInGroup(BodyPartGroupDefOf.Legs) && CurrentStage == InfectionStage.Crystallizing;

        public bool SampleTaken
        {
            get => removedSample;
            private set => value = removedSample;
        }

        private float GroundFusionChance => Mathf.Lerp(0.001f, 0.01f, Severity);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref removedSample, "removedSample");
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.Dead) return;
            if (CurrentStage == InfectionStage.Halted) return;
            //Adjust Severity
            if (!base.pawn.IsHashIntervalTick(200)) return;

            float num = SeverityPerDay;
            num *=  0.00333333341f;
            Severity += (CurrentStage == InfectionStage.Reversing ?  -num/2 : num);

            if (CanFuseToGround && TRandom.Chance(GroundFusionChance))
                FuseToGround();

            if (CurrentStage != InfectionStage.Crystallized) return;
            if (Part.IsCorePart) return;

            WanderToNextPart();
            Crystallize();
        }

        public override bool CauseDeathNow()
        {
            return Part.IsCorePart && CurrentStage == InfectionStage.Crystallized;
        }

        public override bool TryMergeWith(Hediff newHediff)
        {
            if (!(newHediff is Hediff_CrystallizingPart)) return false;
            if (newHediff.Part.IsAncestorOf(this.Part))
            {
                var thisChildren = Part.AllChildParts(true).Count;
                var parentChildren = newHediff.Part.AllChildParts(false).Count;
                newHediff.Severity += Severity*(thisChildren/(float)parentChildren);
                pawn.health.RemoveHediff(this);
                return false;
            }

            if (newHediff.Part.IsChildOf(this.Part))
            {
                Severity += newHediff.Severity;
                return true;
            }
            return false;
        }

        private void FuseToGround()
        {
            Log.Message(pawn + " should fuse to ground!");
        }

        private void Crystallize()
        {
            //Finalize and destroy part
            Hediff_MissingPart crystallized = (Hediff_MissingPart)HediffMaker.MakeHediff(TRHediffDefOf.CrystallizedPart, pawn, null);
            crystallized.IsFresh = false;
            crystallized.lastInjury = null;
            crystallized.Part = Part;
            pawn.health.hediffSet.AddDirect(crystallized);
        }

        private void WanderToNextPart()
        {
            //
            var thisChildren = Part.AllChildParts(true).Count;
            var parentChildren = Part.parent.AllChildParts(false).Count;
            var initSeverity = Severity * (thisChildren / (float)parentChildren);//Parent.CrystallizingParts.Where(h => Part.parent.parts.Contains(h.Part)).Sum(h => Severity) / Part.parent.parts.Count;
            HediffUtils.InfectPart(pawn, Part.parent, initSeverity);
            //Update 
            //Parent.RegisterOrUpdatePart(this, Part.parent);
        }

        //Medical Recipe
        public void RemoveSample(bool criticalFailure)
        {
            if (SampleTaken) return;
            if (criticalFailure)
            {
                severityInt += (1 - severityInt) * severityInt;
                return;
            }
            SampleTaken = true;
        }
    }
}