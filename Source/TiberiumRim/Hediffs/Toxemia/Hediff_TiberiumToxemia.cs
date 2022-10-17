using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Hediff_TiberiumToxemia : HediffWithComps
    {
        private static Color mainColor = new ColorInt(0, 255, 125).ToColor;
        private static Color mutationColor = new ColorInt(200, 100, 255).ToColor;

        public override string LabelInBrackets
        {
            get
            {
                if (pawn.Dead) return null;
                string label = "";
                if (IsImmune)
                {
                    label += "TR_HediffToxemiaRecovering".Translate();
                }
                else
                {
                    var time = TicksUntilDeath(pawn);
                    if (time != null)
                    {
                        label += "TR_HediffToxemiaInfBracket".Translate(time?.ToStringTicksToPeriod(false, false, true, false));
                    }
                }

                return label;
            }
        }

        //"(" + Math.Round(CrystallizedPercent, 2) + ")|(" + Math.Round(Radiation?.Severity ?? 0, 2) + ")|[" + Math.Round(ToxemiaSeverity, 2) + "]|[" + BloodInfectionChance + "]|[" + MutationChance + "]";

        private string InfoText => TicksUntilDeath(pawn)?.ToStringTicksToPeriod( true, true, true, false) ?? "";

        //private string DeathInfo => "Time Left: " + GenDate.ToStringTicksToPeriod(TicksUntilDeath(pawn));
        private string MutationInfo => "TOX:" + Math.Round(ToxemiaSeverity, 2) + "|MC:" + Math.Round(MutationChance, 4) +"/"+ Math.Round(MTBValue, 2) + "d |BIC:" + Math.Round(BloodInfectionChance, 4);
        //private string HediffInfo => "C:" + CrystallizingParts.Count + "|R:" + (Radiation != null) + "|M:" + (Mutation != null) + "|SR:" + ShouldRemove;
        //private string CountInfo => "T:" + TotalAffectedParts + "|B:" + pawn.props.race.body.AllParts.Count + "|NM:" + pawn.HealthComp().NonMisingPartsCount + "|[" + CrystallizingParts.Count + "/" + affectedPartsCount + "]";
        
        public override Color LabelColor => Color.Lerp(mainColor, mutationColor, ToxemiaSeverity);
        public override bool ShouldRemove => base.ShouldRemove;//pawn.health.hediffSet.hediffs.Any(h => h is Hediff_CauseToxemia);//!(HasRadiation || HasCrystallization);

        private float CrystallizedPercent => TotalAffectedParts/(float)pawn.kindDef.RaceProps.body.AllParts.Count;//(float)affectedPartsCount / pawn.kindDef.RaceProps.body.AllParts.Count;
        public float ToxemiaSeverity => (CrystallizedPercent + (Radiation?.Severity ?? 0f)) / 2;

        //TODO: Optimize count and tracking
        private List<Hediff_CrystallizingPart> crystallizing = new();
        private List<Hediff_CrystallizedPart> crystallized = new();
        
        private IEnumerable<Hediff_CrystallizingPart> Crystallizing
        {
            get
            {
               pawn.health.hediffSet.GetHediffs(ref crystallizing);
               return crystallizing;
            }
        }

        private IEnumerable<Hediff_CrystallizedPart> Crystallized
        {
            get
            {
                pawn.health.hediffSet.GetHediffs(ref crystallized);
                return crystallized;
            }
        }

        private int TotalAffectedParts => Crystallizing.SelectMany(c => c.Part.AllChildParts(true)).Union(Crystallized.SelectMany(c => c.Part.AllChildParts(true))).Count();

        private Hediff Radiation => pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure);

        private bool IsImmune => pawn.HealthComp().IsTiberiumImmune;
        private bool HasRadiation => Radiation != null;
        private bool HasMutation => pawn.health.hediffSet.HasHediff(TRHediffDefOf.TiberiumMutation);
        private bool HasCrystallization => TotalAffectedParts > 0;


        private SimpleCurve TechLevelFactorCurve
        {
            get
            {
                var curve = new SimpleCurve();
                curve.Add(0, 1);//Undefined
                curve.Add(1, 1);//Animal
                curve.Add(2, 2);//Neolithic
                curve.Add(3, 2);//Medieval
                curve.Add(4, 1);//Indust
                curve.Add(5, 1);//Spacer
                curve.Add(6, 2);//Ultra
                curve.Add(7, 3);//Archotech
                return curve;
            }
        }

        private SimpleCurve IntelligenceFactorCurve
        {
            get
            {
                var curve = new SimpleCurve();
                curve.Add(0, 4);
                curve.Add(1, 1);
                curve.Add(2, 1);
                return curve;
            }
        }

        private SimpleCurve RadiationEffectCurve
        {
            get
            {
                var curve = new SimpleCurve();
                curve.Add(0   , 0);
                curve.Add(0.5f, 0.10f);
                curve.Add(0.6f, 0.15f);
                curve.Add(0.7f, 0.20f);
                curve.Add(0.8f, 0.75f);
                curve.Add(0.9f, 0.80f);
                curve.Add(1f  , 1);
                return curve;
            }
        }
        
        /*
        public void RegisterOrUpdatePart(Hediff_CrystallizingPart part, BodyPartRecord newPart = null)
        {
            if (CrystallizingParts.Contains(part) && newPart != null)
            {
                CrystallizingParts.Remove(part);
                affectedPartsCount += newPart.AllChildParts(true).Count - part.Part.AllChildParts(true).Count;
                return;
            }
            CrystallizingParts.Add(part);
            affectedPartsCount += part.Part.AllChildParts(true).Count;
        }

        public void RemovePart(Hediff_CrystallizingPart part)
        {
            CrystallizingParts.Remove(part);
            affectedPartsCount -= part.Part.AllChildParts(true).Count;
        }
        */

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);

        }

        public override void PostMake()
        {
            base.PostMake();

        }

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_References.Look(ref Radiation, "radiation");
            //Scribe_Collections.Look(ref CrystallizingParts, "parts");
            //Scribe_Values.Look(ref affectedPartsCount, "partsCount");
        }

        public override void Tick()
        {
            base.Tick();
            if (!pawn.IsHashIntervalTick(750)) return;
            TryMutate();
            TryBloodInfection();
        }

        public static int? TicksUntilDeath(Pawn pawn)
        {
            var hediffs = pawn.health.hediffSet.hediffs.Where(h => h is Hediff_CrystallizingPart).ToList();
            if (!hediffs.Any()) return null;
            var hediff = hediffs.MinBy(h => h.Part.DistanceToCore());

            //Estimate
            int totalTicks = 0;
            BodyPartRecord currentPart = hediff.Part;
            var curSeverity = hediff.Severity;

            while (currentPart != null)
            {
                while (curSeverity < 1)
                {
                    float val = Hediff_CrystallizingPart.SeverityPerDayFunc(curSeverity) * 0.00333333341f; //Per Tick
                    curSeverity += val;
                    totalTicks += 200;
                }

                if (currentPart.IsCorePart) break;
                curSeverity = InitSeverityFor(curSeverity, currentPart, currentPart.parent);
                currentPart = currentPart.parent;
            }

            return totalTicks;
        }

        public static float InitSeverityFor(float Severity, BodyPartRecord curPart, BodyPartRecord nextPart)
        {
            var thisChildren = curPart.AllChildParts(true).Count;
            var parentChildren = nextPart.AllChildParts(false).Count;
            return Severity * (thisChildren / (float)parentChildren);
        }

        private float MTBValue => (TicksUntilDeath(pawn)?.TicksToDays() ?? 1f) * Mathf.Lerp(0.65f, 0.2f, Radiation?.Severity ?? 0.5f);
        private void TryMutate()
        {
            if (HasMutation) return;
            if (!Rand.MTBEventOccurs(MTBValue, 60000, 750)) return;
            if (!TRandom.Chance(MutationChance)) return;
            pawn.health.AddHediff(TRHediffDefOf.TiberiumMutation);
        }

        private void TryBloodInfection()
        {
            if (!TRandom.Chance(BloodInfectionChance)) return;
            BodyPartRecord organ = pawn.AllVitalOrgans().Where(p => !pawn.health.hediffSet.PartIsCrystallizing(p)).RandomElement();
            if (organ == null) return;
            HediffUtils.InfectPart(pawn, organ, 0.01f);
        }

        //Chance Calculation
        private float MutationChanceFactor
        {
            get
            {
                float num = 1;
                if (pawn.Faction != null)
                {
                    if (pawn.Faction.IsPlayer)
                        num *= 2;
                    num *= TechLevelFactorCurve.Evaluate((int) pawn.Faction.def.techLevel);
                }
                num *= IntelligenceFactorCurve.Evaluate((int)pawn.def.race.intelligence);
                return num;
            }
        }

        private float MutationChance
        {
            get
            {
                //if (CrystallizedPercent <= 0) return 0;
                float num = 0;//ToxemiaSeverity; //Mathf.Lerp(0,,ToxemiaSeverity) / 750f;
                num += Radiation?.Severity ?? 0;
                num += CrystallizedPercent;
                num /= 2;
                num *= MutationChanceFactor;
                return Mathf.Lerp(0, ToxemiaSeverity, num);
            }
        }

        private float BloodInfectionChance
        {
            get
            {
                float val = ToxemiaSeverity * CrystallizedPercent;
                var caps = pawn.health.capacities;
                val *= caps.GetLevel(PawnCapacityDefOf.BloodPumping);
                val *= 2f - caps.GetLevel(PawnCapacityDefOf.BloodFiltration);
                return val / 750;
            }
        }

        public override bool CauseDeathNow()
        {
            return base.CauseDeathNow();
        }

    }
}
