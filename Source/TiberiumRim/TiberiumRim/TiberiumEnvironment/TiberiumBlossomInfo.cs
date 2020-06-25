using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumBlossomInfo : ICellBoolGiver, IExposable
    {
        public CellBoolDrawer drawer;
        public BoolGrid blossomGrid;
        private Map map;

        private float mapRadius;

        private  static BoolGrid positionGrid;
        private static List<IntVec3> positions;
        private static bool shouldSpawn = true;

        //Different Sizes Get Different Rules
        //1: Small; 2: Medium; 3: Large
        //private List<IntVec3>[] positionsBySize;
        //private IntVec3[][] availablePositionsBySize;
        //private float[] radiusBySize;

       public TiberiumBlossomInfo(Map map)
        {
            this.map = map;
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.4f);
            mapRadius = map.Center.DistanceToEdge(map);

            blossomGrid = new BoolGrid(map);

            positionGrid = new BoolGrid(map);
            positions = new List<IntVec3>();

            //positionsBySize = new List<IntVec3>[3] {new List<IntVec3>(), new List<IntVec3>(), new List<IntVec3>()};
            //radiusBySize = new float[3] {15f, 20f, 30f};

            GetPositions();
        }

        public void ExposeData()
        {
            throw new NotImplementedException();
        }

        public bool ShouldTrySpawn => shouldSpawn;

        public bool GetCellBool(int index)
        {
            return positionGrid[index];
        }

        public Color GetCellExtraColor(int index)
        {
            if (positionGrid[index])
                return Color.red;
            return Color.white;
        }

        public Color Color => Color.white;

        public void RegisterBlossom(TiberiumBlossom blossom)
        {
            blossomGrid.Set(blossom.Position, true);


        }

        public void DeregisterBlossom(TiberiumBlossom blossom)
        {
            blossomGrid.Set(blossom.Position, false);


        }

        public void UpdateGrid()
        {

        }

        private void GetPositions()
        {
            Predicate<IntVec3> Predicate = c => c.InBounds(map);
            Action<IntVec3> Processor = delegate(IntVec3 c)
            {
                var originalTerrain = c.GetTerrain(map);
               // map.terrainGrid.SetTerrain(c, TiberiumTerrainDefOf.TiberiumSoilGreen);
                if (!c.Standable(map) || c.Fogged(map) || c.Roofed(map)) return;
               // map.terrainGrid.SetTerrain(c, TiberiumTerrainDefOf.TiberiumSoilBlue);
                if (c.DistanceToEdge(map) <= 15) return;
               // map.terrainGrid.SetTerrain(c, TiberiumTerrainDefOf.TiberiumSoilRed);
                if (!TiberiumDefOf.TerrainFilter_Soil.AllowsTerrainDef(originalTerrain)) return;
                //map.terrainGrid.SetTerrain(c, TiberiumTerrainDefOf.TiberiumPodSoilBlue);
                if (!HasConflict(-1, c))
                {
                    //map.terrainGrid.SetTerrain(c, TiberiumTerrainDefOf.TiberiumIce);
                    positions.Add(c);
                    positionGrid.Set(c, true);
                }
            };
            map.floodFiller.FloodFill(map.Center, Predicate, Processor);

            int trueCount = 0;
            for (var i = positions.Count - 1; i >= 0; i--)
            {
                if (Rand.Chance(0.5f))
                {
                    trueCount++;
                    positionGrid.Set(positions[i], false);
                    positions.RemoveAt(i);
                }
            }
        }

        private bool HasConflict(int size, IntVec3 pos)
        {
            return positions.Any(c => pos.DistanceTo(c) < 35f); //positionsBySize[size].Any(c => c.DistanceTo(pos) < radiusBySize[size]);
        }

        //Get The Percentual Value Of The Position Based On The Distance From Center To Edge
        private float PercentileAt(IntVec3 pos)
        {
            return Mathf.InverseLerp(0, mapRadius, map.Center.DistanceTo(pos));
        }
    }
}
