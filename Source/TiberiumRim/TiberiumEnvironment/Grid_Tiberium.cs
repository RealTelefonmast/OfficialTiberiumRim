using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    /* Tiberium Grid, keeps track of all cells related to Tiberium
     * Determines growth patterns
     */

    public class TiberiumGrid : ICellBoolGiver
    {
        public Map map;
        
        public BoolGrid TiberiumBoolGrid;
        public TiberiumCrystal[] TiberiumCrystals;

        public BoolGrid GrowToGrid;
        public BoolGrid GrowFromGrid;

        public BoolGrid AffectedCells;

        public CellBoolDrawer Drawer;
        public BoolGrid[] fieldColorGrids;

        public Color Color => Color.white;

        public TiberiumGrid() { }

        public TiberiumGrid(Map map)
        {
            //
            this.map = map;
            TiberiumBoolGrid = new BoolGrid(map);
            GrowToGrid = new BoolGrid(map);
            GrowFromGrid = new BoolGrid(map);
            AffectedCells = new BoolGrid(map);

            Drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);
            fieldColorGrids = new BoolGrid[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };

            TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
        }

        public void ExposeData()
        {
        }

        public bool GetCellBool(int index)
        {
            return true;
        }

        public Color GetCellExtraColor(int index)
        {
            Color color = Color.clear;
            if (GrowToGrid[index])
            {
                color = Color.cyan;
                return color;
            }
            if (AffectedCells[index])
            {
                color = Color.magenta;
                return color;
            }
            if (GrowFromGrid[index])
            {
                color = Color.green;
                return color;
            }
            return color;
        }

        private int Index(IntVec3 c)
        {
            return map.cellIndices.CellToIndex(c);
        }

        //
        private List<IntVec3> Adjacent(IntVec3 origin, bool andInside = false, Predicate<IntVec3> predicate = null)
        {
            int count = andInside ? 9 : 8;
            List<IntVec3> cells = new List<IntVec3>();
            for (int i = 0; i < count; i++)
            {
                var cell = origin + GenAdj.AdjacentCellsAndInside[i];
                if (cell.InBounds(map) && (predicate == null || predicate(cell)))
                    cells.Add(cell);
            }
            return cells;
        }

        //
        public void SetCrystal(TiberiumCrystal crystal)
        {
            TiberiumBoolGrid[crystal.Position] = true;
            TiberiumCrystals[Index(crystal.Position)] = crystal;

            var adjacent = Adjacent(crystal.Position, true);
            foreach(var adj in adjacent)
            {
                var adjacent2 = Adjacent(adj);
                SetGrowFrom(adj, adjacent2);
                SetAffected(adj, adjacent2);
            }
            SetGrowTo(crystal.Position, crystal);
        }

        public void ResetCrystal(IntVec3 c)
        {
            TiberiumBoolGrid[c] = false;
            TiberiumCrystals[Index(c)] = null;
            GrowFromGrid[c] = false;
            RemoveGrowTo(c, Adjacent(c));

            var adjacent = Adjacent(c);
            foreach (var adj in adjacent)
            {
                var adjacent2 = Adjacent(adj);
                SetGrowFrom(adj, adjacent2);
                SetAffected(adj, adjacent2);
                UpdateGrowTo(adj, adjacent2);
            }

        }

        private void SetGrowFrom(IntVec3 c, List<IntVec3> adjacent)
        {
            GrowFromGrid[c] = TiberiumBoolGrid[c] && adjacent.Any(a => !TiberiumBoolGrid[a] && !a.HasTibFlora(map));
        }

        private void SetAffected(IntVec3 c, List<IntVec3> adjacent)
        {
            AffectedCells[c] = !TiberiumBoolGrid[c] && adjacent.Any(v => TiberiumBoolGrid[v]);
        }

        private void RemoveGrowTo(IntVec3 c, List<IntVec3> adjacent)
        {
            //-> Removal
            GrowToGrid[c] = adjacent.Any(c => TiberiumBoolGrid[c]);
            if (adjacent.Any(c => TiberiumBoolGrid[c]))
            {
                GrowToGrid[c] = true;
                return;
            }
        }

        private void SetGrowTo(IntVec3 c, TiberiumCrystal crystal)
        {
            //-> Addition
            if (crystal == null) return;
            if (TiberiumBoolGrid[c])
            {
                GrowToGrid[c] = false;
            }
            var cells = Adjacent(c, false, x => !TiberiumBoolGrid[x] && !x.HasTibFlora(map));
            if (cells.NullOrEmpty()) return;
            if(Rand.ChanceSeeded(crystal.def.tiberium.rootNodeChance, crystal.GetHashCode()))
            {
                cells.Do(x => GrowToGrid[x] = true);
                return;
            }
            var cell = cells[Rand.RangeSeeded(0, cells.Count - 1, crystal.GetHashCode())];
            GrowToGrid[cell] = true;
        }

        private void UpdateGrowTo(IntVec3 c, List<IntVec3> adjacent)
        {
            var crystal = TiberiumCrystals[Index(c)];
            var hasNeighbor = adjacent.Any(c => TiberiumBoolGrid[c]);
            if(GrowToGrid[c] && hasNeighbor)
            {
                return;
            }
            if (crystal != null)
            {
                SetGrowTo(c, crystal);
                return;
            }
            GrowToGrid[c] = false;
        }

    }


    public class TiberiumGrid2 : ICellBoolGiver, IExposable
    {
        private static int Seed = 468155708;
        private bool dirtyGrid = false;
        private BoolGrid DirtyGrid;


        public Map map;

        //Tiberium Exists At These Cells
        public BoolGrid tiberiumGrid; 

        //Tiberium May Grow From These Cells
        public BoolGrid growFromGrid;
        //Tiberium May Grow To These Cells
        public BoolGrid growToGrid;

        //Tiberium Affects These Cells
        public BoolGrid affectedCells;

        public BoolGrid[] fieldColorGrids;

        public TiberiumCrystal[] TiberiumCrystals;
        //TODO: Use int counter on cells to see how many crystals require it, for easier removal of spread ranged pos's
        //public int[] WantedCount;

        public CellBoolDrawer drawer;

        public TiberiumGrid2(){}

        public TiberiumGrid2(Map map)
        {
            this.map = map;
            DirtyGrid = new BoolGrid(map);
            
            tiberiumGrid = new BoolGrid(map);
            growFromGrid  = new BoolGrid(map);
            growToGrid    = new BoolGrid(map);
            affectedCells = new BoolGrid(map);

            fieldColorGrids = new BoolGrid[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };

            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);

            TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
            //WantedCount = new int[map.cellIndices.NumGridCells];
        }

        public void ExposeData()
        {
            //Scribe_Deep.Look(ref growToGrid, "growToGrid");
        }

        private int Index(IntVec3 c)
        {
            return map.cellIndices.CellToIndex(c);
        }

        //CellBoolGiver
        public bool GetCellBool(int index)
        {
            return true; //tiberiumGrid[index] || affectedCells[index];
        }

        public Color Color => Color.white;

        public Color GetCellExtraColor(int index)
        {
            Color color = Color.clear;
            if (DirtyGrid[index])
            {
                color = Color.yellow;
                return color;
            }
            if (growToGrid[index])
            {
                color = Color.cyan;
                return color;
            }
            if (affectedCells[index])
            {
                color = Color.magenta;
                return color;
            }
            if (growFromGrid[index])
            {
                color = Color.green;
                return color;
            }
            return color;
        }

        public void SetFieldColor(IntVec3 c, bool value, TiberiumValueType type)
        {
            switch (type)
            {
                case TiberiumValueType.Green:
                    fieldColorGrids[0][c] = value;
                    break;
                case TiberiumValueType.Blue:
                    fieldColorGrids[1][c] = value;
                    break;
                case TiberiumValueType.Red:
                    fieldColorGrids[2][c] = value;
                    break;
                default:
                    return;
            }
        }

        //BLANK SLATE
        private List<IntVec3> Adjacent(IntVec3 origin, bool andInside = false, Predicate<IntVec3> predicate = null)
        {
            int count = andInside ? 9 : 8;
            List<IntVec3> cells = new List<IntVec3>();
            for(int i = 0; i < count; i++)
            {
                var cell = origin + GenAdj.AdjacentCellsAndInside[i];
                if (cell.InBounds(map) && (predicate == null || predicate(cell)))
                    cells.Add(cell);
            }
            return cells;
        }

        public void RESETALL(List<TiberiumCrystal> crystals)
        {
            tiberiumGrid = new BoolGrid(map);
            growFromGrid = new BoolGrid(map);
            growToGrid = new BoolGrid(map);
            affectedCells = new BoolGrid(map);

            fieldColorGrids = new BoolGrid[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };

            TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];

            foreach(var crystal in crystals)
            {
                SetCrystal(crystal);
            }
        }

        public void RESETALLREVERSE(List<TiberiumCrystal> crystals)
        {
            tiberiumGrid = new BoolGrid(map);
            growFromGrid = new BoolGrid(map);
            growToGrid = new BoolGrid(map);
            affectedCells = new BoolGrid(map);

            fieldColorGrids = new BoolGrid[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };

            TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];

            for(int i = crystals.Count -1; i > 0; i--)
            {
                SetCrystal(crystals[i]);
            }
        }

        public void Tick()
        {
            if (!dirtyGrid) return;
            foreach(var crystal in TiberiumCrystals)
            {
                if (crystal == null) continue;
                if (DirtyGrid[crystal.Position])
                {
                    ReEvaluate(crystal.Position);
                }
                //dirtyCells.Remove(dirtyCell);
                //DirtyGrid[dirtyCell] = false;
            }
            DirtyGrid.Clear();
            dirtyGrid = false;
        }

        public void ReEvaluate(IntVec3 c)
        {
            var adjacent = Adjacent(c);
            //Can it grow from this position?
            SetGrowFrom(c, adjacent);
            //Where can it grow to?
            SetGrowToFor(c, adjacent, TiberiumCrystals[Index(c)]);
            //Does it affect any new cells?
            SetAffected(c, adjacent);

            //Update Nearby Cells
            foreach (var adj in adjacent)
            {
                Evaluate(adj);
            }
        }

        private void Evaluate(IntVec3 c)
        {
            var adjacent = Adjacent(c);

            SetGrowFrom(c, adjacent);
            SetGrowToFor(c, adjacent, TiberiumCrystals[Index(c)]);
            SetAffected(c, adjacent);
        }

        public void SetCrystal(TiberiumCrystal crystal)
        {
            //Set Grids Direct
            TiberiumCrystals[Index(crystal.Position)] = crystal;
            tiberiumGrid.Set(crystal.Position, true);

            MarkDirty(crystal.Position);

            //ReEvaluate(crystal.Position);
        }

        public void ResetCrystal(IntVec3 c)
        {
            //Reset Grids Direct
            TiberiumCrystals[Index(c)] = null;
            tiberiumGrid.Set(c, false);

            var adjacent = Adjacent(c);
            //Reset for new evaluation
            //adjacent.Do(x => growFromGrid[x] = false);
           // adjacent.Do(x => growToGrid[x] = false);
           // adjacent.Do(x => affectedCells[x] = false);

            //Update Nearby Cells
            foreach (var adj in adjacent)
            {
                MarkDirty(adj);
                //ReEvaluate(adj);
            }
        }

        private void MarkDirty(IntVec3 c)
        {
            DirtyGrid.Set(c, true);
            dirtyGrid = true;
            //DirtyGrid[c] = true;
        }

        private List<IntVec3> AdjacentGrowToCells(IntVec3 origin, out bool nearTib)
        {
            List<IntVec3> cells = new List<IntVec3>();
            nearTib = false;
            for(int i = 0; i < 8; i++)
            {
                var cell = origin + GenAdj.AdjacentCells[i];
                if (!cell.InBounds(map)) continue;
                if (tiberiumGrid[cell])
                {
                    nearTib = true;
                    continue;
                }
                if (cell.HasTibFlora(map)) continue;
                cells.Add(cell);
            }
            return cells;
        }

        private void SetGrowToFor(IntVec3 c, List<IntVec3> adjacent, TiberiumCrystal crystal)
        {
            Log.Message("Setting GrowTo For " + crystal);
            var cells = AdjacentGrowToCells(c, out bool nearTib);
            if (crystal == null)
            {
                if (!nearTib)
                    growToGrid[c] = false;
                return;
            }
            if (growFromGrid[c])
            {
                growToGrid[c] = false;
            }
            if (cells.NullOrEmpty()) return;
            var cell = cells[Rand.RangeSeeded(0, cells.Count - 1, crystal.GetHashCode())];
            growToGrid[cell] = true;
        }

        private void SetGrowFrom(IntVec3 c, List<IntVec3> adjacent)
        {
            growFromGrid[c] = tiberiumGrid[c] && adjacent.Any(a => !tiberiumGrid[a] && !a.HasTibFlora(map));
        }

        private void SetAffected(IntVec3 c, List<IntVec3> adjacent)
        {
            affectedCells[c] = !tiberiumGrid[c] && adjacent.Any(v => tiberiumGrid[v]);
        }

        //##################################################################################//
        //##################################################################################//

        /*
        public void UpdateAll(IntVec3 c, TiberiumCrystal crystal)
        {
            var cells = c.CellsAdjacent8Way(true).Where(c => c.InBounds(map)).ToList();
            UpdateEffects(c, crystal, cells);
            UpdateGrow(c, crystal, cells);
        }

        public void UpdateEffects(IntVec3 c, TiberiumCrystal crystal, List<IntVec3> cells = null)
        {
            cells ??= c.CellsAdjacent8Way(true).Where(c => c.InBounds(map)).ToList();
            foreach (var cell in cells)
            {
                SetAffectedBool(cell);
            }
        }

        public void UpdateGrow(IntVec3 c, TiberiumCrystal crystal, List<IntVec3> cells = null)
        { 
            cells ??= c.CellsAdjacent8Way(true).Where(c => c.InBounds(map)).ToList();
            UpdateGrowTo(cells, crystal);
            foreach (var cell in cells)
            {
                SetGrowFromBool(cell);
                SetGrowToGeneric(cell);
            }
        }

        public void Update(IntVec3 c, TiberiumCrystal crystal)
        {
            var cells = c.CellsAdjacent8Way(true).Where(c => c.InBounds(map)).ToList();
            UpdateGrowTo(cells, crystal);
            Update(cells);
        }

        private void Update(List<IntVec3> cells)
        {
            foreach (var cell in cells)
            {
                SetAffectedBool(cell);
                SetGrowFromBool(cell);
                SetGrowToGeneric(cell);
            }
        }

        private void UpdateGrowTo(List<IntVec3> cells, TiberiumCrystal crystal)
        {
            var potentialGrowTo = crystal != null
                ? cells.Where(t => !(t.HasTiberium(map) || t.HasTibFlora(map))).ToList()
                : cells;//cells.Where(t => growToGrid[t]).ToList();

            if (!potentialGrowTo.Any()) return;
            if (Rand.ChanceSeeded(crystal?.def.tiberium.rootNodeChance ?? 0f, crystal?.GetHashCode() ?? Seed))
                potentialGrowTo.ForEach(SetGrowToSpecific);
            else
                SetGrowToSpecific(WeightedGrowToCell(crystal, potentialGrowTo));
        }

        private IntVec3 WeightedGrowToCell(TiberiumCrystal origin, List<IntVec3> potentialCells)
        {

            return potentialCells[Rand.RangeSeeded(0, potentialCells.Count-1, origin.GetHashCode())];
            // Func<IntVec3, float> action = delegate(IntVec3 cell)
            // {
            //     return Mathf.Lerp(1,0, origin.Position.DistanceTo(cell)-1);
            // };
            // return potentialCells.RandomElementByWeight(action);
        }

        public Color GetCellExtraColor(int index)
        {
            Color color = Color.clear;
            if (growToGrid[index])
            {
                color += Color.cyan;
            }
            if (affectedCells[index])
            {
                color += Color.magenta;
            }
            if (growFromGrid[index])
            {
                color += Color.green;
            }
            return color;
        }

        //TODO: Split dirty calls into tiberium stages for less redundancy
        public void MarkDirty(IntVec3 c, TiberiumCrystal from)
        {
            if (!tiberiumGrid[c] && (from?.Spawned ?? false))
            {
                SetCrystal(c, true, from);
                return;
            }
            Update(c, from);
        }

        public void SetInit() {}

        public void RemoveCrystal(IntVec3 c)
        {
            TiberiumCrystals[Index(c)] = null;
            tiberiumGrid[c] = false;
            growFromGrid[c] = false;
            growToGrid[c] = false;
            affectedCells[c] = false;

            ResetHealthAffects(c);
            //Remove all and mark dirty
            var cells = c.CellsAdjacent8Way(false);
            foreach (var cell in cells)
            {
                tiberiumGrid[cell] = false;
                growFromGrid[cell] = false;
                growToGrid[cell] = false;
                affectedCells[cell] =false;
                MarkDirty(cell, TiberiumCrystals[Index(cell)]);
            }
        }

        public void SetCrystal(IntVec3 c, bool value, TiberiumCrystal crystal)
        {
            TiberiumCrystals[Index(c)] = crystal;
            tiberiumGrid.Set(c, value);

            SetHealthAffects(c, crystal);
            Update(c, crystal);
        }

        private void SetGrowFromBool(IntVec3 c)
        {
            TiberiumCrystal crystal = TiberiumCrystals[Index(c)];
            if (crystal == null)
            {
                growFromGrid[c] = false;
                return;
            }
            growFromGrid[c] = !c.CellsAdjacent8Way().All(v => v.InBounds(map) && (!growToGrid[v] || tiberiumGrid[v] || v.HasTibFlora(map)));
        }

        private void SetGrowToSpecific(IntVec3 c)
        {
            growToGrid[c] = c.CellsAdjacent8Way().Any(t => t.InBounds(map) && tiberiumGrid[t]);
        }

        private void SetGrowToGeneric(IntVec3 c)
        {
            growToGrid[c] = growToGrid[c];
            growToGrid[c] &= !tiberiumGrid[c];
        }

        private void SetAffectedBool(IntVec3 c)
        {
            affectedCells[c] = !tiberiumGrid[c] && c.CellsAdjacent8Way().Any(v => v.InBounds(map) && tiberiumGrid[v]);
        }

        public void Notify_ThingUpdated(Thing thing)
        {

        }

        public void Notify_TerrainUpdated(TerrainDef def, IntVec3 cell)
        {

        }
        */
    }
}