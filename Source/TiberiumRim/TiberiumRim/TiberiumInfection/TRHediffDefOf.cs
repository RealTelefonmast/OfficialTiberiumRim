using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    [DefOf]
    public static class TRHediffDefOf
    {
        //Tiberium Meds
        public static HediffDef TiberBlockHediff;
        public static HediffDef TiberAddHediff;
        public static HediffDef TiberAddSide;

        //Tiberium Affect
        public static HediffRelativeDef TiberiumExposure;
        public static HediffDef TiberiumCrystallization;
        public static HediffDef TiberiumGroundLock;
        public static HediffDef TiberiumBurn;
        public static HediffDef CrystallizedPart;

        public static HediffDef CrystalBlade;
        public static HediffDef HandSpikes;

        //MutationGroups
        public static HediffMutationGroup Crystallized;
        public static HediffMutationGroup Visceral;
        public static HediffMutationGroup Enhanced;

        public static HediffDef TiberiumMutation;
        public static HediffDef TiberiumMutationPart;
        public static HediffDef SymbioticPart;
        public static HediffDef VisceralPart;

        public static HediffDef VisceralBlister;
        public static HediffDef ViscousPart;

        public static HediffDef TiberianArm;
        public static HediffDef TiberianLeg;
        public static HediffDef VisceralArm;

        public static HediffDef SymbioticCore;

        public static NeedDef TiberiumNeed;
    }
}
