using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Hediff_TiberiumMutation : HediffWithComps
    {
        private bool symbioticBias = false;
        private bool visceralBias = false;

        public override string LabelInBrackets => pawn.Dead ? null : (VisceralPct + SymbioticPct).ToStringPercent();//VisceralRisk.ToStringPercent() + "/x" + MutationSpeed;

        //public override float PainFactor { get; }

        //public override float PainOffset => 0.15f;

        private Comp_TRHealthCheck HealthComp => pawn.GetComp<Comp_TRHealthCheck>();

        private IEnumerable<Hediff_TiberiumMutationPart> MutationParts => pawn.health.hediffSet.GetHediffs<Hediff_TiberiumMutationPart>();

        private bool CanFinalize => (Mathf.RoundToInt(VisceralPct * 100f) + Mathf.RoundToInt(SymbioticPct * 100)) / 100 == 1;
        private int VisceralParts => MutationParts.Count(p => p.Mutation == Hediff_TiberiumMutationPart.MutationState.Visceral);
        private int SymbioticParts => MutationParts.Count(p => p.Mutation == Hediff_TiberiumMutationPart.MutationState.Symbiotic);
        private float VisceralPct => VisceralParts / (float)HealthComp.NonMisingPartsCount;
        private float SymbioticPct => SymbioticParts / (float)HealthComp.NonMisingPartsCount;

        //Pawn's health determines how well the mutation goes
        //Tib Mutation is naturally aggressive and bad 
        public float VisceralRisk
        {
            get
            {
                //Naturally mutation is aggressive
                float num = 1f;
                //Pawn's health may add to bad mutation probability
                num += 1f - pawn.Health();
                //
                if (VisceralPct > 0) 
                    num *= (SymbioticPct/VisceralPct);
                if (visceralBias)
                    num *= 1.25f;
                if(symbioticBias)
                    num *= 0.7f;
                //Being in Tiberium increases the probability
                if (pawn.Position.GetTiberium(pawn.Map) != null)
                    num *= 2f;
                return num * 0.5f;
            }
        }

        public int MutationSpeed
        {
            get
            {
                if (pawn.HealthComp().IsInTiberium) return 1;
                return Mathf.RoundToInt(Mathf.Lerp(1, 8, 1 - pawn.health.hediffSet.GetFirstHediffOfDef(TRHediffDefOf.TiberiumToxemia).Severity));
            }
        }

        public override void PostMake()
        {
            base.PostMake();
            symbioticBias = TRUtils.Chance(0.40f);
            visceralBias = !symbioticBias && TRUtils.Chance(0.475f);
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            //Should always affect whole body
            if (Part != null)
                Part = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void Tick()
        {
            base.Tick();
            if (!pawn.IsHashIntervalTick(750 * MutationSpeed)) return;
            //SLowly turn each part of the pawn into a mutated part
            MutateRandomNewPart();

            if (!CanFinalize) return;
            //Once all parts are mutated, finalize and decide pawn's fate
            FinalizeMutation();
        }

        private void MutateRandomNewPart()
        {
            var hediffSet = pawn.health.hediffSet;
            var parts = (from x in HealthComp.NonMissingParts
                where !hediffSet.PartHasHediff(x, TRHediffDefOf.TiberiumMutationPart)
                select x).ToList();
            if (!parts.Any()) return;
            var randomPart = parts.RandomElement();

            Hediff_TiberiumMutationPart mutationPart = (Hediff_TiberiumMutationPart)HediffMaker.MakeHediff(TRHediffDefOf.TiberiumMutationPart, pawn, null);
            float symbioticChance = 1 - VisceralRisk;
            bool visceral = TRUtils.Chance(VisceralRisk);
            if (visceral)
            {
                mutationPart.Mutation = Hediff_TiberiumMutationPart.MutationState.Visceral;
                if (TRUtils.Chance(VisceralRisk * VisceralRisk))
                {
                    //Add worse visceral effect (blisters, viscous organ)
                    HediffDef hediff = TRHediffDefOf.Visceral.HediffFor(randomPart);
                    if (hediff != null)
                    {
                        pawn.health.AddHediff(hediff, randomPart);
                    }
                }
            }
            else
            {
                mutationPart.Mutation = Hediff_TiberiumMutationPart.MutationState.Symbiotic;
                if (TRUtils.Chance(symbioticChance * symbioticChance))
                {
                    //
                    HediffDef hediff = TRHediffDefOf.Symbiotic.HediffFor(randomPart);
                    if (hediff != null)
                    {
                        pawn.health.AddHediff(hediff, randomPart);
                    }
                }
            }

            mutationPart.Mutation = TRUtils.Chance(VisceralRisk)
                ? Hediff_TiberiumMutationPart.MutationState.Visceral
                : Hediff_TiberiumMutationPart.MutationState.Symbiotic;
            pawn.health.AddHediff(mutationPart, randomPart);
        }

        private void FinalizeMutation()
        {
            bool visceral = VisceralPct > SymbioticPct;
            if (visceral && TRUtils.Chance(VisceralPct))
            {
                HediffUtils.FormVisceralPod(pawn);
            }
            else if(TRUtils.Chance(SymbioticPct))//Symbiotic - Turns animals into fiends, pawns into mutants
            {
                if (pawn.RaceProps.Animal)
                {
                    var mutations = TRHediffDefOf.TiberiumFiendMutations;
                    var kind = mutations.TiberiumFiendFor(pawn.kindDef);
                    if (kind != null)
                    {
                        PawnGenerationRequest request = new PawnGenerationRequest(kind);
                        Pawn newPawn = PawnGenerator.GeneratePawn(request);
                        newPawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
                        newPawn.ageTracker.AgeBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
                        newPawn.ageTracker.BirthAbsTicks = pawn.ageTracker.BirthAbsTicks;
                        //newPawn.health = pawn.health;
                        GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map);
                        pawn.DeSpawn();
                        return;
                    }
                }
                ApplyImmunity();
            }
            pawn.health.RemoveHediff(this);
            var partList = MutationParts.ToList();
            foreach (var hediff in partList)
            {
                pawn.health.RemoveHediff(hediff);
            }
            pawn.health.AddHediff(TRHediffDefOf.MutationRecovery);
        }

        private void ApplyImmunity()
        {
            pawn.health.AddHediff(TRHediffDefOf.TiberiumImmunity);
            for (var index = pawn.health.hediffSet.hediffs.Count - 1; index >= 0; index--)
            {
                var hediff = pawn.health.hediffSet.hediffs[index];
                if (hediff is Hediff_CrystallizingPart && TRUtils.Chance(hediff.Severity))
                {
                    HediffDef crystalDiff = TRHediffDefOf.Crystallized.HediffFor(hediff.Part);
                    if (crystalDiff != null)
                    {
                        pawn.health.AddHediff(crystalDiff, hediff.Part);
                        pawn.health.RemoveHediff(hediff);
                    }
                }
            }
        }


        public void DrawMutation(Rect rect, ref float curY)
        {
            float viscNum = VisceralPct;
            float symbNum = SymbioticPct;
            float reverseVisc = 1f - viscNum;

            string s2 = viscNum == symbNum ? " = " : (viscNum > symbNum ? " > " : " < ");
            Vector2 vec = Text.CalcSize(s2);
            string visc = "◀ " + "TR_Visceral".Translate() + " " + viscNum.ToStringPercent();
            Vector2 vec1 = Text.CalcSize(visc);
            string symb = symbNum.ToStringPercent() + " " + "TR_Symbiotic".Translate() + " ▶";
            Vector2 vec2 = Text.CalcSize(symb);

            Rect rectSide = new Rect((rect.width / 2f) - (vec.x / 2f), curY, vec.x, vec.y);
            Rect rect1 = new Rect((rect.width / 2f) - vec1.x - rectSide.width / 2f, curY, vec1.x, vec1.y);
            Rect rect2 = new Rect((rect.width / 2f) + rectSide.width / 2f, curY, vec2.x, vec2.y);
            curY += rect1.height;

            Rect fillRectTotal = new Rect(0f, curY, rect.width, 18f).ContractedBy(3);
            Rect visceral = fillRectTotal.LeftHalf();
            Rect symbiotic = fillRectTotal.RightHalf();
            Rect fullArea = new Rect(fillRectTotal.xMin, rect1.yMin, fillRectTotal.width, fillRectTotal.height + rect1.height);
            curY += fillRectTotal.ExpandedBy(3).height;

            GUI.color = TRColor.VisceralColor;
            Widgets.Label(rect1, visc);
            GUI.color = Color.white;
            Widgets.Label(rectSide, s2);
            GUI.color = TRColor.SymbioticColor;
            Widgets.Label(rect2, symb);
            GUI.color = Color.white;
            Widgets.FillableBar(visceral, reverseVisc, TRMats.grey, TRMats.MutationVisceral, false);
            Widgets.FillableBar(symbiotic, symbNum, TRMats.MutationSymbiotic, TRMats.grey, false);
            TooltipHandler.TipRegion(fullArea, "TR_MutationTip".Translate());
        }
    }
}
