using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    /* Tiberium Grid, keeps track of all cells related to Tiberium
     * Determines growth, inhibition, patterns
     */

    public class TiberiumGrid : ICellBoolGiver, IExposable
    {
        public Map map;

        //Tiberium Exists At These Cells
        public BoolGrid tiberiumGrid; 
        //Tiberium May Grow From These Cells
        public BoolGrid growFromGrid;
        //Tiberium May Grow To These Cells
        public BoolGrid growToGrid;

        //Tiberium Affects These Cells
        public BoolGrid affectedCells;
        //Tiberium Grows Indefinitely On These Cells
        public BoolGrid forceGrow;

        public BoolGrid[] fieldColorGrids;

        public TiberiumCrystal[] TiberiumCrystals;

        public CellBoolDrawer drawer;

        private bool dirtyGrid = false;
        private readonly List<IntVec3> dirtyCells = new List<IntVec3>();

        public TiberiumGrid(){}

        public TiberiumGrid(Map map)
        {
            this.map = map;
            tiberiumGrid = new BoolGrid(map);
            growFromGrid  = new BoolGrid(map);
            growToGrid    = new BoolGrid(map);
            affectedCells = new BoolGrid(map);
            forceGrow     = new BoolGrid(map);

            fieldColorGrids = new BoolGrid[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };

            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);
            TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
        }

        public void ExposeData()
        {
            Scribe_Deep.Look(ref forceGrow, "forceGrowGrid");
        }

        //CellBoolGiver
        public bool GetCellBool(int index)
        {
            return tiberiumGrid[index] || affectedCells[index];
        }

        public Color Color => Color.white;

        public void MarkDirty(IntVec3 c)
        {
            foreach (var v in c.CellsAdjacent8Way(true))
            {
                if (v.InBounds(map) && !dirtyCells.Contains(v))
                    dirtyCells.Add(v);
            }
            dirtyGrid = true;
            UpdateDirties();
        }

        public void Reset()
        { 
            foreach (var cell in map.AllCells)
            {
                    
            }
        }

        public void UpdateDirties()
        {
            if (!dirtyGrid) return;
            for (int i = dirtyCells.Count - 1; i > 0; i--)
            {
                IntVec3 cell = dirtyCells[i];
                SetAffectedBool(cell);
                SetGrowToBool(cell);
                SetGrowFromBool(cell);
                dirtyCells.Remove(cell);
            }
            dirtyGrid = false;
        }

        public Color GetCellExtraColor(int index)
        {
            if (dirtyCells.Contains(map.cellIndices.IndexToCell(index)))
            {
                return Color.yellow;
            }
            if (affectedCells[index])
            {
                return Color.magenta;
            }
            if (growFromGrid[index])
            {
                return Color.green;
            }
            return Color.red;
        }

        public void SetCrystal(IntVec3 c, bool value, TiberiumCrystal crystal)
        {
            TiberiumCrystals[map.cellIndices.CellToIndex(c)] = crystal;
            tiberiumGrid.Set(c, value);
            MarkDirty(c);
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

        private void SetGrowFromBool(IntVec3 c)
        {
            if (!tiberiumGrid[c])
            {
                growFromGrid[c] = false;
                return;
            }
            bool surrounded = c.CellsAdjacent8Way().All(v => v.InBounds(map) && (tiberiumGrid[v] || map.Tiberium().FloraInfo.HasFloraAt(v) || !growToGrid[v])); //!c.CellsAdjacent8Way().Any(v => v.InBounds(map) && (!tiberiumBools[v] && !map.Tiberium().FloraInfo.HasFloraAt(v)));
            growFromGrid[c] = !surrounded;
        }

        private void SetGrowToBool(IntVec3 c)
        {
            //Main Ruleset For Tiberium Spread
            growToGrid[c] = (forceGrow[c] || !GenTiberium.HasFloraAt(c, map)) && affectedCells[c];
        }

        private void SetAffectedBool(IntVec3 c)
        {
            affectedCells[c] = !tiberiumGrid[c] && c.CellsAdjacent8Way().Any(v => v.InBounds(map) && tiberiumGrid[v]);
            //if(affectedCells[c])
                //map.Tiberium().cell
        }
    }
}