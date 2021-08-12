using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class QuadTree
    {
        public CellRect boundary;
        public int capacity;
        public List<IntVec3> points;
        public QuadTree topLeft, topRight, bottomLeft, bottomRight;

        private bool divided;
        
        public QuadTree(CellRect boundary, int capacity)
        {
            this.boundary = boundary;
            Log.Message("Making quad tree with: " + boundary.ToString());
            this.capacity = capacity;
            points = new List<IntVec3>(); //new IntVec3[capacity];
        }

        public void Insert(IntVec3 point)
        {
            Log.Message("Inserting: " + point);
            if (!boundary.Contains(point))
                return;

            if (points.Count < capacity)
            {
                points.Add(point);
            }
            else 
            {
                if (!divided)
                {
                    SubDivide();
                    divided = true;
                }
                topLeft.Insert(point);
                topRight.Insert(point);
                bottomLeft.Insert(point);
                bottomRight.Insert(point);
            }
        }

        public void SubDivide()
        {
            int x = boundary.minX;
            int z = boundary.minZ;
            int w = boundary.Width;
            int h = boundary.Height;
            int adder = 0; // w % 2 == 0 ? 0 : 1;
            Log.Message("TopLeft Quadt");
            CellRect tLeft = new CellRect(x, z + ((h / 2) + adder), w / 2, h/2);
            topLeft = new QuadTree(tLeft, capacity);
            Log.Message("TopRight Quadt");
            CellRect tRight = new CellRect(x + (w / 2) + adder, z + ((h / 2) + adder), w / 2, h / 2);
            topRight = new QuadTree(tRight, capacity);
            Log.Message("BottomLeft Quadt");
            CellRect bLeft = new CellRect(x, z, w / 2, h / 2);
            bottomLeft = new QuadTree(bLeft, capacity);
            Log.Message("BottomRight Quadt");
            CellRect bRight = new CellRect(x + ((w / 2) + adder), z, w / 2, h / 2);
            bottomRight = new QuadTree(bRight, capacity);
        }

        public void Draw()
        {
            GenDraw.DrawFieldEdges(boundary.Cells.ToList(), Color.blue);
            GenDraw.DrawFieldEdges(points.ToList(), Color.red);
            if (divided)
            {
                topLeft.Draw();
                topRight.Draw();
                bottomLeft.Draw();
                bottomRight.Draw();
            }
        }
    }
}
