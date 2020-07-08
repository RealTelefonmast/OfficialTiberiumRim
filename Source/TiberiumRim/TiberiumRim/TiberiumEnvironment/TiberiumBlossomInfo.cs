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
        //private static List<IntVec3> positions;

        public TiberiumBlossomInfo(Map map)
        {
            this.map = map;
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.4f);
            mapRadius = map.Center.DistanceToEdge(map);

            blossomGrid = new BoolGrid(map);

            positionGrid = new BoolGrid(map);
            //positions = new List<IntVec3>();

            GetPositions();
        }

        public void ExposeData()
        {
            throw new NotImplementedException();
        }

        public bool ShouldTrySpawn { get; set; } = true;

        public bool TryGetNewBlossom(out IntVec3 pos)
        {
            pos = IntVec3.Invalid;
            if (!ShouldTrySpawn) return false;
            if (positionGrid.TrueCount == 0)
            {
                ShouldTrySpawn = false;
                return false;
            }
            pos = positionGrid.ActiveCells.RandomElement();
            positionGrid.Set(pos, false);
            return true;
        }

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
            if (!blossomGrid[blossom.Position])
                blossomGrid.Set(blossom.Position, true);
            //positionGrid.Set(blossom.Position, false);

        }

        public void DeregisterBlossom(TiberiumBlossom blossom)
        {
            if(blossomGrid[blossom.Position])
                blossomGrid.Set(blossom.Position, false);
            if (!positionGrid[blossom.Position])
                positionGrid.Set(blossom.Position, true);

        }

        public void UpdateGrid()
        {

        }

        private void GetPositions()
        {
            Predicate<IntVec3> BlossomCheck = delegate(IntVec3 c)
            {
                if (!c.Standable(map) || c.Fogged(map) || c.Roofed(map)) return false;
                if (c.DistanceToEdge(map) <= 10) return false;
                if (!TiberiumDefOf.TerrainFilter_Soil.AllowsTerrainDef(c.GetTerrain(map))) return false;
                return true;
            };
            Predicate<IntVec3> Predicate = c => c.InBounds(map);
            Action<IntVec3> Processor = delegate(IntVec3 c)
            {
                if (!HasConflict(c))
                    positionGrid.Set(c, true);
            };
            map.floodFiller.FloodFill(map.Center, Predicate, Processor);
            var cachedList = positionGrid.ActiveCells.ToList();
            foreach (var cell in cachedList)
            {
                positionGrid.Set(cell, false);
                if (Rand.Chance(0.5f)) continue;
                var cells = GenRadial.RadialCellsAround(cell, 15, 20).Where(c => BlossomCheck(c)).ToList();
                if (cells.NullOrEmpty()) continue;
                var rand = cells.RandomElement();
                positionGrid.Set(rand, true);
            }
        }

        private bool HasConflict(IntVec3 pos)
        {
            return positionGrid.ActiveCells.Any(c => pos.DistanceTo(c) < 30f); //positionsBySize[size].Any(c => c.DistanceTo(pos) < radiusBySize[size]);
        }

        //Get The Percentual Value Of The Position Based On The Distance From Center To Edge
        private float PercentileAt(IntVec3 pos)
        {
            return Mathf.InverseLerp(0, mapRadius, map.Center.DistanceTo(pos));
        }
    }
}
