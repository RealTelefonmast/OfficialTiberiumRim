﻿using UnityEngine;
using Verse;

namespace TR
{
    public class Hediff_TiberiumMutationPart : Hediff
    {
        public MutationState Mutation = MutationState.None;

        private bool Visceral => Mutation == MutationState.Visceral;
        public override Color LabelColor => Visceral ? TRColor.VisceralColor : TRColor.SymbioticColor;

        public override float Severity => Visceral ? 1 : 2;

        public override int CurStageIndex => Visceral ? 0 : 1;
        //public override HediffStage CurStage { get; }

        public enum MutationState
        {
            None,
            Visceral,
            Symbiotic
        }
    }
}
