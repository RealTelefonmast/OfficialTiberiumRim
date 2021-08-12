using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Recipe_RemoveTiberiumSample : Recipe_Surgery
    {
        private Hediff_CrystallizingPart tempHediff = null;

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            var hediffs = pawn.health.hediffSet.GetHediffs<Hediff_CrystallizingPart>();
            foreach (var hediff in hediffs)
            {
                if (!hediff.Part.IsCorePart && !hediff.SampleTaken && hediff.Severity < 0.95f)
                    yield return hediff.Part;
            }
        }

        public override bool IsViolationOnPawn(Pawn pawn, BodyPartRecord part, Faction billDoerFaction)
        {
            return pawn.Faction != billDoerFaction && pawn.Faction != null && HealthUtility.PartRemovalIntent(pawn, part) == BodyPartRemovalIntent.Harvest;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            //bool flag = MedicalRecipesUtility.IsClean(pawn, part);
            //bool flag2 = this.IsViolationOnPawn(pawn, part, Faction.OfPlayer);
            if (billDoer == null || !recipe.products.NullOrEmpty()) return;
            tempHediff = (Hediff_CrystallizingPart)pawn.health.hediffSet.GetHediffAt(part, TRHediffDefOf.CrystallizingPart);
            if (tempHediff == null) return;

            if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
            {
                tempHediff.RemoveSample(true);
                return;
            }
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
            {
                billDoer,
                pawn
            });
            for (int i = 0; i < recipe.products.Count; i++)
            {
                ThingDefCountClass prod = recipe.products[i];
                Thing product = ThingMaker.MakeThing(prod.thingDef);
                product.stackCount = prod.count;
                GenSpawn.Spawn(product, pawn.Position, pawn.Map);
            }
            if (!pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss))
            {
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.BloodLoss, pawn);
                hediff.Severity = 0.05f;

            }
            tempHediff.RemoveSample(false);
        }
    }
}
