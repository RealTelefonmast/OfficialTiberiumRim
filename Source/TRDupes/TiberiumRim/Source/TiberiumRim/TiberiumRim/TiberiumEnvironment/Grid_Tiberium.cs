using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TiberiumGrid : ICellBoolGiver
    {
        public Map map;

        //Tiberium
        public BoolGrid tiberiumBools; 
        public BoolGrid growBools;
        public BoolGrid affectedCells;

        public BoolGrid[] fieldColorGrids;

        public TiberiumCrystal[] TiberiumCrystals;

        //Flora
        public BoolGrid floraBools;


        public CellBoolDrawer   drawer;

        private bool dirtyGrid = false;
        private List<IntVec3> dirtyCells = new List<IntVec3>();

        public TiberiumGrid(Map map)
        {
            this.map = map;
            tiberiumBools = new BoolGrid(map);
            growBools = new BoolGrid(map);
            floraBools = new BoolGrid(map);
            affectedCells = new BoolGrid(map);

            fieldColorGrids = new BoolGrid[] { new BoolGrid(map), new BoolGrid(map), new BoolGrid(map) };

            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);
            TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
        }

        //CellBoolGiver
        public bool GetCellBool(int index)
        {
            return tiberiumBools[index] || floraBools[index] || affectedCells[index];
        }

        public Color Color
        {
            get
            {
                return Color.white;
            }
        }

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

        public void UpdateDirties()
        {
            if (!dirtyGrid) return;
            for (int i = dirtyCells.Count - 1; i > 0; i--)
            {
                IntVec3 cell = dirtyCells[i];
                SetGrowBool(cell);
                SetAffectedBool(cell);
                SetFloraBool(cell);
                dirtyCells.Remove(cell);
            }
            dirtyGrid = false;
        }

        public bool CanGrowFrom(IntVec3 cell)
        {          
            return growBools[cell];
        }

        public Color GetCellExtraColor(int index)
        {
            if (dirtyCells.Contains(map.cellIndices.IndexToCell(index)))
            {
                return Color.yellow;
            }
            if (floraBools[index])
            {
                return Color.cyan;
            }

            if (affectedCells[index])
            {
                return Color.magenta;
            }
            if (growBools[index])
            {
                return Color.green;
            }
            return Color.red;
        }

        public void SetPlant(IntVec3 c, bool value)
        {
            floraBools.Set(c, value);
            MarkDirty(c);
        }

        public void SetCrystal(IntVec3 c, bool value, TiberiumCrystal crystal)
        {
            TiberiumCrystals[map.cellIndices.CellToIndex(c)] = crystal;
            tiberiumBools.Set(c, value);
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

        private void SetFloraBool(IntVec3 c)
        {

        }

        private void SetGrowBool(IntVec3 c)
        {
            bool surrounded = !c.CellsAdjacent8Way().Any(c => c.InBounds(map) && (!tiberiumBools[c] && !floraBools[c]));
            growBools[c] = !surrounded;
        }

        private void SetAffectedBool(IntVec3 c)
        {
            affectedCells[c] = !tiberiumBools[c] && c.CellsAdjacent8Way().Any(v => v.InBounds(map) && tiberiumBools[v]);
        }
    }
}