using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PollutionGrid : ICellBoolGiver, IExposable
    {
        public ushort[] Grid;
        public CellBoolDrawer Drawer;
        public Map map;

        public PollutionGrid()
        {

        }

        public void ExposeData()
        {

        }

        public Color Color
        {
            get
            {
                return new Color();
            }
        }

        public Color GetCellExtraColor(int index)
        {
            return new Color();
        }

        public bool GetCellBool(int index)
        {
            return true;
        }
    }
}
