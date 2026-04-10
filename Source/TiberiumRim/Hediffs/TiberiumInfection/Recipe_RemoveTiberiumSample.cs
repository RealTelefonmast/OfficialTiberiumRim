using System.Collections.Generic;
using RimWorld;
using UnityEngine.Pool;
using Verse;

namespace TR.TiberiumInfection;

public class Recipe_RemoveTiberiumSample : Recipe_Surgery
{
    private readonly Hediff_Crystallizing tempHediff = null;

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        var hediffs = ListPool<Hediff_Crystallizing>.Get();
        pawn.health.hediffSet.GetHediffs(ref hediffs);
        foreach (var hediff in hediffs)
            if (!hediff.Part.IsCorePart && hediff.HasAvailableSample && hediff.Severity < 0.95f)
                yield return hediff.Part;
        ListPool<Hediff_Crystallizing>.Release(hediffs);
    }

    public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
    {
        return pawn.Faction != billDoerFaction && pawn.Faction != null &&
               HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        //bool flag = MedicalRecipesUtility.IsClean(pawn, part);
        //bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
        if (billDoer == null || !recipe.products.NullOrEmpty()) return;
        if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill)) return;
        TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        for (var i = 0; i < recipe.products.Count; i++)
        {
            var prod = recipe.products[i];
            var product = ThingMaker.MakeThing(prod.thingDef);
            product.stackCount = prod.count;
            GenSpawn.Spawn(product, pawn.Position, pawn.Map);
        }

        if (!pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss))
        {
            var hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
            hediff.Severity = 0.05f;
        }

        tempHediff.RemoveSample();
    }
}