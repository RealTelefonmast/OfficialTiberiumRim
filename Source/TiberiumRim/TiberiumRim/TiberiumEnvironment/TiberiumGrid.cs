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
        public BoolGrid tiberiumBools;
        public BoolGrid growBools;
        public CellBoolDrawer drawer;
        public TiberiumCrystal[] TiberiumCrystals;

        private bool dirtyGrid = false;
        private List<IntVec3> dirtyCells = new List<IntVec3>();

        public TiberiumGrid(Map map)
        {
            this.map = map;
            tiberiumBools = new BoolGrid(map);
            growBools = new BoolGrid(map);
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.35f);
            TiberiumCrystals = new TiberiumCrystal[map.cellIndices.NumGridCells];
        }

        public bool GetCellBool(int index)
        {
            return tiberiumBools[index];
        }

        public Color Color
        {
            get
            {
                return Color.white;
            }
        }

        public void UpdateDirties()
        {
            if (dirtyGrid)
            {
                for (int i = dirtyCells.Count -1; i > 0; i--)
                {
                    IntVec3 cell = dirtyCells[i];
                    SetGrowBool(cell);
                    dirtyCells.Remove(cell);
                }
                dirtyGrid = false;
            }
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
            if (growBools[index])
            {
                return Color.green;
            }
            return Color.red;
        }

        public void Set(IntVec3 c, bool value, TiberiumCrystal crystal)
        {
            TiberiumCrystals[map.cellIndices.CellToIndex(c)] = crystal;
            tiberiumBools.Set(c, value);
            SetGrowBool(c);
            foreach(var v in c.CellsAdjacent8Way())
            {
                if (v.InBounds(map) && !dirtyCells.Contains(v))
                {
                    dirtyCells.Add(v);
                }
            }
            dirtyGrid = true;
        }

        private void SetGrowBool(IntVec3 c)
        {
            bool flag = c.CellsAdjacent8Way().Where(p => p.InBounds(map)).All(p => tiberiumBools[p]);
            growBools.Set(c, !flag);
        }
    }
}
