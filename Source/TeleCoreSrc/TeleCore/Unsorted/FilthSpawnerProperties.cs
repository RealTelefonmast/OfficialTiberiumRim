using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TeleCore.Unsorted;

public class FilthSpawnerProperties
{
    public List<DefValueLoadable<ThingDef, float>> filths;
    public float spreadRadius = 1.9f;

    public void SpawnFilth(IntVec3 center, Verse.Map map)
    {
        foreach (var cell in GenRadial.RadialCellsAround(center, spreadRadius, true))
            foreach (var filth in filths)
                if (Rand.Chance(filth.Value))
                {
                    FilthMaker.TryMakeFilth(cell, map, filth.Def);
                    break;
                }
    }
}