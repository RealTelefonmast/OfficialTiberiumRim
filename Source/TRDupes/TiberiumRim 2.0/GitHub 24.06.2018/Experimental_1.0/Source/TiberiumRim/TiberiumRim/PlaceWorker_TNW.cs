using System;
using System.Linq;
using System.Collections.Generic;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class PlaceWorker_TNW: PlaceWorker
    {
        private List<IntVec3> cellList = new List<IntVec3>();

        public Map Map = Current.Game.VisibleMap;
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null)
        {
            FillLists(map);
            if (cellList.Contains(loc))
            {
                return true;
            }
            return false;
        }

        public void FillLists(Map map)
        {
            List<Thing> thingList = map.listerThings.AllThings.FindAll((Thing x) => !x.CellsAdjacent8WayAndInside().Any((IntVec3 v) => v.InBounds(map) && v.GetThingList(map).Any((Thing y) => y.def.defName.Contains("Connector_TNW_Blueprint"))) && x.TryGetComp<Comp_TNW>() != null && x.TryGetComp<Comp_TNW>()?.Connector == null);
            if (thingList.Count > 0)
            {
                for (int i = 0; i < thingList.Count; i++)
                {
                    List<IntVec3> cells = thingList[i].OccupiedRect().ExpandedBy(1).EdgeCells.ToList<IntVec3>();
                    foreach (IntVec3 cell in cells)
                    {
                        if (!cellList.Contains(cell))
                        {
                            cellList.Add(cell);
                        }
                    }
                }
            }
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            FillLists(Map);
            for (int i = 0; i < cellList.Count; i++)
            {
                IntVec3 c = cellList[i];
                CellRenderer.RenderCell(c, 0.44f);             
            }
        }
    }
}
