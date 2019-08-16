﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Hediff_Mutation : HediffWithComps
    {
        private Texture2D icon;
        private static IntRange wanderRange = new IntRange(100, 750);//TODO: Change Back To Higher
        private int ticksLeft;

        public override TextureAndColor StateIcon
        {
            get
            {
                if (icon == null)
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/Mutation", true);
                return icon;
            }
        }

        public override string LabelInBrackets => VisceralRisk.ToStringPercent();
        public override float PainFactor => base.PainFactor + (VisceralCoverage * 2);

        public override void PostMake()
        {
            base.PostMake();
            if (Part != null)
            {
                Part = null;
            }
        }

        public override void Tick()
        {
            base.Tick();
            //Spread mutation on the body, slowly mutate part by part
            if (ticksLeft <= 0)
                Wander();
            ticksLeft--;

            //Finalize mutation, occasionally turn animals into fiends and pawns into mutants
            if (CanFinalize)
                FinalizeMutation();
        }

        private void Wander()
        {
            HediffDef hediffDef = null;
            var potentialPart = pawn.health.hediffSet.GetWanderParts().RandomElement();
            if (TRUtils.Chance(VisceralRisk))
                hediffDef = TRHediffDefOf.VisceralPart;
            else
                hediffDef = TRHediffDefOf.SymbioticPart;
            pawn.health.AddHediff(hediffDef, potentialPart);
            ticksLeft = TRUtils.Range(wanderRange);
        }

        private void FinalizeMutation()
        {
            //Chance based on Symbiotic Coverage
            //Turns animals into fiends, pawns into mutants
            if (TRUtils.Chance(Mathf.Pow(SymbioticCoverage, 2)))
                SymbioticAdaptation();
            //If unlucky, may lead to organ failure, cancer, or potentially forms a visceroid
            else

                return;
        }

        private void SymbioticAdaptation()
        {
            if (pawn.RaceProps.Animal)
            {
                var mutations = TRHediffDefOf.TiberiumFiendMutations;
                var kind = mutations.TiberiumFiendFor(pawn.kindDef);
                if (kind != null)
                {
                    PawnGenerationRequest request = new PawnGenerationRequest(kind);
                    Pawn newPawn = PawnGenerator.GeneratePawn(request);
                    newPawn.ageTracker = pawn.ageTracker;
                    newPawn.health = pawn.health;
                    GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map);
                    pawn.DeSpawn();
                    return;
                }
            }
            pawn.health.AddHediff(TRHediffDefOf.TiberiumImmunity);
        }
        
        private bool CanFinalize => VisceralCoverage + SymbioticCoverage >= 1f;

        //Pawn's health determines how well the mutation goes
        //Tib Mutation is naturally aggressive and bad though
        private float VisceralRisk
        {
            get
            {
                //Naturally mutation is aggressive
                float num = 1f;
                //Pawn's health may add to bad mutation probability
                num += 1f - pawn.Health();
                //Crystallizing parts make up 1/3 of the mutation probability
                var hediffs = pawn.health.hediffSet.GetHediffs<Hediff_Crystallizing>().ToArray();
                if (hediffs.Any())
                    num += hediffs.Sum(h => h.Severity) / hediffs.Count();
                //Being in Tiberium worsens the probability
                if (pawn.Position.GetTiberium(pawn.Map) != null)
                    num += 0.75f;
                return num / 3f;
            }
        }

        public float SymbioticCoverage
        {
            get
            {
                float count = pawn.health.hediffSet.hediffs.Count(h => h.def == TRHediffDefOf.SymbioticPart);
                return count / TotalCoverage();
            }
        }

        public float VisceralCoverage
        {
            get
            {
                float count = pawn.health.hediffSet.hediffs.Count(h => h.def == TRHediffDefOf.VisceralPart);
                return count / TotalCoverage();
            }
        }

        private float TotalCoverage()
        {
            var set = pawn.health.hediffSet;
            return set.GetWanderParts().Count() + set.GetHediffs<Hediff_MutationPart>().Count();
        }
    }
}
