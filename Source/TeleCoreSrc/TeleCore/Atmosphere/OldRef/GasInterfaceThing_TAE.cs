// Preserved from TeleCore/SpreadingGas/GasInterfaceThing.cs

using Verse;

namespace TeleCore.Atmosphere.OldRef;

public class GasInterface_TAE : DefModExtension
{
    public TAE.SpreadingGasTypeDef gasType;
    public float startingValue;
}

public class GasInterfaceThing_TAE : Thing
{
    public GasInterface_TAE Interface => def.GetModExtension<GasInterface_TAE>();

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (Interface != null)
            Map.GetMapInfo<TAE.AtmosphericMapInfo>()
                .TrySpawnGasAt(Position, Interface.gasType, Interface.startingValue);
        DeSpawn();
    }
}