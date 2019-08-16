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
        //Mutations
        public static AnimalMutationDef TiberiumFiendMutations;

        public static HediffDef MissingBodyPartInvisible;

        //Tiberium Meds
        public static HediffDef TiberBlockHediff;
        public static HediffDef TiberAddHediff;
        public static HediffDef TiberAddSide;

        //Tiberium Affect
        public static HediffRelativeDef TiberiumExposure;
        public static HediffDef TiberiumCrystallization;
        public static HediffDef TiberiumBurn;
        public static HediffDef CrystallizedPart;

        public static HediffDef CrystalBlade;
        public static HediffDef HandSpikes;

        //MutationGroups
        public static HediffMutationGroup Crystallized;
        public static HediffMutationGroup Visceral;
        public static HediffMutationGroup Enhanced;
        public static HediffMutationGroup Animal;

        public static HediffDef TiberiumMutation;
        public static HediffDef SymbioticPart;
        public static HediffDef VisceralPart;

        //Symbiotic Parts
        public static HediffDef TiberiumEnhancedArm;
        public static HediffDef TiberiumEnhancedLeg;

        //Visceral Parts
        public static HediffDef VisceralArm;
        public static HediffDef VisceralBlister;
        public static HediffDef ViscousPart;

        public static HediffDef TiberiumImmunity;

        public static NeedDef TiberiumNeed;

        public static TraitDef TiberiumTrait;
    }
}
