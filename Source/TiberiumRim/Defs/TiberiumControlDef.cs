using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TR;

public class TiberiumControlDef : Def
{
    public Color AlertColor;
    public float AmalgamationChance;
    public Color BlueColor;
    public int cellsPerMonolith;
    public float ChunkCorruptionChance;

    public Color GasColor;

    //Corruption
    public float GeyserCorruptionChance;
    public Color GreenColor;
    public Color RedColor;
    public float RockCorruptionChance;
    public Color SludgeColor;

    public List<ThingDef> spreadFilter;
    public float TiberiumLeakScale;
    public float TiberiumMinTemp;
    public float VeinHitDamage;
    public float WallCorruptionChance;
    public float workFloat;
    public float WorldCorruptAdder;
    public float WorldCorruptMinPct;

    public static TiberiumControlDef Named(string defName)
    {
        return DefDatabase<TiberiumControlDef>.GetNamed(defName);
    }
}

[DefOf]
public static class MainTCD
{
    public static TiberiumControlDef Main;
}