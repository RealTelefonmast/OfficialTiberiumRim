using TR.World;
using Verse;

namespace TiberiumRim;

public class TiberiumStructure : FXBuilding
{
    public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
    public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        TiberiumComp.StructureInfo.TryRegister(this);
        foreach (var cell in this.OccupiedRect().Cells)
        {
            cell.GetTiberium(map)?.DeSpawn();
            cell.GetPlant(Map)?.DeSpawn();
        }
    }
}