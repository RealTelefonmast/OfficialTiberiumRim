using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class IngestionOutcomeDoer_GiveHediffWithSideEffect : IngestionOutcomeDoer_GiveHediff
    {
        //public new HediffDef hediffDef;
        //public new ChemicalDef toleranceChemical;
        //public float severity = -1f;

        public List<NeedDef> requiredNeeds; //Needs required to avoid side effect
        public List<NeedDef> culpritNeeds;  //Needs that cause side effect

        public List<HediffDef> requiredHediffs = new List<HediffDef>(); //Hediffs required to avoid side effect
        public List<HediffDef> culpritHediffs = new List<HediffDef>();  //Hediffs that cause side effect

        public List<DefFloat<HediffDef>> sideEffects = new List<DefFloat<HediffDef>>();

        public override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (ShoulDoSideEffect(pawn))
            {
                foreach (var hediff in sideEffects)
                {
                    if (TRandom.Chance(hediff.value))
                    {
                        var hediffCaused = HediffMaker.MakeHediff(hediff.def, pawn);
                        hediffCaused.Severity = 0.5f;
                        pawn.health.AddHediff(hediffCaused);
                    }
                }
            }
            else
            {
                Hediff hdiff = HediffMaker.MakeHediff(hediffDef, pawn);
                float num;
                num = severity >= 0 ? severity : hediffDef.initialSeverity;

                hdiff.Severity = num;
                pawn.health.AddHediff(hdiff);
            }
        }

        private bool ShoulDoSideEffect(Pawn pawn)
        {
            var allNeeds = pawn.needs.AllNeeds;
            var allHediffs = pawn.health.hediffSet.hediffs;

            //Culrpits are prioritized
            if(!culpritNeeds.NullOrEmpty() && allNeeds.Any(n => culpritNeeds.Contains(n.def)))
            {
                return true;
            }

            if (!culpritHediffs.NullOrEmpty() && allHediffs.Any(h => culpritHediffs.Contains(h.def)))
            {
                return true;
            }

            bool hasRequiredNeeds =  requiredNeeds.NullOrEmpty() || requiredNeeds.All(x => allNeeds.Select(n => n.def).Contains(x));
            bool hasRequiredHediffs =  requiredHediffs.NullOrEmpty() || requiredHediffs.All(x => allHediffs.Select(n => n.def).Contains(x));

            return !(hasRequiredHediffs && hasRequiredNeeds);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            foreach (var stat in base.SpecialDisplayStats(parentDef))
            {
                yield return stat;
            }
            yield return new StatDrawEntry(StatCategoryDefOf.Drug, "TR_SideEffectLabel".Translate(), GetReadout() , ReportString(), 2500);
        }

        private string GetReadout()
        {
            string result;

            return sideEffects.Count + " side effects";
        }

        private string ReportString()
        {
            string result = "TR_SideEffectDesc".Translate() + "\n\n"; ;

            //List Side Effects (First)
            if (!sideEffects.NullOrEmpty())
            {
                result += "TR_SideEffectListing".Translate() + "\n"; ;
                foreach (var sideEffect in sideEffects)
                {
                    result += "     -  " + sideEffect.ToStringPercent() + "\n";
                }
                result += "\n\n";
            }

            //List Requirements
            if (!requiredNeeds.NullOrEmpty())
            {
                result += "TR_SideEffectRequiredNeeds".Translate() + "\n"; ;
                foreach (var need in requiredNeeds)
                {
                    result += "     -  " + need.LabelCap + "\n"; ;
                }
                result += "\n";
            }

            if (!requiredHediffs.NullOrEmpty())
            {
                result += "TR_SideEffectRequiredHediffs".Translate() + "\n"; ;
                foreach (var hediff in requiredHediffs)
                {
                    result += "     -  " + hediff.LabelCap + "\n"; ;
                }
                result += "\n";
            }

            //Warn if culprits exist
            if (!culpritHediffs.NullOrEmpty() || !culpritNeeds.NullOrEmpty())
            {
                result += "TR_SideEffectWarning".Translate() + "\n";
                if (!culpritNeeds.NullOrEmpty())
                {
                    result += ((string)"TR_SideEffectCulpritNeeds".Translate()).Colorize(Color.red).Bold() + "\n"; ;
                    foreach (var culpritNeed in culpritNeeds)
                    {
                        result += "     -  " + culpritNeed.LabelCap + "\n"; ;
                    }
                    result += "\n";
                }

                if (!culpritHediffs.NullOrEmpty())
                {
                    result += "TR_SideEffectCulpritHediffs".Translate().Colorize(Color.red).Bold() + "\n"; ;
                    foreach (var culpritHediff in culpritHediffs)
                    {
                        result += "     -  " + culpritHediff.LabelCap + "\n"; ;
                    }
                    result += "\n";
                }
            }

            //If No culprit exists list chance
            if (chance > 0)
            {
                result += "TR_SideEffectChance".Translate(chance.ToStringPercent());
            }

            return result;
        }
    }
}
