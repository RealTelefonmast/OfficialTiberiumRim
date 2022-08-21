using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumWaterInfo : MapInformation, ICellBoolGiver
    {
        public BoolGrid allWaterCells;

        public BoolGrid lakeCells;
        public BoolGrid riverCells;

        public BoolGrid corruption;

        public IntGrid corruptionInt;

        //
        public BoolGrid landableCells;

        private CellBoolDrawer drawer;

        private List<IntVec3> dirtyCells = new List<IntVec3>();

        public Color Color => Color.white;

        [TweakValue("MapComponent_ShowWater", 0f, 100f)]
        public static bool DrawBool = false;

        public TiberiumWaterInfo(Map map) : base(map)
        {
            allWaterCells = new BoolGrid(map);

            lakeCells = new BoolGrid(map);
            riverCells = new BoolGrid(map);

            landableCells = new BoolGrid(map);

            corruption = new BoolGrid(map);
            corruptionInt = new IntGrid(map);
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref allWaterCells, "waterCells");
            Scribe_Deep.Look(ref lakeCells, "lakeCells");
            Scribe_Deep.Look(ref riverCells, "riverCells");
            Scribe_Deep.Look(ref landableCells, "landableCells");
        }

        public bool GetCellBool(int index)
        {
            return allWaterCells[index] || landableCells[index];
        }

        public Color GetCellExtraColor(int index)
        {
            if (corruption[index])
                return Color.magenta;
            if (landableCells[index])
                return Color.green;
            if (riverCells[index])
                return Color.cyan;
            if (lakeCells[index])
                return Color.blue;
            if (allWaterCells[index])
                return Color.magenta;
            return Color.clear;
        }

        public override void InfoInit(bool initAfterReload = false)
        {
            base.InfoInit(initAfterReload);
            LongEventHandler.QueueLongEvent(delegate ()
            {
                foreach (var cell in map.AllCells)
                {
                    var terr = cell.GetTerrain(map);
                    if (terr.IsWater)
                    {
                        allWaterCells[cell] = true;
                        if (terr.IsRiver)
                            riverCells[cell] = true;
                        else if(!terr.defName.Contains("Ocean"))
                        {
                            lakeCells[cell] = true;
                        }
                    }
                }

                var activeCells = riverCells.ActiveCells.ToList();
                foreach (var landableCell in map.AllCells.Where(c => !landableCells[c] && !riverCells[c] && activeCells.All(r => c.DistanceTo(r) >= 3.9f) && activeCells.Any(r => c.DistanceTo(r) <= 9.9f)))
                {
                    landableCells[landableCell] = true;
                }
            }, "SettingWaterData", false, null);
        }

        public override void Tick()
        {
        }

        public bool IsLake(IntVec3 cell)
        {
            return lakeCells[cell];
        }

        public override void Update()
        {
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
