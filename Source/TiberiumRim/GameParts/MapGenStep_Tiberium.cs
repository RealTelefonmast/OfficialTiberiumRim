using System;
using System.Collections.Generic;
using TeleCore;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TiberiumRim;

public class MapDefInjector : DefInjectBase
{
	public override bool AcceptsSpecial(Def def)
	{
		if (def is MapGeneratorDef)
		{
			return true;
		}
		return false;
	}

	public override void OnDefSpecialInjected(Def def)
	{
		var mapDef = def as MapGeneratorDef;
		mapDef.genSteps.Add(DefDatabase<GenStepDef>.GetNamed("TiberiumGenStep"));
		TRLog.Debug($"Def injected on {def}!");
	}
}

//
public class MapGenStep_Tiberium : GenStep
{
	//
	private static List<IntVec3> tmpMapCells = new List<IntVec3>();
	private static FastPriorityQueue<IntVec3> tmpInfestableCells;
	private static List<IntVec3> tmpInfestedCells = new List<IntVec3>();
	
	public override int SeedPart => 7943543;

	public override void Generate(Map map, GenStepParams parms)
	{
		var tibCoverage = Find.World.GetComponent<WorldComponent_TR>().TiberiumInfo.WorldCoverageAt(map.Tile);
	    TRLog.Debug($"Post-Processsing Map with Tiberium: {tibCoverage}");
	    if (tibCoverage <= 0) return;
        //var perlin = new Perlin(0.5, 2, 1, 6, map.ConstantRandSeed, QualityMode.High);
        InfestMapPercent(map, tibCoverage);
    }

    public static void InfestMapPercent(Map map, float coveragePct, int globSize = 1000)
    {
        coveragePct = Mathf.Clamp01(coveragePct);
        var tiberiumInfo = map.Tiberium().TiberiumInfo;
        float totalCoverage = tiberiumInfo.InfestationPercent;
        if (coveragePct == totalCoverage)
        {
            return;
        }
        
        //TODO CHANGE UP WITH TIB SPECIFIC
        List<IntVec3> allInfestableCells = map.pollutionGrid.AllPollutableCells;
        if (coveragePct < totalCoverage)
        {
	        /*
            tmpMapCells.Clear();
            tmpMapCells.AddRange(allInfestableCells.InRandomOrder());
            float num = totalCoverage - coveragePct;
            float num2 = Mathf.Max(0f, (float)tmpMapCells.Count * num);
            for (int i = 0; (float)i < num2; i++)
            {
				tiberiumInfo.Desinfest(tmpMapCells[i]);
            }
            tmpMapCells.Clear();
            */
        }
        else
        {
            int num3 = Mathf.FloorToInt((float)allInfestableCells.Count * coveragePct);
            int num4 = Mathf.CeilToInt(num3 / globSize);
            TRLog.Debug($"Making {num4} globs for a total of {num3} cells");
            for (int j = 0; j < num4; j++)
            {
                int num5 = Mathf.Min(globSize, num3);
                GrowTiberiumInfestationAt(allInfestableCells.RandomElementByWeight(GlobCellSelectionWeight), map, num5);
                num3 -= num5;
            }
        }
        
        float GlobCellSelectionWeight(IntVec3 c)
        {
	        //Defines a weight for each cell based on distance from center and distance from edge - resulting in cells inbetween to be more likely to be selected
            return 1f * (c.DistanceTo(map.Center) / map.Size.LengthHorizontal) * (c.DistanceToEdge(map) / map.Size.LengthHorizontal);
        }
    }

