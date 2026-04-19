using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class StatWorker_Tiberium : StatWorker
{
    public override bool IsDisabledFor(Thing thing)
    {
        if (thing is MechanicalPawn) return false;
        return base.IsDisabledFor(thing);
    }

    public override void FinalizeValue(StatRequest req, ref float val, bool applyPostProcess)
    {
        base.FinalizeValue(req, ref val, applyPostProcess);
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return req.HasThing && req.Thing.def.category == ThingCategory.Pawn;
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        StringBuilder stringBuilder = new StringBuilder();
        var baseValueFor = GetBaseValueFor(req);
        if (baseValueFor != 0f)
        {
            stringBuilder.AppendLine("StatsReport_BaseValue".Translate() + ": " +
                                     stat.ValueToString(baseValueFor, numberSense));
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine(GearExplenation(req));
        return stringBuilder.ToString().TrimEndNewlines();
    }

    protected virtual string GearExplenation(StatRequest req)
    {
        var pawn = req.Thing as Pawn;
        if (pawn == null) return "";
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("StatsReport_RelevantGear".Translate());
        if (pawn.apparel != null)
        {
            var count = pawn.apparel.WornApparelCount;
            for (var l = 0; l < count; l++)
            {
                var gear = pawn.apparel.WornApparel[l];
                stringBuilder.AppendLine(TextFromGear(gear, count));
            }
        }

        if (pawn.equipment != null && pawn.equipment.Primary != null)
            stringBuilder.AppendLine(TextFromGear(pawn.equipment.Primary));
        stringBuilder.AppendLine();
        return stringBuilder.ToString();
    }

    private string TextFromGear(Thing gear, int gearCount = 1)
    {
        var f = OffsetFromGear(gear, gearCount);
        return "    " + gear.LabelCap + ": " + f.ToStringByStyle(stat.toStringStyle, ToStringNumberSense.Offset);
    }

    protected virtual float OffsetFromGear(Thing gear, int gearCount)
    {
        return gear.def.equippedStatOffsets.GetStatOffsetFromList(stat);
    }

    protected virtual float PawnCapacityOffset(Pawn pawn)
    {
        return 0;
    }
}

public class StatWorker_TiberiumInfResistance : StatWorker_Tiberium
{
    protected override float OffsetFromGear(Thing gear, int gearCount)
    {
        var baseVal = base.OffsetFromGear(gear, gearCount);
        baseVal += 1f - Mathf.Clamp01(gear.GetStatValue(StatDefOf.ToxicSensitivity) / gearCount);
        baseVal += 1f - Mathf.Clamp01(gear.GetStatValue(StatDefOf.ArmorRating_Sharp) / gearCount);
        return baseVal;
    }

    private static float InfectionChance(Pawn pawn, bool isGas)
    {
        var num = 1f;
        if (isGas)
        {
            var infFactor = 1f;
            infFactor *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumGasResistance);

            if (!pawn.CanBeInfected(true, out var gasFac)) return 0;

            num = gasFac;
            num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Breathing);
            num *= pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness) / 2f;
        }
        else
        {
            var infFactor = 1f;
            infFactor *= 1 - pawn.GetStatValue(TiberiumDefOf.TiberiumInfectionResistance);

            if (!pawn.CanBeInfected(false, out var infFact)) return 0;

            num = infFact;
            if (pawn.apparel != null)
            {
                float tox = 0, sharp = 0;
                foreach (var apparel in pawn.apparel.WornApparel)
                {
                    tox += Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ToxicSensitivity));
                    sharp += Mathf.Clamp01(apparel.GetStatValue(StatDefOf.ArmorRating_Sharp));
                }

                num *= tox;
                num *= 1 - Mathf.Clamp01(sharp);
            }
        }

        return num;
    }
}

public class StatWorker_TiberiumCorrosionResistance : StatWorker_Tiberium
{
}

public class StatWorker_TiberiumInfResistance : StatWorker_Tiberium
{
}

public class StatWorker_TiberiumGasResistance : StatWorker_Tiberium
{
}

public class StatWorker_TiberiumRadResistance : StatWorker_Tiberium
{
}

public class StatWorker_TiberiumDamageResistance : StatWorker_Tiberium
{
    public override bool ShouldShowFor(StatRequest req)
    {
        return req.HasThing && req.Thing.def.useHitPoints;
    }
}