using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class MapComponent_TiberiumWater : MapComponent, ICellBoolGiver
    {
        public BoolGrid waterCells;
        public BoolGrid riverCells;
        public BoolGrid corruption;

        public IntGrid corruptionInt;

        //
        public BoolGrid landableCells;

        private CellBoolDrawer drawer;

        private List<IntVec3> dirtyCells = new List<IntVec3>();

        public MapComponent_TiberiumWater(Map map) : base(map)
        {
            waterCells = new BoolGrid(map);
            riverCells = new BoolGrid(map);
            landableCells = new BoolGrid(map);

            corruption = new BoolGrid(map);
            corruptionInt = new IntGrid(map);
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z);
        }

        public bool GetCellBool(int index)
        {
            return waterCells[index] || landableCells[index];
        }

        public Color GetCellExtraColor(int index)
        {
            if (corruption[index])
                return Color.magenta;
            if (landableCells[index])
                return Color.green;
            if (riverCells[index])
                return Color.cyan;
            if (waterCells[index])
                return Color.blue;
            return Color.clear;
        }

        public Color Color => Color.white;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref waterCells, "waterCells");
            Scribe_Deep.Look(ref riverCells, "riverCells");
            Scribe_Deep.Look(ref landableCells, "landableCells");
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
            LongEventHandler.QueueLongEvent(delegate ()
            {
                foreach (var waterCell in map.AllCells.Where(c => c.GetTerrain(map).IsWater))
                {
                    waterCells[waterCell] = true;
                }

                foreach (var riverCell in waterCells.ActiveCells.Where(c => c.GetTerrain(map).IsRiver))
                {
                    riverCells[riverCell] = true;
                }

                var activeCells = riverCells.ActiveCells.ToList();
                foreach (var landableCell in map.AllCells.Where(c => !landableCells[c] && !riverCells[c] && activeCells.All(r => c.DistanceTo(r) >= 3.9f) && activeCells.Any(r => c.DistanceTo(r) <= 9.9f)))
                {
                    landableCells[landableCell] = true;
                }
            }, "SettingWaterData", false, null);
        }

        [TweakValue("MapComponent_ShowWater", 0f, 100f)]
        public static bool DrawBool = false;

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            //if(Find.TickManager.TicksGame % 100 == 0)
                //UpdateCorruption();
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (DrawBool && Find.CurrentMap == this.map)
            {
                drawer.RegenerateMesh();
                drawer.MarkForDraw();
                drawer.CellBoolDrawerUpdate();
            }
        }

        public void Notify_TibSpawned(TiberiumCrystal crystal)
        {
            if (!riverCells[crystal.Position]) return;
            corruption[crystal.Position] = true;
            dirtyCells.Add(crystal.Position);
        }

        private void UpdateCorruption()
        {
            if (!dirtyCells.Any()) return;

            for(int i = dirtyCells.Count - 1; i >= 0; i--)
            {
                IntVec3 pos = dirtyCells[i];
                CorruptCell(pos);
                foreach (var intVec3 in SelectFlowCellsFrom(pos))
                {
                    if(dirtyCells.Contains(intVec3))continue;
                    dirtyCells.Add(intVec3);
                }
                dirtyCells.RemoveAt(i);
            }
        }

        private IEnumerable<IntVec3> SelectFlowCellsFrom(IntVec3 pos)
        {
            WaterInfo waterInfo = map.waterInfo;
            Vector3 movementAt = waterInfo.GetWaterMovement(pos.ToVector3());

            foreach (IntVec3 c in pos.CellsAdjacent8Way())
            {
                if (!riverCells[c]) continue;

                var posDiff = (pos - c).ToVector3();
                var xDiff = movementAt.x - posDiff.x;
                var yDiff = movementAt.y - posDiff.y;

                Log.Message("Trying: " + movementAt + " - " + posDiff + " |x: " + xDiff + " |y: " + yDiff);
                if (xDiff >= 0 && yDiff >= 0)
                    yield return c;
            }
        }

        private void CorruptCell(IntVec3 c)
        {
            corruption[c] = true;
            map.terrainGrid.SetTerrain(c, DefDatabase<TerrainDef>.GetNamed("TiberiumShallowWater"));
        }
    }
}
