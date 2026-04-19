using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim;

public class TNW_Silo : TiberiumNetworkBuilding
{
    public override IEnumerable<IntVec3> ConnectableCells
    {
        get
        {
            var rect = this.OccupiedRect();
            var cells = rect.Cells.ToList();
            rect.Corners.ToList().ForEach(x => cells.Remove(x));
            return cells;
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);
    }
}