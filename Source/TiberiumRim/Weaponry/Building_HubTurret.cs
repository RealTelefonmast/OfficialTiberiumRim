using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_HubTurret : Building_TRTurret
    {
        public Building_TurretHub parentHub;

        public override CompRefuelable RefuelComp => parentHub.RefuelComp;
        public override CompPowerTrader PowerComp => parentHub.PowerComp;
        public override CompMannable MannableComp => parentHub.MannableComp;
        public override StunHandler Stunner => parentHub.Stunner;
        public override CompPower ForcedPowerComp => PowerComp;

        public override Vector3[] DrawPositions => new Vector3[]{ DrawPos, DrawPos, DrawPos, MainGun.top.barrels[0].DrawPos, MainGun.top.barrels[1].DrawPos };
        public override float?[] RotationOverrides => new float?[] { null, null, MainGun?.TurretRotation, MainGun?.TurretRotation, MainGun?.TurretRotation };

        public bool NeedsRepair => false;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ConnectToParent();
            Map.mapDrawer.MapMeshDirty(parentHub.Position, MapMeshFlag.Buildings);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            parentHub.RemoveHubTurret(this);
            Map.mapDrawer.MapMeshDirty(parentHub.Position, MapMeshFlag.Buildings);
            base.DeSpawn(mode);
        }

        public void ConnectToParent()
        {
            var hub = PlaceWorker_AtTurretHub.FindClosestTurretHub(this.def, Position, Map);
            hub?.AddHubTurret(this);
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
        }
    }
}
