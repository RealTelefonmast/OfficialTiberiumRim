using TeleCore.Utils;
using Verse;

namespace TeleCore.Atmosphere.Grid;

public class GasInterface : DefModExtension
{
    public SpreadingGasTypeDef gasType;
    public float startingValue;
}

public class GasInterfaceThing : Thing
{
    public GasInterface Interface => def.GetModExtension<GasInterface>();

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (Interface != null)
            Map.GetMapInfo<AtmosphericMapInfo>().TrySpawnGasAt(Position, Interface.gasType, Interface.startingValue);
        DeSpawn();
    }
}