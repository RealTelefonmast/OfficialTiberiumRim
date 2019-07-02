using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class TiberiumControlDef : Def
    {
        public float VeinHitDamage;
        public float TiberiumLeakScale;
        public float WorldCorruptMinPct;
        public float WorldCorruptAdder;
        public float TiberiumMinTemp;
        public float AmalgamationChance;
        public float workFloat;
        public int cellsPerMonolith;
        //Corruption 
        public float GeyserCorruptionChance;
        public float RockCorruptionChance;
        public float WallCorruptionChance;
        public float ChunkCorruptionChance;

        public Color AlertColor;
        public Color GreenColor;
        public Color BlueColor;
        public Color RedColor;
        public Color SludgeColor;
        public Color GasColor;

        public static TiberiumControlDef Named(string defName)
        {
            return DefDatabase<TiberiumControlDef>.GetNamed(defName, true);
        }
    }

    [DefOf]
    public static class MainTCD
    {
        public static TiberiumControlDef Main;
    }
}