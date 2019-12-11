using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class SuppressionGrid : ICellBoolGiver
    {
        public Map map;
        public BoolGrid suppressionBools;
        public CellBoolDrawer drawer;

        public SuppressionGrid(Map map)
        {
            this.map = map;
            suppressionBools = new BoolGrid(map);
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.4f);
        }

        public void Set(IntVec3 c, bool value)
        {
            suppressionBools.Set(c, value);
        }

        public Color Color => Color.white;

        public Color GetCellExtraColor(int index)
        {
            return suppressionBools[index] ? Color.red : Color.gray;
        }

        public bool GetCellBool(int index)
        {
            return suppressionBools[index];
        }
    }
}
