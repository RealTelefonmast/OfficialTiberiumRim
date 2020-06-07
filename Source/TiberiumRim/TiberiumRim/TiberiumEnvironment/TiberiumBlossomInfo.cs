using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TiberiumBlossomInfo : ICellBoolGiver, IExposable
    {
        public BoolGrid blossomGrid;
        private Map map;

        private float mapRadius;

        //Different Sizes Get Different Rules
        //1: Small; 2: Medium; 3: Large
        private IntVec3[][] availablePositionsBySize;
        private int[] counters;
        private float[] radiusBySize;
        private bool shouldSpawn = true;
        

        public TiberiumBlossomInfo(Map map)
        {
            this.map = map;
            mapRadius = map.Center.DistanceToEdge(map);

            blossomGrid = new BoolGrid(map);

            availablePositionsBySize = new IntVec3[3][];
            counters = new int[3];
            radiusBySize = new float[3] {15f, 20f, 30f};

            //SetPotentialPositions();
            //Debug_TryFillPositions();
        }

        public void ExposeData()
        {
            throw new NotImplementedException();
        }

        public bool ShouldTrySpawn => shouldSpawn;

        public bool GetCellBool(int index)
        {
            throw new NotImplementedException();
        }

        public Color GetCellExtraColor(int index)
        {
            throw new NotImplementedException();
        }

        public Color Color { get; }

        public void RegisterBlossom()
        {

        }

        public void DeregisterBlossom()
        {

        }

        public void UpdateGrid()
        {

        }

        private void Debug_TryFillPositions()
        {
            for (int i = 2; i >= 0; i--)
            {
                for(int k = 0; k < counters[i]; k++)
                {
                    if (i == 2)
                    {
                        var tree = TRUtils.Chance(0.6f) ? TiberiumDefOf.BlossomTree : TiberiumDefOf.BlueBlossomTree;
                        GenSpawn.Spawn(tree, availablePositionsBySize[i][k], map);
                    }
                    if(i == 1)
                    {
                        GenSpawn.Spawn(TiberiumDefOf.SmallBlossom, availablePositionsBySize[i][k], map);
                    }
                    if (i == 0)
                    {
                        GenSpawn.Spawn(TiberiumDefOf.AlocasiaBlossom, availablePositionsBySize[i][k], map);
                    }
                }
            }
        }

        private void SetPotentialPositions()
        {
            int currentSize = 0;

            Predicate<IntVec3> BlossomValidator = cell =>
                cell.DistanceToEdge(map) >= EdgeRangeForSize(currentSize);
            Predicate<IntVec3> Predicate = c => c.IsValid;
            Action<IntVec3> Processor = delegate(IntVec3 c)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (!HasConflict(i, c) && InRangeForSize(i, c) && TRUtils.Chance(ChanceForSizeAt(i, c)))
                    {
                        availablePositionsBySize[i][counters[i]] = c;
                        counters[i]++;
                    }
                }
            };
            map.floodFiller.FloodFill(map.cellIndices.IndexToCell(0), Predicate, Processor);   
        }

        private int EdgeRangeForSize(int size)
        {
            switch (size)
            {
                case 0:
                    return TRUtils.Range(10, 15);
                case 1:
                    return TRUtils.Range(15, 20);
                case 2:
                    return TRUtils.Range(20,25);
                default: 
                    return 9999;
            }
        }

        //The Usage Of The Map From the Center To Edge
        private float RangeForSize(int size)
        {
            switch (size)
            {
                case 0:
                    return 0.75f;
                case 1:
                    return 0.45f;
                case 2:
                    return 0f;
                default:
                    return 2;
            }
        }

        //Determine The Chance For A Specific Blossom Size To Spawn At The Designated Position
        private float ChanceForSizeAt(int size, IntVec3 pos)
        {
            //The further away it is from the max radius, the less likely it will be
            float distanceChance = 1f - PercentileAt(pos);
            return distanceChance;

        }

        private bool InRangeForSize(int size, IntVec3 pos)
        {
            float pct = PercentileAt(pos);
            return pct >= RangeForSize(size);
        }

        private bool HasConflict(int size, IntVec3 pos)
        {
            return availablePositionsBySize[size].Any(c => c.DistanceTo(pos) < radiusBySize[size]);
        }

        //Get The Percentual Value Of The Position Based On The Distance From Center To Edge
        private float PercentileAt(IntVec3 pos)
        {
            return Mathf.InverseLerp(0, mapRadius, map.Center.DistanceTo(pos));
        }
    }
}
