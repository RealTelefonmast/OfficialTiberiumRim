using System;
using Verse;

namespace TiberiumRim
{
    public class ExposableIntGrid : IExposable
    {
        private int[] grid;
        private int mapCells;
        private int mapSizeX;
        private int mapSizeZ;

        private byte[] savedBytes;

        public ExposableIntGrid(Map map)
        {
            mapSizeX = map.Size.x;
            mapSizeZ = map.Size.z;
            mapCells = mapSizeX * mapSizeZ;
            grid = new int[mapCells];
            savedBytes = new byte[mapCells * 4];
        }

        public int this[IntVec3 c]
        {
            get => grid[CellIndicesUtility.CellToIndex(c, this.mapSizeX)];
            set => grid[CellIndicesUtility.CellToIndex(c, this.mapSizeX)] = value;
        }

        public int this[int index]
        {
            get => grid[index];
            set => grid[index] = value;
        }

        public int this[int x, int z]
        {
            get => grid[CellIndicesUtility.CellToIndex(x, z, this.mapSizeX)];
            set => grid[CellIndicesUtility.CellToIndex(x, z, this.mapSizeX)] = value;
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Buffer.BlockCopy(grid, 0, savedBytes, 0, mapCells * 4);
            }

            Scribe_Values.Look(ref mapCells, "mapCells");
            DataExposeUtility.ByteArray(ref savedBytes, "savedBytes");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Buffer.BlockCopy(savedBytes, 0, grid, 0, mapCells);
            }
        }

        public void Clear(int value = 0)
        {
            if (value == 0)
            {
                Array.Clear(grid, 0, grid.Length);
                return;
            }
            for (int i = 0; i < grid.Length; i++)
            {
                grid[i] = value;
            }
        }
	}
}
