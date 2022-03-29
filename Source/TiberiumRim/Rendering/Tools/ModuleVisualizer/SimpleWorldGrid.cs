using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class SimpleWorldGrid
    {
        public List<Tile> tiles = new List<Tile>();
        public List<Vector3> verts;
        public List<int> tileIDToVerts_offsets;
        public List<int> tileIDToNeighbors_offsets;
        public List<int> tileIDToNeighbors_values;
        public float averageTileSize; 
        public Vector3 viewCenter;
        public float viewAngle;

        private int cachedTraversalDistance = -1;
        private int cachedTraversalDistanceForStart = -1;
        private int cachedTraversalDistanceForEnd = -1;
        private static List<int> tmpNeighbors = new List<int>();
        private const int SubdivisionsCount = 10;
        public const float PlanetRadius = 100f;
        public const int ElevationOffset = 8192;
        public const int TemperatureOffset = 300;
        public const float TemperatureMultiplier = 10f;

        public float PlanetCoverage => 1.0f;

        public int TilesCount => tileIDToNeighbors_offsets.Count;
        public Vector3 NorthPolePos => new Vector3(0f, 100f, 0f);
        public bool HasWorldData => false; //this.tileBiome != null;

        //
        public Tile this[int tileID]
        {
            get
            {
                if ((ulong)tileID >= (ulong)((long)TilesCount))
                {
                    return null;
                }
                return tiles[tileID];
            }
        }

        public SimpleWorldGrid()
        {
            CalculateViewCenterAndAngle();
            PlanetShapeGenerator.Generate(10, out verts, out tileIDToVerts_offsets, out tileIDToNeighbors_offsets, out tileIDToNeighbors_values, 100f, viewCenter, viewAngle);
            CalculateAverageTileSize();
        }

        private void CalculateViewCenterAndAngle()
        {
            viewAngle = PlanetCoverage * 180f;
            viewCenter = Vector3.back;
            float angle = 45f;
            if (viewAngle > 45f)
            {
                angle = Mathf.Max(90f - viewAngle, 0f);
            }
            viewCenter = Quaternion.AngleAxis(angle, Vector3.right) * viewCenter;
        }

        private void CalculateAverageTileSize()
        {
            int tilesCount = TilesCount;
            double num = 0.0;
            int num2 = 0;
            for (int i = 0; i < tilesCount; i++)
            {
                Vector3 tileCenter = GetTileCenter(i);
                int num3 = (i + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[i + 1] : tileIDToNeighbors_values.Count;
                for (int j = tileIDToNeighbors_offsets[i]; j < num3; j++)
                {
                    int tileID = tileIDToNeighbors_values[j];
                    Vector3 tileCenter2 = GetTileCenter(tileID);
                    num += (double)Vector3.Distance(tileCenter, tileCenter2);
                    num2++;
                }
            }
            averageTileSize = (float)(num / (double)num2);
        }

        public Vector3 GetTileCenter(int tileID)
        {
            int num = (tileID + 1 < tileIDToVerts_offsets.Count) ? tileIDToVerts_offsets[tileID + 1] : verts.Count;
            Vector3 a = Vector3.zero;
            int num2 = 0;
            for (int i = tileIDToVerts_offsets[tileID]; i < num; i++)
            {
                a += verts[i];
                num2++;
            }
            return a / (float)num2;
        }

        public bool InBounds(int tileID)
        {
            return (ulong)tileID < (ulong)((long)TilesCount);
        }

        public Vector2 LongLatOf(int tileID)
        {
            Vector3 tileCenter = GetTileCenter(tileID);
            float x = Mathf.Atan2(tileCenter.x, -tileCenter.z) * 57.29578f;
            float y = Mathf.Asin(tileCenter.y / 100f) * 57.29578f;
            return new Vector2(x, y);
        }
    }
}
