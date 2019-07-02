using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class MapComponent_TiberiumWater : MapComponent
    {
        public HashSet<IntVec3> WaterCells = new HashSet<IntVec3>();
        public HashSet<IntVec3> RiverCells = new HashSet<IntVec3>();

        public MapComponent_TiberiumWater(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
            WaterCells.AddRange(map.AllCells.Where(c => c.GetTerrain(map).IsWater).ToList());
            RiverCells.AddRange(WaterCells.Where(c => c.GetTerrain(map).IsRiver).ToList());
        }
    }
}
