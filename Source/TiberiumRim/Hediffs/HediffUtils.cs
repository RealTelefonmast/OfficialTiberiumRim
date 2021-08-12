using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public static class HediffUtils
    {
        public static Comp_TRHealthCheck HealthComp(this Pawn pawn)
        {
            return pawn.GetComp<Comp_TRHealthCheck>();
        }

        public static float HediffCoverageFor(Pawn pawn, BodyPartRecord part, HediffDef coverageOf)
        {
            float num = 0;
            float parts = part.parts.Count + 1;
            var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.def == coverageOf).ToArray();
            var records = part.AllChildParts(true);
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

        public static bool IsTiberiumHediff(this Hediff hediff)
        {
            return hediff is Hediff_TiberiumToxemia || hediff is Hediff_CrystallizingPart || hediff is Hediff_TiberiumMutationPart || hediff is Hediff_TiberiumPart tp;
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
            return Enumerable.Any(set.hediffs, t => t.Part == part && t is Hediff_CrystallizingPart);
        }

        public static bool PartHasHediff(this HediffSet set, BodyPartRecord part, HediffDef def)
        {
            return Enumerable.Any(set.hediffs, hediff => hediff.Part == part && hediff.def == def);
        }

        public static bool IsChildOf(this BodyPartRecord record, BodyPartRecord parent)
        {
            return parent.AllChildParts(false).Contains(record);
        }

        public static bool IsAncestorOf(this BodyPartRecord record, BodyPartRecord child)
        {
            BodyPartRecord curParent = child.parent;
            while (curParent != null)
            {
                if (curParent == record)
                    return true;
                curParent = curParent.parent;
            }
            return false;
        }

        public static Hediff GetHediffAt(this HediffSet set, BodyPartRecord part, HediffDef def)
        {
            foreach (var hediff in set.hediffs)
            {
                if (hediff.Part == part && hediff.def == def)
                    return hediff;
            }
            return null;
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

        public static List<BodyPartRecord> AllChildParts(this BodyPartRecord record, bool withParent)
        {
            var parts = new List<BodyPartRecord>();
            if (record == null) return null;
            if(withParent) parts.Add(record);
            foreach (var part in record.parts)
                parts.AddRange(part.AllChildParts(true));
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

        public static int CountUntilMeetSpecificParent(this BodyPartRecord record, BodyPartRecord parent)
        {
            int count = 0;

            if (record.parent == null)
                return 0;

            if (record != parent)
            {
                count++;
                count += CountUntilMeetSpecificParent(record.parent, parent);
            }

            return count;
        }

        public static int DistanceToCore(this BodyPartRecord part)
        {
            if (part == null) return -1;
            var distance = 0;
            var curPart = part;
            while (!curPart.IsCorePart)
            {
                curPart = curPart.parent;
                distance++;
            }
            return distance;
        }

        public static int Recurse(int n, int i)
        {
            if (n > 0)
                return 0;
            int b = Recurse(n, i);
            return n - i;
        }

        public static bool CanBeAffectedByTib(this Thing thing, bool isGas = false)
        {
            float val;
            if (thing is Pawn p)
            {
                val = 2;
                CanBeInfected(p, isGas, out float fac1);
                CanBeIrradiated(p, out float fac2);
                val -= fac1;
                val -= fac2;
                return val > 0;
            }
            else
            {
                val = 1;
                CanBeDamagedByTib(thing, out float fac3);
                val -= fac3;
                return val > 0;
            }
        }

        public static bool CanBeDamagedByTib(this Thing thing, out float damageFactor)
        {
            damageFactor = 1f;
            damageFactor *= 1 - thing.GetStatValue(TiberiumDefOf.TiberiumDamageResistance);
            return damageFactor > 0f;
        }

        public static bool CanBeInfected(this Pawn pawn, bool isGas, out float damageFactor)
        {
            damageFactor = 1f;
            damageFactor *= 1 - (isGas ? pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance) : pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance));
            return damageFactor > 0f;
        }

        public static bool CanBeIrradiated(this Pawn pawn, out float damageFactor)
        {
            damageFactor = 1f;
            damageFactor *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumRadiationResistance);
            return damageFactor > 0f;
        }

        public static void TryInfectPawn(Pawn pawn, float infectivity, bool isGas, int perTicks)
        {
            if(pawn.DestroyedOrNull())
                Log.Error("Trying to infect null Pawn");

            //Getting the initial infection weight
            float infectionValue = 0.0001f * perTicks * infectivity;
            //Applying Infection Check
            infectionValue *= isGas ? 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance) : 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance);

            //
            if (infectionValue <= 0) return;

            //
            Comp_TRHealthCheck tibCheck = pawn.GetComp<Comp_TRHealthCheck>();
            if (tibCheck == null)
            {
                Log.ErrorOnce("TibCheck missing on " + pawn, 4554666);
                return;
            }
            List<BodyPartRecord> PossibleBodyParts = isGas ? tibCheck.PartsForGas : tibCheck.PartsForInfection;
            BodyPartRecord selectedPart = PossibleBodyParts.RandomElement();

            if (TouchedCrystal(pawn, selectedPart))
                InfectPart(pawn, selectedPart, infectionValue);
        }

        public static void InfectPart(Pawn pawn, BodyPartRecord part, float severity)
        {
            if (severity <= 0) return;
            if (!pawn.health.hediffSet.PartIsCrystallizing(part))
            {
                Hediff hediff2 = HediffMaker.MakeHediff(TRHediffDefOf.CrystallizingPart, pawn);
                hediff2.Severity = severity;
                pawn.health.AddHediff(hediff2, part);
                return;
            }
            //Damage Apparel if part is already infected
            if(pawn.apparel?.WornApparelCount > 0)
                pawn.apparel.WornApparel.RandomElement().TakeDamage(new DamageInfo(TRDamageDefOf.TiberiumBurn, 3));
        }

        //Pawns can be turned into visceroids on short-term high tiberium exposure
        public static bool TryFormVisceralPod(Pawn pawn, float radiation) 
        { 
            float chance = radiation;
            if(pawn.DestroyedOrNull() || pawn.Downed)
                return false;

            chance += 0.125f * pawn.CellsAdjacent8WayAndInside().Count(c => c.InBounds(pawn.Map) && c.GetTiberium(pawn.Map) != null);
            chance = Mathf.Clamp01(chance);
            chance *= pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure)?.Severity ?? 0f;
            chance *= 0.01f;

            //Visceral chance check
            if (!TRUtils.Chance(chance)) return false;

            //If successful, spawn pod with pawn inside
            FormVisceralPod(pawn);
            return true;
        }

        public static void FormVisceralPod(Pawn pawn)
        {
            IntVec3 loc = pawn.Position;
            Map map = pawn.Map;
            VisceralPod pod = (VisceralPod)ThingMaker.MakeThing(TiberiumDefOf.VisceralPod);
            pod.VisceralSetup(pawn);
            GenPlace.TryPlaceThing(pod, loc, map, ThingPlaceMode.Near);

            if ((!pawn.Faction?.IsPlayer ?? true) || !pawn.Faction.AllyOrNeutralTo(Faction.OfPlayer)) return;
            Letter letter = LetterMaker.MakeLetter("TR_VisceralPodLetter".Translate(), "TR_VisceralPodLetterDesc".Translate(pawn), LetterDefOf.NegativeEvent, pod, pawn.Faction);
            Find.LetterStack.ReceiveLetter(letter);
        }

        private static float InfectionChance(Pawn pawn, bool isGas)
        {
            float num = 1f;
            if (isGas)
            {
                if (!pawn.CanBeInfected(true, out float gasFac)) return 0;

                num = gasFac;
                num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Breathing);
                num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) / 2f;
            }
            else
            {
                if (!pawn.CanBeInfected(false, out float infFact)) return 0;

                num = infFact;
                if (pawn.apparel != null)
                {
                    float tox = 0, sharp = 0;
                    foreach (Apparel apparel in pawn.apparel.WornApparel)
                    {
                        tox   += Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ToxicSensitivity));
                        sharp += Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ArmorRating_Sharp));
                    }

                    num *= tox;
                    num *= 1 - Mathf.Clamp01(sharp);
                    //Log.Message("Infection Chance Apparel: Tox: " + tox + " | Sharp: " + sharp);
                }
            }
            return num;
        }

        public static bool TryIrradiatePawn(Pawn pawn, float radiation, int perTicks, out float radiationDone)
        {
            if (pawn.DestroyedOrNull())
                Log.Error("Trying to irradiate null Pawn");

            //Setup Base Rad weight based on tick-frequency 
            float rads = 0.00013f * perTicks * radiation;
            //Apply Radiation Check
            rads *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumRadiationResistance);

            radiationDone = rads;
            //Check validity of radiation
            if (!(rads > 0)) return false;

            //If irradiated, increase weight, else, create radiation
            Hediff radiationHediff = pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure);
            if (radiationHediff != null)
            { radiationHediff.Severity += rads; }
            else
            {
                Hediff hediff = HediffMaker.MakeHediff(TRHediffDefOf.TiberiumExposure, pawn);
                hediff.Severity = rads;
                pawn.health.AddHediff(hediff);
            }

            //if (TryFormVisceralPod(pawn, rads))
                //visceralPod = true;
            return true;
        }

        public static bool TouchedCrystal(Pawn pawn, BodyPartRecord part)
        {
            if (!part.CanBeHit()) return false;

            float chance = TRUtils.RandValue;                        //Infection Chance
            chance -= pawn.GetStatValue(StatDefOf.MeleeDodgeChance); //Dodge Chance
            if (!TRUtils.Chance(Mathf.Clamp01(chance))) return true;

            pawn.TakeDamage(new DamageInfo(TRDamageDefOf.TiberiumBurn, chance * 6f, 0, -1, null, part));
            return false;
        }
    }
}
