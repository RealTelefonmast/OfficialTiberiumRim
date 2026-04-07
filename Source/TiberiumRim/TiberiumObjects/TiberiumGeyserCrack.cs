using TR.DefOf;
using TR.GameParts;
using Verse;

namespace TR.TiberiumObjects;

public class TiberiumGeyserCrack : TRBuilding
{
    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        foreach (var cell in this.OccupiedRect())
            Map.terrainGrid.SetTerrain(cell, TiberiumTerrainDefOf.TiberiumSoilGreen);
    }
}