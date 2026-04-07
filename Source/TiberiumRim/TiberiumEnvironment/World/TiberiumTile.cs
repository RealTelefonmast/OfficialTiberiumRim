using System.Collections.Generic;
using System.Text;
using RimWorld.Planet;
using TR.DefOf;
using TR.Util;
using Verse;

namespace TR.TiberiumEnvironment.World;

public class TiberiumTile : WorldObject
{
    protected float coverageInt;

    private bool hasSpread;

    public bool shouldSpawnNeighour = true;

    public Map Map => Find.World.worldObjects.WorldObjectAt<MapParent>(Tile)?.Map;
    public bool HasMap => Map != null;

    public float Coverage
    {
        get => HasMap ? Map.Tiberium().TiberiumInfo.Coverage : coverageInt;
        set => coverageInt = value;
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Coverage: " + Coverage);
        return sb.ToString();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref coverageInt, "tiberiumCoverage");
    }

    //TODO: fix memory alloc
    public override void SpawnSetup()
    {
        base.SpawnSetup();
        //Find.World.renderer

        Log.Message("World Tile: " + Tile);
        if (shouldSpawnNeighour)
        {
            var tempNeighbors = new List<PlanetTile>();
            Find.WorldGrid.GetTileNeighbors(Tile, tempNeighbors);
            for (var i = 0; i < tempNeighbors.Count; i++)
            {
                var tibTile = (TiberiumTile)WorldObjectMaker.MakeWorldObject(TiberiumDefOf.TiberiumTile);
                tibTile.Tile = tempNeighbors[i];
                tibTile.shouldSpawnNeighour = false;
                tibTile.coverageInt = Rand.Range(0.25f, 1);
                Find.World.worldObjects.Add(tibTile);
            }
        }
    }

    private float InfestationPerDay()
    {
        return 0;
    }

    public override void Tick()
    {
        base.Tick();
        if (this.IsHashIntervalTick(250))
            Find.World.renderer.SetDirty<WorldDrawLayer_Tiberium>(TRFind.CurPlanetLayer);
        //Update Infestation
        if (!HasMap && this.IsHashIntervalTick(10))
        {
            if (Coverage < 1f)
                Coverage += 0.01f;
            if (Coverage >= 0.45f && !hasSpread)
                Spread();
        }
    }

    //TODO: fix memory alloc
    private void Spread()
    {
        var tempNeighbors = new List<PlanetTile>();
        Find.WorldGrid.GetTileNeighbors(Tile, tempNeighbors);
        for (var i = 0; i < tempNeighbors.Count; i++)
        {
            if (Find.World.worldObjects.WorldObjectAt<TiberiumTile>(tempNeighbors[i]) != null) continue;
            var tibTile = (TiberiumTile)WorldObjectMaker.MakeWorldObject(TiberiumDefOf.TiberiumTile);
            tibTile.Tile = tempNeighbors[i];
            tibTile.shouldSpawnNeighour = false;
            tibTile.coverageInt = 0.01f;
            Find.World.worldObjects.Add(tibTile);
        }

        hasSpread = true;
    }
}