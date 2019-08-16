using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumStructure : FXBuilding
    {
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            TiberiumComp.StructureInfo.TryRegister(this);
            foreach(IntVec3 cell in this.OccupiedRect().Cells)
            {
                cell.GetTiberium(map)?.DeSpawn();
                cell.GetPlant(Map)?.DeSpawn();
            }
        }

        public WorldComponent_Tiberium WorldTiberiumComp => Find.World.GetComponent<WorldComponent_Tiberium>();
        public MapComponent_Tiberium TiberiumComp => Map.GetComponent<MapComponent_Tiberium>();
    }
}
