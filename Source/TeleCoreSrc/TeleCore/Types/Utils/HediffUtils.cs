using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public static class HediffUtils
{
    // --- Pawn helpers ---

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

    // --- Capacity helpers ---

    public static bool IsCritical(this PawnCapacityDef def)
    {
        return def.lethalFlesh || def.lethalMechanoids;
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
}
