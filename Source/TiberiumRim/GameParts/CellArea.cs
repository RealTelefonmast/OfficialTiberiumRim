using System.Collections.Generic;
using TeleCore;
using Verse;

namespace TR
{
    public class CellArea : IExposable
    {
        private List<IntVec3> cells = new List<IntVec3>();
        private List<IntVec3> border = new List<IntVec3>();

        private bool withBorder;

        private bool[] cellBools;
        private int trueCountInt;
        private int mapSizeX;
        private int mapSizeZ;

        public int Count => cells.Count;

        public List<IntVec3> Cells => cells;
        public List<IntVec3> Border => border;

        public IntVec3 this[int i]
        {
            get => cells[i];
            set => cells[i] = value;
        }

        public CellArea(){}

        public CellArea(Map map, bool withBorder = false)
        {
            cells = new List<IntVec3>();
            mapSizeX = map.Size.x;
            mapSizeZ = map.Size.z;
            cellBools = new bool[mapSizeZ * mapSizeX];
            trueCountInt = 0;
            this.withBorder = withBorder;
        }

        public void Add(IntVec3 cell)
        {
            cells.Add(cell);
            cellBools[CellIndicesUtility.CellToIndex(cell, mapSizeX)] = true;
            trueCountInt++;
        }

        public void AddRange(List<IntVec3> newCells)
        {
            foreach (var cell in newCells)
            {
                cells.Add(cell);
                cellBools[CellIndicesUtility.CellToIndex(cell, mapSizeX)] = true;
                trueCountInt++;
            }
        }

        public bool Remove(IntVec3 cell)
        {
            if (cells.Remove(cell))
            {
                cellBools[CellIndicesUtility.CellToIndex(cell, mapSizeX)] = false;
                trueCountInt--;
                return true;
            }
            return false;
        }

        public bool Contains(IntVec3 cell, Map map)
        {
            return cellBools[cell.Index(map)];
        }

        public bool Empty()
        {
            return cells.NullOrEmpty();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref trueCountInt, "trueCount");
            Scribe_Values.Look(ref mapSizeX, "mapSizeX");
            Scribe_Values.Look(ref mapSizeZ, "mapSizeZ");

            DataExposeUtility.BoolArray(ref cellBools, mapSizeZ * mapSizeX, "cellBools");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                for (var index = 0; index < cellBools.Length; index++)
                {
                    var cell = cellBools[index];
                    if (cell)
                        cells.Add(CellIndicesUtility.IndexToCell(index, mapSizeX));
                }
            }
        }
    }
}
