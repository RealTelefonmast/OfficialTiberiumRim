using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumHediffGrid : IExposable
    {
        private double[] radiationGrid;
        private double[] infectionGrid;

        private byte[] radiationBytes;
        private byte[] infectionBytes;
        private Map map;
        private int mapCells;

        private BoolGrid affectedCells;

        public TiberiumHediffGrid(Map map)
        {
            this.map = map;
            affectedCells = new BoolGrid(map);
            mapCells = map.cellIndices.NumGridCells;
            radiationGrid = new double[mapCells];
            infectionGrid = new double[mapCells];
            radiationBytes = new byte[mapCells * 4];
            infectionBytes = new byte[mapCells * 4];
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Buffer.BlockCopy(radiationGrid, 0, radiationBytes, 0, mapCells * 4);
                Buffer.BlockCopy(infectionGrid, 0, infectionBytes, 0, mapCells * 4);
            }

            Scribe_Deep.Look(ref affectedCells, "affectedHediffCells");
            Scribe_Values.Look(ref mapCells, "mapCells");
            DataExposeUtility.ByteArray(ref radiationBytes, "radiationBytes");
            DataExposeUtility.ByteArray(ref infectionBytes, "infectionBytes");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Buffer.BlockCopy(radiationBytes, 0, radiationGrid, 0, mapCells);
                Buffer.BlockCopy(infectionBytes, 0, infectionGrid, 0, mapCells);
            }
        }

        public void UpdateGrid()
        {

        }

        public void DrawValues()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                for (int i = 0; i < mapCells; i++)
                {
                    if (radiationGrid[i] <= 0) continue;
                    Vector3 v = GenMapUI.LabelDrawPosFor(map.cellIndices.IndexToCell(i));
                    Vector3 v2 = v + new Vector3(0, 15f, 0);
                    GenMapUI.DrawThingLabel(v2, radiationGrid[i].ToString(),
                        Color.Lerp(Color.green, Color.red, (float)radiationGrid[i]));
                    GenMapUI.DrawThingLabel(v, infectionGrid[i].ToString(),
                        Color.Lerp(Color.white, Color.cyan, (float)infectionGrid[i]));
                    //GenMapUI.DrawThingLabel(v2, Container.StoredPercent.ToString() + "p", Color.yellow);
                }
            }
        }

        public bool IsAffected(IntVec3 cell)
        {
            return affectedCells[cell];
        }

        public float RadiationAt(IntVec3 cell)
        {
            return (float)radiationGrid[map.cellIndices.CellToIndex(cell)];
        }

        public float InfectivityAt(IntVec3 cell)
        {
            return (float)infectionGrid[map.cellIndices.CellToIndex(cell)];
        }

        private void SetAffected(IntVec3 cell)
        {
            var index = map.cellIndices.CellToIndex(cell);
            if (radiationGrid[index] > 0 || infectionGrid[index] > 0)
                affectedCells[index] = true;
            if(radiationGrid[index] <= 0 && infectionGrid[index] <= 0)
                affectedCells[index] = false;
        }

        public void SetInfection(IntVec3 pos, double infVal)
        {
            var index = map.cellIndices.CellToIndex(pos);
            infectionGrid[index] += infVal; //Math.Round(infVal, 5); //infVal; //Math.Round(infectionGrid[map.cellIndices.CellToIndex(pos)] + infVal, 2);
            if (infectionGrid[index] <= 0.001)
                infectionGrid[index] = 0;
            SetAffected(pos);
        }

        public void SetRadiation(IntVec3 pos, double radVal)
        {
            var index = map.cellIndices.CellToIndex(pos);
            radiationGrid[index] += radVal; //Math.Round(radVal, 5); //Math.Round(radiationGrid[map.cellIndices.CellToIndex(pos)] + radVal, 2);
            if (radiationGrid[index] <= 0.001)
                radiationGrid[index] = 0;
            SetAffected(pos);
        }

    }
}
