using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TR.DefOf;
using TR.GameParts.EVA;
using TR.Hediffs.TiberiumInfection;
using TR.TiberiumObjects;
using TR.Util;
using UnityEngine;
using Verse;
using VisceralPod = TR.GameParts.VisceralPod;

namespace TR.Hediffs;

public static class HediffUtils
{
    // --- Pawn helpers ---

    public static Comp_TRHealthCheck HealthComp(this Pawn pawn)
    {
        return pawn?.GetComp<Comp_TRHealthCheck>();
    }

    public static bool IsMechanoid(this Pawn pawn)
    {
        return pawn?.kindDef?.RaceProps?.IsMechanoid ?? false;
    }

    public static float Health(this Pawn pawn)
    {
        var general = pawn.health.summaryHealth.SummaryHealthPercent;
        var handler = pawn.health.capacities;
        var capacityAmt = 0;
        var totalCapacities = 0f;
        foreach (var def in DefDatabase<PawnCapacityDef>.AllDefsListForReading)
            if (handler.CapableOf(def))
            {
                capacityAmt++;
                totalCapacities += def.IsCritical() ? Mathf.Pow(handler.GetLevel(def), 2) : handler.GetLevel(def);
            }

        return (general * 2 + totalCapacities / capacityAmt) / 3;
    }

    public static bool IsTiberiumMutant(this Pawn pawn)
    {
        var diffs = pawn.health.hediffSet;
        return diffs.HasHediff(TRHediffDefOf.TiberiumImmunity) || diffs.HasHediff(TRHediffDefOf.TiberiumMutation) ||
               (pawn.story?.traits.HasTrait(TRHediffDefOf.TiberiumTrait) ?? false);
    }

    // --- Capacity / hediff type helpers ---

    public static bool IsCritical(this PawnCapacityDef def)
    {
        return def.lethalFlesh || def.lethalMechanoids;
    }

    public static bool IsTiberiumHediff(this Hediff hediff)
    {
        return hediff is Hediff_Crystallizing || hediff is Hediff_Mutation || hediff is Hediff_TiberiumPart;
    }

    // --- HediffSet queries ---

    public static float HediffCoverageFor(Pawn pawn, BodyPartRecord part, HediffDef coverageOf)
    {
        float num = 0;
        float parts = part.parts.Count + 1;
        var hediffs = pawn.health.hediffSet.hediffs.Where(h => h.def == coverageOf).ToArray();
        var records = part.ChildParts(true);
        if (records.Count > 1)
            foreach (var potPart in records)
                if (hediffs.Any(h => h.Part == potPart))
                    num++;

        return num / parts;
    }

    public static bool PartIsCrystallizing(this HediffSet set, BodyPartRecord part)
    {
        for (var i = 0; i < set.hediffs.Count; i++)
            if (set.hediffs[i].Part == part && set.hediffs[i] is Hediff_Crystallizing)
                return true;
        return false;
    }

    public static bool PartHasHediff(this HediffSet set, BodyPartRecord part, HediffDef def)
    {
        return set.hediffs.Any(hediff => hediff.Part == part && hediff.def == def);
    }

    public static Hediff GetHediffAt(this HediffSet set, BodyPartRecord part, HediffDef def)
    {
        foreach (var hediff in set.hediffs)
            if (hediff.Part == part && hediff.def == def)
                return hediff;
        return null;
    }

    public static IEnumerable<BodyPartRecord> GetWanderParts(this HediffSet set, Hediff_Mutation mutation)
    {
        return from x in set.pawn.def.race.body.AllParts
            where !set.hediffs.Any(h => h.Part == x && (h.IsTiberiumHediff() || h is Hediff_MissingPart))
            select x;
    }

    public static IEnumerable<BodyPartRecord> GetMutatableParts(this HediffSet set)
    {
        var allPartsList = set.pawn.def.race.body.AllParts;
        for (var i = 0; i < allPartsList.Count; i++)
        {
            var part = allPartsList[i];
            if (!set.hediffs.Any(h => h.Part == part && (h is Hediff_TiberiumPart || h is Hediff_MissingPart)))
                yield return part;
        }
    }

