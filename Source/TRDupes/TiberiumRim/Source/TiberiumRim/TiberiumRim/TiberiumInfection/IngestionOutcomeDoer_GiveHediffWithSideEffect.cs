using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class IngestionOutcomeDoer_GiveHediffWithSideEffect : IngestionOutcomeDoer_GiveHediff
    {
        public new HediffDef hediffDef;
        public new ChemicalDef toleranceChemical;
        public new float severity = -1f;

        public List<NeedDef> needs = new List<NeedDef>();
        public List<HediffDef> hediffs = new List<HediffDef>();
        public List<HediffDef> sideEffects = new List<HediffDef>();
        public bool needsNeeded = true;
        public bool hediffsNeeded = false;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (ShoulDoSideEffect(pawn))
            {
                Log.Message("Add side effects");
                foreach (var hediff in sideEffects)
                    pawn.health.AddHediff(hediff);
            }
            else
            {
                Hediff hdiff = HediffMaker.MakeHediff(hediffDef, pawn);
                float num;
                if (severity >= 0)
                    num = severity;
                else
                    num = hediffDef.initialSeverity;

                hdiff.Severity = num;
                pawn.health.AddHediff(hdiff);
            }
        }

        private bool ShoulDoSideEffect(Pawn pawn)
        {
            bool gotNeeds, gotHediffs;
            gotNeeds = pawn.needs.AllNeeds.Any(x => needs.Contains(x.def));
            gotNeeds = needsNeeded ? !gotNeeds : gotNeeds;

            gotHediffs = hediffs.Any(h => pawn.health.hediffSet.HasHediff(h));
            gotHediffs = hediffsNeeded ? !gotHediffs : gotHediffs;


            Log.Message("GotNeeds: " + gotNeeds + " GotHediffs: " + gotHediffs);
            return gotNeeds || gotHediffs;
        }
    }
}
