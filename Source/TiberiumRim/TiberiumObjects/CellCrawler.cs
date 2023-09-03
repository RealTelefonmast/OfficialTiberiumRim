using System;
using System.Collections.Generic;
using Verse;

namespace TR
{
    public class CellCrawler : IExposable
    {
        private Map map;
        private IntVec3 origin;

        private int numCellsLeft = -1;

        private Predicate<IntVec3> Pattern;

        private List<IntVec3> Points = new List<IntVec3>();

        private void GeneratePoints()
        {

        }

        public CellCrawler(Map map)
        {

        }

        public void ExposeData()
        {

        }

        public void DrawPoints()
        {
            if (Points == null) return;
            for (int i = 0; i < Points.Count; i++)
            {
                GenDraw.DrawCircleOutline(Points[i].ToVector3Shifted(), 1, SimpleColor.Red);
            }
        }
    }
}
