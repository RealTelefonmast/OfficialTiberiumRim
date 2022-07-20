using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Zone_HarvestTiberium : Zone
    {
        private CompTNS_Refinery parentRefinery;
        public override Color NextZoneColor => ZoneColorUtility.NextStorageZoneColor();

        public CompTNS_Refinery ParentRefinery
        {
            get => parentRefinery;
            set => parentRefinery = value;
        }

        public Zone_HarvestTiberium(ZoneManager zoneManager) : base("TR_HarvestTiberiumZone".Translate(), zoneManager)
        {
        }

        public override IEnumerable<Gizmo> GetZoneAddGizmos()
        {
            yield return parentRefinery.ZoneDesignator;
        }

        public override void AddCell(IntVec3 c)
        {
            if (cells.Contains(c))
            {
                Log.Error($"Adding cell to zone which already has it. c={c}, zone={this}");
                return;
            }

            var list = Map.thingGrid.ThingsListAt(c);
            for (var i = 0; i < list.Count; i++)
            {
                var thing = list[i];
                if (!thing.def.CanOverlapZones)
                {
                    Log.Error($"Added zone over zone-incompatible thing {thing}");
                    return;
                }
            }

            cells.Add(c);
            zoneManager.AddZoneGridCell(this, c);
            Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
            cellsShuffled = false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void PostRegister()
        {
            base.PostRegister();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos();
        }
    }
}
