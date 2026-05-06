using RimWorld;
using TeleCore.PlaceWorkers;
using TeleCore.Types;
using TeleCore.Types.Utils;
using TeleCore.Visual.VFX.FX.Layer;
using Verse;

namespace TeleCore.Buildings;

public class Building_HubTurret : Building_TeleTurret
{
    public Building_TurretHubCore parentHub;

    public override CompRefuelable RefuelComp => parentHub.RefuelComp;
    public override CompPowerTrader PowerTraderComp => parentHub.PowerTraderComp;
    public override CompMannable MannableComp => parentHub.MannableComp;

    public override StunHandler Stunner
    {
        get
        {
            if (parentHub?.Stunner != null && parentHub.stunner.Stunned)
                return parentHub.stunner;
            return stunner;
        }
    }

    public override CompCanBeDormant DormantComp => parentHub.DormantComp;
    public override CompInitiatable InitiatableComp => parentHub.InitiatableComp;
    public override CompNetwork NetworkComp => parentHub.NetworkComp;

    public bool NeedsRepair => false;

    //
    public override CompPowerTrader FX_PowerProviderFor(FXArgs args)
    {
        return PowerTraderComp;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        ConnectToParent();
        Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Buildings | MapMeshFlagDefOf.Terrain);
        Map.mapDrawer.MapMeshDirty(parentHub.Position, MapMeshFlagDefOf.Buildings | MapMeshFlagDefOf.Terrain);
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        parentHub.RemoveHubTurret(this);
        Map.mapDrawer.MapMeshDirty(parentHub.Position, MapMeshFlagDefOf.Buildings);
        base.DeSpawn(mode);
    }

    public void ConnectToParent()
    {
        var hub = PlaceWorker_AtTurretHub.FindClosestTurretHub(def, Position, Map);
        if (hub == null)
        {
            TLog.Error($"{this} failed to find a parent hub! Destroying...");
            Destroy();
            return;
        }

        hub?.AddHubTurret(this);
    }

    public override void Print(SectionLayer layer)
    {
        base.Print(layer);
    }
}