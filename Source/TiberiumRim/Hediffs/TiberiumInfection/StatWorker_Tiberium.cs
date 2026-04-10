using RimWorld;
using TeleCore.Utils;
using UnityEngine;
using Verse;

namespace TR.TiberiumInfection;

public class StatWorker_Tiberium : StatWorker
{
    public override bool IsDisabledFor(Thing thing)
    {
        if (thing is MechanicalPawn) return false;
        return base.IsDisabledFor(thing);
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return req.HasThing && req.Thing.def.category == ThingCategory.Pawn;
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
        return req.HasThing && GenData.IsBuilding(req.Thing.def);
    }
}