    public static void GrowTiberiumInfestationAt(IntVec3 root, Map map, int cellsToInfest = 4)
    {
	    TRLog.Debug($"Trying to grow tib glob at {root} of size {cellsToInfest}");
	    if (cellsToInfest < 0)
	    {
		    TRLog.Error("Tried to infest negative amount of cells.");
		    return;
	    }

	    TiberiumCrystalDef crystalDef = Rand.Chance(0.05f) ? TiberiumDefOf.TiberiumBlue : TiberiumDefOf.TiberiumGreen;
	    
	    var tiberiumInfo = map.Tiberium().TiberiumInfo;
	    if (tiberiumInfo.TotalCount >= map.cellIndices.NumGridCells)
	    {
		    //Illegal state should never occur
		    return;
	    }

	    if (root.Walkable(map))
	    {
		    //tiberiumInfo.TiberiumGrid.CanInfest(root);
		    GenTiberium.SpawnTiberium(root, map, crystalDef);
		    cellsToInfest--;
	    }

	    if (cellsToInfest <= 0)
	    {
		    return;
	    }

	    tmpInfestableCells = new FastPriorityQueue<IntVec3>(new TiberiumCellComparer(root, map));
	    map.floodFiller.FloodFill(root, x => x.HasTiberium(map), delegate(IntVec3 x) { tmpInfestedCells.Add(x); });
	    TRLog.Debug($"Got root cells: {tmpInfestedCells.Count}");
	    if (tmpInfestedCells.Count == 0)
	    {
		    return;
	    }

	    tmpInfestableCells.Clear();
	    for (int i = 0; i < tmpInfestedCells.Count; i++)
	    {
		    foreach (IntVec3 adjacentPollutableCell in GetAdjacentInfestableCells(tmpInfestedCells[i], map))
		    {
			    if (!tmpInfestableCells.Contains(adjacentPollutableCell))
			    {
				    tmpInfestableCells.Push(adjacentPollutableCell);
			    }
		    }
	    }
	    tmpInfestedCells.Clear();
	    
	    TRLog.Debug($"Got infestable cells: {tmpInfestableCells.Count} with cells left: {cellsToInfest}");
	    while (cellsToInfest > 0 && tmpInfestableCells.Count > 0)
	    {
		    IntVec3 intVec = tmpInfestableCells.Pop();
		    TRLog.Debug($"Spawning tib {crystalDef}.. {intVec}");
		    var tiberium = GenTiberium.SpawnTiberium(intVec, map, crystalDef);
		    if (tiberium != null)
		    {
			    tiberium.Growth = Rand.Range(0.5f, 1f);
			    foreach (var adjCell in GetAdjacentInfestableCells(intVec, map))
			    {
				    if (!tmpInfestableCells.Contains(adjCell))
				    {
					    tmpInfestableCells.Push(adjCell);
				    }
			    }
		    }

		    cellsToInfest--;
	    }
	    
	    IEnumerable<IntVec3> GetAdjacentInfestableCells(IntVec3 c, Map m)
	    {
		    foreach (var t in GenAdj.CardinalDirections)
		    {
			    IntVec3 intVec3 = c + t;
			    if (intVec3.InBounds(map) && intVec3.Walkable(map))
			    {
				    yield return intVec3;
			    }
		    }
	    }
    }
    
    internal class TiberiumCellComparer : IComparer<IntVec3>
    {
	    private const float NoisyEdgeFactor = 0.25f;
	    private const float PerlinNoiseFactor = 2f;
	    private IntVec3 root;
	    private Map map;
	    private ModuleBase perlin;
	    
	    public TiberiumCellComparer(IntVec3 root, Map map, float frequency = 0.015f)
	    {
		    this.root = root;
		    this.map = map;
		    perlin = new Perlin((double)frequency, 2.0, 0.5, 6, map.uniqueID, QualityMode.Medium);
		    perlin = new ScaleBias(0.5, 0.5, this.perlin);
	    }

	    private float InfestScore(IntVec3 c)
	    {
		    var num = 1f;
		    num *= 1f / c.DistanceTo(root);
		    num *= 1f + (float) perlin.GetValue(c.x, c.y, c.z) * PerlinNoiseFactor;
		    if (MapGenerator.mapBeingGenerated == map)
			    num *= c.DistanceTo(MapGenerator.PlayerStartSpot) / map.Size.LengthHorizontal;
		    return num * (1f + AdjacentTibCount(c) / 8f * NoisyEdgeFactor);
	    }

	    private int AdjacentTibCount(IntVec3 c)
	    {
		    var num = 0;
		    for (var i = 0; i < GenAdj.AdjacentCells.Length; i++)
		    {
			    var b = GenAdj.AdjacentCells[i];
			    var c2 = c + b;
			    if (c2.InBounds(map) && c2.HasTiberium(map)) 
				    num++;
		    }

		    return num;
	    }

	    public int Compare(IntVec3 a, IntVec3 b)
	    {
		    var num = InfestScore(a);
		    var num2 = InfestScore(b);
		    if (num < num2) return 1;
		    if (num > num2) return -1;
		    return 0;
	    }
    }
}