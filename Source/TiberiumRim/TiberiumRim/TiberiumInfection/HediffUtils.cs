using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public static class HediffUtils
    {
        public static float HediffCoverageFor(Pawn pawn, BodyPartRecord part, HediffDef coverageOf)
        {
            float num = 0;
            float parts = part.parts.Count + 1;
            var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.def == coverageOf).ToArray();
            var records = part.ChildParts(true);
            if (records.Count > 1)
            {
                foreach (BodyPartRecord potPart in records)
                {
                    if (hediffs.Any(h => h.Part == potPart))
                        num++;
                }
            }
            return num / parts;
        }

        public static float Health(this Pawn pawn)
        {
            float general = pawn.health.summaryHealth.SummaryHealthPercent;
            PawnCapacitiesHandler handler = pawn.health.capacities;
            int capacityAmt = 0;
            float totalCapacities = 0f;
            foreach (PawnCapacityDef def in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
            {
                if (handler.CapableOf(def))
                {
                    capacityAmt++;
                    totalCapacities += def.IsCritical() ? Mathf.Pow(handler.GetLevel(def), 2) : handler.GetLevel(def);
                }
            }
            return ((general * 2) + (totalCapacities / capacityAmt)) / 3;
        }

        public static bool IsCritical(this PawnCapacityDef def)
        {
            return def.lethalFlesh || def.lethalMechanoids;
        }

        public static bool IsTiberiumPart(this Hediff hediff)
        {
            return hediff is Hediff_Crystallizing || hediff is Hediff_Mutation || hediff is Hediff_TiberiumPart tp;
        }

        public static IEnumerable<BodyPartRecord> GetWanderParts(this HediffSet set, Hediff_Mutation mutation)
        {
            return from x in set.pawn.def.race.body.AllParts where !set.hediffs.Any(h => h.Part == x && (h.IsTiberiumPart() || h is Hediff_MissingPart)) select x;

            //Obsolete 
            /*
            List < BodyPartRecord > allPartsList = set.pawn.def.race.body.AllParts;
            for (int i = 0; i < allPartsList.Count; i++)
            {
                BodyPartRecord part = allPartsList[i];
                if (!set.hediffs.Any(h => h.Part == part && (h.IsTiberiumPart() || (h is Hediff_MissingPart))))
                    yield return part;
            }
            yield break;
            */
        }

        public static IEnumerable<BodyPartRecord> GetMutatableParts(this HediffSet set)
        {
            List<BodyPartRecord> allPartsList = set.pawn.def.race.body.AllParts;
            for (int i = 0; i < allPartsList.Count; i++)
            {
                BodyPartRecord part = allPartsList[i];
                if (!set.hediffs.Any(h => h.Part == part && (h is Hediff_TiberiumPart tp || h is Hediff_MissingPart)))
                    yield return part;
            }
            yield break;
        }

        public static IEnumerable<BodyPartRecord> GetNonCrystallizingParts(this HediffSet set)
        {
            List<BodyPartRecord> allPartsList = set.pawn.def.race.body.AllParts;
            for (int i = 0; i < allPartsList.Count; i++)
            {
                BodyPartRecord part = allPartsList[i];
                if(!set.hediffs.Any(h => h.Part == part && ((h is Hediff_Crystallizing) || (h is Hediff_MissingPart))))
                    yield return part;              
            }
            yield break;
        }

        public static bool IsTiberiumMutant(this Pawn pawn)
        {
            var diffs = pawn.health.hediffSet;
            return diffs.HasHediff(TRHediffDefOf.TiberiumImmunity) || diffs.HasHediff(TRHediffDefOf.TiberiumMutation) || (pawn.story?.traits.HasTrait(TRHediffDefOf.TiberiumTrait) ?? false);
        }

        public static bool CanBeHit(this BodyPartRecord part)
        {
            return part.coverageAbs > 0f;
        }

        public static bool PartIsCrystallizing(this HediffSet set, BodyPartRecord part)
        {
            for (int i = 0; i < set.hediffs.Count; i++)
            {
                if (set.hediffs[i].Part == part && set.hediffs[i] is Hediff_Crystallizing)
                    return true;
            }
            return false;
        }

        public static bool IsLimb(this BodyPartRecord record)
        {
            return record.def.tags.Any(t =>
            t == BodyPartTagDefOf.MovingLimbCore ||
            t == BodyPartTagDefOf.ManipulationLimbCore
            );
        }

        public static bool IsOrgan(this BodyPartRecord record)
        {
            return record.def.tags.Any(t =>
            t == BodyPartTagDefOf.ConsciousnessSource   ||
            t == BodyPartTagDefOf.BloodFiltrationLiver  ||
            t == BodyPartTagDefOf.BloodFiltrationSource ||
            t == BodyPartTagDefOf.BloodPumpingSource    ||
            t == BodyPartTagDefOf.MetabolismSource      ||
            t == BodyPartTagDefOf.BreathingSource       ||
            t == BodyPartTagDefOf.SightSource
            );
        }

        public static List<BodyPartRecord> AllVitalOrgans(this Pawn pawn)
        {
            return AllParts(pawn, new List<BodyPartTagDef>()
            {
                BodyPartTagDefOf.ConsciousnessSource,
                BodyPartTagDefOf.BloodFiltrationLiver,
                BodyPartTagDefOf.BloodFiltrationSource,
                BodyPartTagDefOf.BloodPumpingSource,
                BodyPartTagDefOf.MetabolismSource,
                BodyPartTagDefOf.BreathingSource
            });
        }

        public static List<BodyPartRecord> AllBreathingOrgans(this Pawn pawn)
        {
            return AllParts(pawn, new List<BodyPartTagDef>()
            {
                BodyPartTagDefOf.BreathingPathway,
                BodyPartTagDefOf.BreathingSource,
            });
        }

        public static List<BodyPartRecord> ChildParts(this BodyPartRecord record, bool withParent)
        {
            Log.Message("Parent part: " + record);
            var parts = new List<BodyPartRecord>();
            if (record == null) return null;
            if(withParent) parts.Add(record);
            foreach (var part in record.parts)
                parts.AddRange(part.ChildParts(true));
            return parts;
        }

        public static List<BodyPartRecord> AllParts(this Pawn pawn, List<BodyPartTagDef> tags)
        {
            var organs = new List<BodyPartRecord>();
            foreach(var tag in tags)
            {
                organs.AddRange(AllPartsOfTag(pawn, tag));
            }
            return organs;           
        }

        public static IEnumerable<BodyPartRecord> AllPartsOfTag(this Pawn pawn, BodyPartTagDef tag)
        {
            return pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, tag);
        }

        public static BodyPartRecord GetNotMissingPart(this Pawn pawn, BodyPartDef def)
        {
            return pawn.health.hediffSet.GetNotMissingParts(0, 0, null, null).FirstOrDefault(x => x.def == def);
        }

        public static int DistanceToCore(this BodyPartRecord part)
        {
            var distance = 0;
            var curPart = part;
            while (!curPart.IsCorePart)
            {
                curPart = curPart.parent;
                distance++;
            }
            return distance;
        }

        public static void MutatePart(Pawn pawn, BodyPartRecord part, HediffMutationGroup mutation)
        {
            if (part == null)
                return;
            var tags = part.def.tags;
            HediffDef hediff = null;
            if (tags.Contains(BodyPartTagDefOf.ManipulationLimbCore))
                hediff = mutation.arms;
            if (tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment) && part.depth == BodyPartDepth.Outside && part.GetDirectChildParts().Any(p => p.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbDigit)))
                hediff = mutation.hands;
            if (tags.Contains(BodyPartTagDefOf.MovingLimbCore))
                hediff = mutation.legs;
            if (part.IsOrgan())
                hediff = mutation.organs;

            if (hediff == null) return;
            Hediff_TiberiumPart h = (Hediff_TiberiumPart) HediffMaker.MakeHediff(hediff, pawn, part);
            h.addedManually = false;
            pawn.health.AddHediff(h);
        }

        public static int Recurse(int n, int i)
        {
            if (n > 0)
                return 0;
            int b = Recurse(n, i);
            return n - i;
        }

        public static bool CanBeAffected(this Thing thing, out float damageFactor)
        {
            damageFactor = 1f;
            var isPawn = thing is Pawn;
            damageFactor *= 1 - (isPawn
                                ? thing.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance)
                                : thing.GetStatValue(TiberiumDefOf.TiberiumDamageResistance));
            return damageFactor > 0f;
        }

        public static void TryAffectPawn(Pawn pawn, bool isGas, int perTicks)
        {
            Log.Message("Trying to affect " + pawn);
            float numCryst = 0.001f;
            float numRad = 0.00013f * perTicks;
            Comp_TRHealthCheck tibCheck = pawn.GetComp<Comp_TRHealthCheck>();
            List<BodyPartRecord> PossibleBodyParts = new List<BodyPartRecord>();
            BodyPartRecord selectedPart = null;
            if (isGas)
            {
                PossibleBodyParts = tibCheck.partsForGas;
                numCryst *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance);
            }
            else
            {
                PossibleBodyParts = tibCheck.partsForInfection;
                numCryst *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance);
            }
            numRad *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumRadiationResistance);
            Log.Message("Infection Value: " + numCryst + " Radiation Value: " + numRad);
            if (PossibleBodyParts.NullOrEmpty()) return;
            selectedPart = PossibleBodyParts.RandomElement();
            //GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.TiberiumExposure);

            TryIrradiate(pawn, numRad);
            if (TryFormVisceralPod(pawn, numCryst)) return;
            if (numCryst > 0 && TRUtils.Chance(InfectionChance(pawn, isGas)) && TouchedCrystal(pawn, selectedPart))
                TryInfect(pawn, selectedPart, numCryst);
        }

        public static bool TryInfect(Pawn pawn, BodyPartRecord part, float severity)
        {
            if (!(severity > 0)) return false;
            if (!pawn.health.hediffSet.PartIsCrystallizing(part))
            {
                Hediff hediff2 = HediffMaker.MakeHediff(TRHediffDefOf.TiberiumCrystallization, pawn);
                hediff2.Severity = severity;
                pawn.health.AddHediff(hediff2, part);
                return true;
            }
            pawn.apparel?.WornApparel?.RandomElement().TakeDamage(new DamageInfo(TRDamageDefOf.TiberiumBurn, 3));
            return false;
        }

        //Pawns can be turned into visceroids on short-term high tiberium exposure
        private static bool TryFormVisceralPod(Pawn pawn, float num)
        {
            num *= 1000f;
            float chance = 0f;
            if(num <= 0 || pawn.DestroyedOrNull() || pawn.Downed)
                return false;

            chance += 0.125f * pawn.CellsAdjacent8WayAndInside().Count(c => c.InBounds(pawn.Map) && c.GetTiberium(pawn.Map) != null);
            chance = Mathf.Clamp01(chance);
            chance *= pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure).Severity;
            chance *= 0.05f;
            chance *= num;
            Log.Message("Visceral Pod Chance: " + chance);
            if (!TRUtils.Chance(chance)) return false;
            IntVec3 loc = pawn.Position;
            Map map = pawn.Map;
            VisceralPod pod = (VisceralPod)ThingMaker.MakeThing(TiberiumDefOf.VisceralPod);
            pod.VisceralSetup(pawn);
            GenPlace.TryPlaceThing(pod, loc, map, ThingPlaceMode.Near);
            return true;
        }

        private static float InfectionChance(Pawn pawn, bool isGas)
        {
            //TODO: Adjust for tiberium resistance
            float num = TRUtils.Range(0.25f, 1f);
            if (isGas)
            {
                num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Breathing);
                num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) / 2f;
            }
            else
            {
                num *= 1f - pawn.GetStatValue(StatDefOf.MeleeDodgeChance);
                if (pawn.apparel != null)
                {
                    float count = pawn.apparel.WornApparelCount;
                    foreach (Apparel apparel in pawn.apparel.WornApparel)
                    {
                        num *= 1f - Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ToxicSensitivity) / count);
                        num *= 1f - Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ArmorRating_Sharp) / count);
                    }
                }
            }
            return num;
        }

        public static bool TryIrradiate(Pawn pawn, float rads)
        {
            if (!(rads > 0)) return false;

            Hediff radiation = pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure);
            if (radiation != null)
            { radiation.Severity += rads; }
            else
            {
                Hediff hediff = HediffMaker.MakeHediff(TRHediffDefOf.TiberiumExposure, pawn);
                hediff.Severity = rads;
                pawn.health.AddHediff(hediff);
            }
            return false;
        }

        public static bool TouchedCrystal(Pawn pawn, BodyPartRecord part)
        {
            float chance = TRUtils.Value;
            chance += pawn.GetStatValue(StatDefOf.MeleeDodgeChance) * 0.5f;
            Log.Message("Dodge Chance: " + chance);
            if (part.CanBeHit() && TRUtils.Chance(chance))
            {
                var dinfo = new DamageInfo(TRDamageDefOf.TiberiumBurn, chance * 6f, 0, -1, null, part);
                pawn.TakeDamage(dinfo);
                return false;
            }
            return true;
        }
    }
}
