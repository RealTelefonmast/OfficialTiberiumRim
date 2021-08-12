using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AreaGrid : IExposable, ICellBoolGiver
    {
        private Map map;
        private BoolGrid grid;
        private CellBoolDrawer drawer;

        public AreaGrid() { }

        public AreaGrid(Map map)
        {
            this.map = map;
            this.grid = new BoolGrid(map);
        }

        public void ExposeData()
        {
            Scribe_Deep.Look<BoolGrid>(ref this.grid, "innerGrid", new object[0]);
        }

        public bool GetCellBool(int index)
        {
            return grid[index];
        }

        public Color GetCellExtraColor(int index)
        {
            return Color;
        }

        public IEnumerable<IntVec3> Cells => grid.ActiveCells;

        public int CellCount => grid.TrueCount;

        public Color Color => Color.cyan;

        private CellBoolDrawer Drawer => drawer ??= new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.33f);

        public bool this[int index]
        {
            get => grid[index];
            set => Set(map.cellIndices.IndexToCell(index), value);
        }

        public bool this[IntVec3 c]
        {
            get => grid[map.cellIndices.CellToIndex(c)];
            set => Set(c, value);
        }

        public virtual void Set(IntVec3 c, bool val)
        {
            int index = map.cellIndices.CellToIndex(c);
            if (grid[index] == val)
                return;

            grid[index] = val;
            MarkDirty(c);
        }

        private void MarkDirty(IntVec3 c)
        {
            Drawer.SetDirty();
        }

        public void MarkForDraw()
        {
            if (map == Find.CurrentMap)
            {
                Drawer.MarkForDraw();
            }
        }

        public void AreaUpdate()
        {
            Drawer.CellBoolDrawerUpdate();
        }
    }
}
