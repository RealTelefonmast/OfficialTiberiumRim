using System;
using Verse;

namespace TR.Scrin;

public class DronePlatform : TRBuilding
{
    private bool spawnedPortal;
    private int ticksUntilPortal = 400;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void Tick()
    {
        base.Tick();
        if (ticksUntilPortal <= 0 && !spawnedPortal)
        {
            Predicate<IntVec3> cellCheck = x => x.Standable(Map);
            CellFinder.TryFindRandomCellNear(Position, Map, 6, cellCheck, out var result);
            GenPortal.SpawnDronePortal(result, Map);
            spawnedPortal = true;
        }
        else
        {
            ticksUntilPortal--;
        }
    }
}