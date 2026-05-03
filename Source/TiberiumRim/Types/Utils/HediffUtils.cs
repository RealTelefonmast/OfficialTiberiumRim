using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public static class HediffUtils
{
    // --- Pawn helpers ---

    public static Comp_TRHealthCheck HealthComp(this Pawn pawn)
    {
        return pawn?.GetComp<Comp_TRHealthCheck>();
    }

    public static bool IsTiberiumMutant(this Pawn pawn)
    {
        var diffs = pawn.health.hediffSet;
        return diffs.HasHediff(TRHediffDefOf.TiberiumImmunity) || diffs.HasHediff(TRHediffDefOf.TiberiumMutation) ||
               (pawn.story?.traits.HasTrait(TRHediffDefOf.TiberiumTrait) ?? false);
    }

    // --- Tiberium hediff detection ---

    public static bool IsTiberiumHediff(this Hediff hediff)
    {
        return hediff is Hediff_Crystallizing || hediff is Hediff_Mutation || hediff is Hediff_TiberiumPart;
    }

    // --- HediffSet queries ---

    public static bool PartIsCrystallizing(this HediffSet set, BodyPartRecord part)
    {
        for (var i = 0; i < set.hediffs.Count; i++)
            if (set.hediffs[i].Part == part && set.hediffs[i] is Hediff_Crystallizing)
                return true;
        return false;
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
            if (!Enumerable.Any(set.hediffs,
                    h => h.Part == part && (h is Hediff_TiberiumPart || h is Hediff_MissingPart)))
                yield return part;
        }
    }

    public static IEnumerable<BodyPartRecord> GetNonCrystallizingParts(this HediffSet set)
    {
        var allPartsList = set.pawn.def.race.body.AllParts;
        for (var i = 0; i < allPartsList.Count; i++)
        {
            var part = allPartsList[i];
            if (!Enumerable.Any(set.hediffs,
                    h => h.Part == part && (h is Hediff_Crystallizing || h is Hediff_MissingPart)))
                yield return part;
        }
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