    public static IEnumerable<BodyPartRecord> GetNonCrystallizingParts(this HediffSet set)
    {
        var allPartsList = set.pawn.def.race.body.AllParts;
        for (var i = 0; i < allPartsList.Count; i++)
        {
            var part = allPartsList[i];
            if (!set.hediffs.Any(h => h.Part == part && (h is Hediff_Crystallizing || h is Hediff_MissingPart)))
                yield return part;
        }
    }

    // --- Body part record helpers ---

    public static bool CanBeHit(this BodyPartRecord part)
    {
        return part.coverageAbs > 0f;
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
            t == BodyPartTagDefOf.ConsciousnessSource ||
            t == BodyPartTagDefOf.BloodFiltrationLiver ||
            t == BodyPartTagDefOf.BloodFiltrationSource ||
            t == BodyPartTagDefOf.BloodPumpingSource ||
            t == BodyPartTagDefOf.MetabolismSource ||
            t == BodyPartTagDefOf.BreathingSource ||
            t == BodyPartTagDefOf.SightSource
        );
    }

    public static bool IsChildOf(this BodyPartRecord record, BodyPartRecord parent)
    {
        return parent.ChildParts(false).Contains(record);
    }

    public static bool IsAncestorOf(this BodyPartRecord record, BodyPartRecord child)
    {
        var curParent = child.parent;
        while (curParent != null)
        {
            if (curParent == record)
                return true;
            curParent = curParent.parent;
        }

        return false;
    }

    public static int CountUntilMeetSpecificParent(this BodyPartRecord record, BodyPartRecord parent)
    {
        var count = 0;
        if (record.parent == null)
            return 0;
        if (record != parent)
        {
            count++;
            count += record.parent.CountUntilMeetSpecificParent(parent);
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

    // --- Part traversal ---

    /// <summary>Canonical name.</summary>
    public static List<BodyPartRecord> ChildParts(this BodyPartRecord record, bool withParent)
    {
        var parts = new List<BodyPartRecord>();
        if (record == null) return null;
        if (withParent) parts.Add(record);
        foreach (var part in record.parts)
            parts.AddRange(part.ChildParts(true));
        return parts;
    }

    /// <summary>Backward-compat alias for ChildParts.</summary>
    public static List<BodyPartRecord> AllChildParts(this BodyPartRecord record, bool withParent)
    {
        return record.ChildParts(withParent);
    }

    public static List<BodyPartRecord> AllVitalOrgans(this Pawn pawn)
    {
        return pawn.AllParts(new List<BodyPartTagDef>
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
        return pawn.AllParts(new List<BodyPartTagDef>
        {
            BodyPartTagDefOf.BreathingPathway,
            BodyPartTagDefOf.BreathingSource
        });
    }

    public static List<BodyPartRecord> AllParts(this Pawn pawn, List<BodyPartTagDef> tags)
    {
        var organs = new List<BodyPartRecord>();
        foreach (var tag in tags) organs.AddRange(pawn.AllPartsOfTag(tag));
        return organs;
    }

    public static IEnumerable<BodyPartRecord> AllPartsOfTag(this Pawn pawn, BodyPartTagDef tag)
    {
        return pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, tag);
    }

    public static BodyPartRecord GetNotMissingPart(this Pawn pawn, BodyPartDef def)
    {
        return pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == def);
    }

    // --- Mutation ---

    public static void MutatePart(Pawn pawn, BodyPartRecord part, HediffMutationGroup mutation)
    {
        if (part == null)
            return;
        var tags = part.def.tags;
        HediffDef hediff = null;
        if (tags.Contains(BodyPartTagDefOf.ManipulationLimbCore))
            hediff = mutation.Arm;
        if (tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment) && part.depth == BodyPartDepth.Outside &&
            part.GetDirectChildParts().Any(p => p.def.tags.Contains(BodyPartTagDefOf.ManipulationLimbDigit)))
            hediff = mutation.Hand;
        if (tags.Contains(BodyPartTagDefOf.MovingLimbCore))
            hediff = mutation.Leg;
        if (part.IsOrgan())
            hediff = mutation.Organ.RandomElementByWeight(o => o.value).hediffDef;

        if (hediff == null) return;
        var h = (Hediff_TiberiumPart)HediffMaker.MakeHediff(hediff, pawn, part);
        h.addedManually = false;
        pawn.health.AddHediff(h);
    }

    // --- Tib resistance checks ---

    public static bool CanBeAffectedByTib(this Thing thing, bool isGas = false)
    {
        float val;
        if (thing is Pawn p)
        {
            val = 2;
            p.CanBeInfected(isGas, out var fac1);
            p.CanBeIrradiated(out var fac2);
            val -= fac1;
            val -= fac2;
            return val > 0;
        }

        val = 1;
        thing.CanBeDamagedByTib(out var fac3);
        val -= fac3;
        return val > 0;
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
        damageFactor *= 1 - (isGas
            ? pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance)
            : pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance));
        return damageFactor > 0f;
    }

    public static bool CanBeIrradiated(this Pawn pawn, out float damageFactor)
    {
        damageFactor = 1f;
        damageFactor *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumRadiationResistance);
        return damageFactor > 0f;
    }

    // --- Infection ---

    public static void TryInfectPawn(Pawn pawn, TiberiumCrystal crystal, bool isGas, int perTicks)
    {
        if (pawn.DestroyedOrNull())
            Log.Error("Trying to infect null Pawn");

        var numCryst = 0.0001f * perTicks;
        numCryst *= isGas
            ? 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance)
            : 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance);

        var shouldInfect = numCryst > 0 && (crystal?.def.IsInfective ?? isGas);
        if (!shouldInfect || !TRUtils.Chance(InfectionChance(pawn, isGas))) return;

        var tibCheck = pawn.GetComp<Comp_TRHealthCheck>();
        if (tibCheck == null)
        {
            Log.ErrorOnce("TibCheck missing on " + pawn, 4554666);
            return;
        }

        var possibleBodyParts = isGas ? tibCheck.PartsForGas : tibCheck.PartsForInfection;
        var selectedPart = possibleBodyParts.RandomElement();

        if (pawn.Faction?.IsPlayer ?? false)
            GameComponent_EVA.EVAComp().ReceiveSignal(EVASignal.TiberiumExposure, pawn);

        if (TouchedCrystal(pawn, selectedPart))
            InfectPart(pawn, selectedPart, numCryst);
    }

    /// <summary>Backward-compat overload for callers passing a raw infectivity float (e.g. VeinGasCloud).</summary>
    public static void TryInfectPawn(Pawn pawn, float infectivity, bool isGas, int perTicks)
    {
        if (pawn.DestroyedOrNull())
            Log.Error("Trying to infect null Pawn");

        var infectionValue = 0.0001f * perTicks * infectivity;
        infectionValue *= isGas
            ? 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance)
            : 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance);

        if (infectionValue <= 0) return;

        var tibCheck = pawn.GetComp<Comp_TRHealthCheck>();
        if (tibCheck == null)
        {
            Log.ErrorOnce("TibCheck missing on " + pawn, 4554666);
            return;
        }

        var possibleBodyParts = isGas ? tibCheck.PartsForGas : tibCheck.PartsForInfection;
        var selectedPart = possibleBodyParts.RandomElement();

        if (TouchedCrystal(pawn, selectedPart))
            InfectPart(pawn, selectedPart, infectionValue);
    }

    public static void InfectPart(Pawn pawn, BodyPartRecord part, float severity)
    {
        if (severity <= 0) return;
        if (!pawn.health.hediffSet.PartIsCrystallizing(part))
        {
            var hediff2 = HediffMaker.MakeHediff(TRHediffDefOf.TiberiumCrystallization, pawn);
            hediff2.Severity = severity;
            pawn.health.AddHediff(hediff2, part);
            return;
        }

        if (pawn.apparel?.WornApparelCount > 0)
            pawn.apparel.WornApparel.RandomElement().TakeDamage(new DamageInfo(TRDamageDefOf.TiberiumBurn, 3));
    }

    private static float InfectionChance(Pawn pawn, bool isGas)
    {
        var num = 1f;
        if (isGas)
        {
            if (!pawn.CanBeInfected(true, out var gasFac)) return 0;

            num = gasFac;
            num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Breathing);
            num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) / 2f;
        }
        else
        {
            if (!pawn.CanBeInfected(false, out var infFact)) return 0;

            num = infFact;
            if (pawn.apparel != null)
            {
                float tox = 0, sharp = 1;
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    tox += Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ToxicResistance));
                    sharp -= Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ArmorRating_Sharp));
                }

                num *= tox;
                num *= 1 - sharp;
            }
        }

        return num;
    }

    // --- Irradiation ---

    public static bool TryIrradiatePawn(Pawn pawn, TiberiumCrystal crystal, int perTicks)
    {
        if (pawn.DestroyedOrNull())
            Log.Error("Trying to irradiate null Pawn");

        var rads = 0.00013f * perTicks;
        rads *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumRadiationResistance);

        if (crystal?.def.tiberium.radiates ?? false) return false;
        if (!(rads > 0)) return false;

        var radiation = pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure);
        if (radiation != null)
        {
            radiation.Severity += rads;
        }
        else
        {
            var hediff = HediffMaker.MakeHediff(TRHediffDefOf.TiberiumExposure, pawn);
            hediff.Severity = rads;
            pawn.health.AddHediff(hediff);
        }

        TryFormVisceralPod(pawn, rads);
        return true;
    }

    /// <summary>
    ///     Backward-compat overload for callers passing a raw radiation float with an out result (e.g.
    ///     Comp_TRHealthCheck).
    /// </summary>
    public static bool TryIrradiatePawn(Pawn pawn, float radiation, int perTicks, out float radiationDone)
    {
        if (pawn.DestroyedOrNull())
            Log.Error("Trying to irradiate null Pawn");

        var rads = 0.00013f * perTicks * radiation;
        rads *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumRadiationResistance);

        radiationDone = rads;
        if (!(rads > 0)) return false;

        var radiationHediff = pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure);
        if (radiationHediff != null)
        {
            radiationHediff.Severity += rads;
        }
        else
        {
            var hediff = HediffMaker.MakeHediff(TRHediffDefOf.TiberiumExposure, pawn);
            hediff.Severity = rads;
            pawn.health.AddHediff(hediff);
        }

        return true;
    }

    // --- Visceral pod ---

    private static bool TryFormVisceralPod(Pawn pawn, float radiation)
    {
        if (pawn.DestroyedOrNull() || pawn.Downed)
            return false;

        var chance = radiation;
        chance += 0.125f * pawn.CellsAdjacent8WayAndInside()
            .Count(c => c.InBounds(pawn.Map) && c.GetTiberium(pawn.Map) != null);
        chance = Mathf.Clamp01(chance);
        chance *= pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumExposure)?.Severity ?? 0f;
        chance *= 0.05f;

        if (!TRUtils.Chance(chance)) return false;

        FormVisceralPod(pawn);
        return true;
    }

    public static void FormVisceralPod(Pawn pawn)
    {
        var loc = pawn.Position;
        var map = pawn.Map;
        var pod = (VisceralPod)ThingMaker.MakeThing(TiberiumDefOf.VisceralPod);
        pod.VisceralSetup(pawn);
        GenPlace.TryPlaceThing(pod, loc, map, ThingPlaceMode.Near);

        if ((!pawn.Faction?.IsPlayer ?? true) || !pawn.Faction.AllyOrNeutralTo(Faction.OfPlayer)) return;
        var letter = LetterMaker.MakeLetter("TR_VisceralPodLetter".Translate(),
            "TR_VisceralPodLetterDesc".Translate(pawn), LetterDefOf.NegativeEvent, pod, pawn.Faction);
        Find.LetterStack.ReceiveLetter(letter);
    }

    // --- Contact ---

    public static bool TouchedCrystal(Pawn pawn, BodyPartRecord part)
    {
        var chance = TRUtils.RandValue;
        chance += pawn.GetStatValue(StatDefOf.MeleeDodgeChance) * 0.5f;
        if (part.CanBeHit() && TRUtils.Chance(chance))
        {
            pawn.TakeDamage(new DamageInfo(TRDamageDefOf.TiberiumBurn, chance * 6f, 0, -1, null, part));
            return false;
        }

        return true;
    }

    // --- Misc ---

    public static int Recurse(int n, int i)
    {
        if (n > 0)
            return 0;
        var b = Recurse(n, i);
        return n - i;
    }
}