using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public static class HediffUtils
    {
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
            return hediff is Hediff_Crystallizing || hediff is Hediff_Mutation || hediff is Hediff_MutationPart || hediff is Hediff_MutatedPart || hediff is Hediff_TiberiumPart;
        }

        public static IEnumerable<BodyPartRecord> GetWanderParts(this HediffSet set)
        {
            List<BodyPartRecord> allPartsList = set.pawn.def.race.body.AllParts;
            for (int i = 0; i < allPartsList.Count; i++)
            {
                BodyPartRecord part = allPartsList[i];
                if (!set.hediffs.Any(h => h.Part == part && (h.IsTiberiumPart() || (h is Hediff_MissingPart))))
                    yield return part;
            }
            yield break;
        }

        public static IEnumerable<BodyPartRecord> GetMutatableParts(this HediffSet set)
        {
            List<BodyPartRecord> allPartsList = set.pawn.def.race.body.AllParts;
            for (int i = 0; i < allPartsList.Count; i++)
            {
                BodyPartRecord part = allPartsList[i];
                if (!set.hediffs.Any(h => h.Part == part && ((h is Hediff_MutatedPart) || (h is Hediff_MissingPart))))
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

        public static bool CanBeHit(this BodyPartRecord part)
        {
            return part.coverageAbs > 0f;
        }

        public static bool PartIsCrystallizing(this HediffSet set, BodyPartRecord part)
        {
            for (int i = 0; i < set.hediffs.Count; i++)
            {
                if (set.hediffs[i].Part == part && set.hediffs[i] is Hediff_Crystallizing)
                {
                    return true;
                }
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
            var tags = part.def.tags;
            HediffDef hediff = null;
            if (tags.Contains(BodyPartTagDefOf.ManipulationLimbCore))
                hediff = mutation.arms;
            if (tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment))
                hediff = mutation.hands;
            if (tags.Contains(BodyPartTagDefOf.MovingLimbCore))
                hediff = mutation.legs;
            if (part.IsOrgan())
                hediff = mutation.organs;
            pawn.health.AddHediff(hediff, part);
        }

        public static void TryAffectPawn(Pawn pawn, bool isGas)
        {
            float numCryst = 0.001f;
            float numRad = 0.002f;
            List<BodyPartRecord> PossibleBodyParts = new List<BodyPartRecord>();
            List<Apparel> apparelSet = new List<Apparel>();
            BodyPartRecord selectedPart = null;
            if (pawn.apparel != null)
            {
                apparelSet = pawn.apparel.WornApparel;
            }
            if (isGas)
            {
                PossibleBodyParts = pawn.health.hediffSet.GetNotMissingParts().Where(p => p.def.tags.Any(t => t == BodyPartTagDefOf.BreathingPathway || t == BodyPartTagDefOf.BreathingSource)).ToList();
                foreach (Apparel apparel in apparelSet)
                {
                    if (apparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
                    {
                        numCryst *= (1 - Mathf.Clamp(apparel.GetStatValue(StatDefOf.ToxicSensitivity), 0f, 1f)) * 0.25f;
                        numCryst *= 1 - apparel.GetStatValue(TiberiumDefOf.TibGasFiltering);
                    }
                }
            }
            else
            {
                PossibleBodyParts = pawn.health.hediffSet.GetNotMissingParts().Where(p => p.height == BodyPartHeight.Bottom && p.depth == BodyPartDepth.Outside).ToList();
                foreach (Apparel apparel in apparelSet)
                {
                    if (apparel.def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead))
                    {
                        numCryst *= (1 - Mathf.Clamp(apparel.GetStatValue(StatDefOf.ToxicSensitivity), 0f, 1f)) * 0.25f;
                        numCryst *= 1 - apparel.GetStatValue(TiberiumDefOf.TibGasFiltering);

                        numRad *= 1 - Mathf.Clamp(apparel.GetStatValue(StatDefOf.Insulation_Heat), 0f, 0.5f);
                        if (apparel.def.defName.Contains("_TBP"))
                        {
                            numRad -= 0.005f;
                        }
                    }
                    if (apparel.def.apparel.bodyPartGroups.Any(b => b == BodyPartGroupDefOf.Legs && b == BodyPartGroupDefOf.Torso))
                    {
                        numCryst *= 1 - Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ArmorRating_Sharp));
                        numRad *= 1 - Mathf.Clamp(apparel.GetStatValue(StatDefOf.Insulation_Heat), 0f, 0.5f);
                        if (apparel.def.defName.Contains("_TBP"))
                        {
                            numRad -= 0.005f;
                            numCryst *= 0;
                        }
                    }
                }
            }
            Log.Message("NumCryst: " + numCryst + " NumRads: " + numRad);
            selectedPart = PossibleBodyParts.RandomElement();
            TryIrradiate(pawn, numRad);
            if (!TryFormVisceralPod(pawn, numCryst))
            {
                if (TRUtils.Chance(InfectionChance(pawn, isGas)))
                {
                    TryInfect(pawn, selectedPart, numCryst);
                }
            }
        }

        public static bool TryInfect(Pawn pawn, BodyPartRecord bodyPart, float severity)
        {
            if (severity > 0)
            {
                if (!pawn.health.hediffSet.PartIsCrystallizing(bodyPart) && TouchedCrystal(pawn, bodyPart))
                {
                    Hediff hediff2 = HediffMaker.MakeHediff(TRHediffDefOf.TiberiumCrystallization, pawn);
                    hediff2.Severity = severity;
                    pawn.health.AddHediff(hediff2, bodyPart);
                    return true;
                }
                pawn.apparel?.WornApparel?.RandomElement().TakeDamage(new DamageInfo(TRDamageDefOf.TiberiumBurn, 3));
            }
            return false;
        }

        //Pawns can be turned into visceroids on short-term high tiberium exposure
        private static bool TryFormVisceralPod(Pawn pawn, float num)
        {
            num *= 1000f;
            float chance = 0f;
            if(pawn.DestroyedOrNull() || pawn.Downed)
            {
                return false;
            }
            pawn.CellsAdjacent8WayAndInside().Where(c => c.InBounds(pawn.Map)).ToList().ForEach(c =>
            {
                if (c.GetTiberium(pawn.Map) != null)
                {
                    chance += 0.125f;
                }
            });
            chance = Mathf.Clamp01(chance);
            chance *= pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure).Severity;
            chance *= 0.05f;
            chance *= num;
            Log.Message("Visceral Pod Chance: " + chance);
            if (TRUtils.Chance(chance))
            {
                IntVec3 loc = pawn.Position;
                Map map = pawn.Map;
                VisceralPod pod = (VisceralPod)ThingMaker.MakeThing(TiberiumDefOf.VisceralPod);
                pod.VisceralSetup(pawn);
                //pawn.DeSpawn();
                GenPlace.TryPlaceThing(pod, loc, map, ThingPlaceMode.Near);
                return true;
            }
            return false;
        }

        private static float InfectionChance(Pawn pawn, bool isGas)
        {
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
            if (rads > 0)
            {
                Hediff radiation = pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure);
                if (radiation != null)
                { radiation.Severity += rads; }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(TRHediffDefOf.TiberiumExposure, pawn);
                    hediff.Severity = rads;
                    pawn.health.AddHediff(hediff);
                }
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
