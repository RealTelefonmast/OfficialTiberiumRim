using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using  RimWorld;
using  Verse;
using  UnityEngine;
using Verse.Noise;

namespace TiberiumRim
{
    public class MapComponent_Terrain : MapComponent
    {
        public Color[] ColorOffsetGrid;

        public MapComponent_Terrain(Map map) : base(map)
        {
            ColorOffsetGrid = new Color[map.cellIndices.NumGridCells];
        }

        private void GenerateNoise()
        {

        }

        public void Set(IntVec3 c, Color value)
        {
            ColorOffsetGrid[map.cellIndices.CellToIndex(c)] = value;
            
        }
    }

    public class ColorOffsetGrid : ICellBoolGiver
    {
        public Map map;
        public Color[] colorGrid;

        public ColorOffsetGrid(Map map)
        {
            this.map = map;
            colorGrid = new Color[map.cellIndices.NumGridCells];
        }

        public bool GetCellBool(int index)
        {
            return true;
        }

        public Color GetCellExtraColor(int index)
        {
            return Color.cyan;
        }

        public Color Color
        {
            get
            {
                return Color.cyan;
            }
        }
    }
}
