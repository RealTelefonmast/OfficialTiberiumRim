using System;
using System.Collections.Generic;
using RimWorld.Planet;
using TeleCore;
using UnityEngine;
using Verse;

namespace TR
{
    public class TiberiumWorldInfo : WorldInfo
    {
        private int worldTiles;

        private static int maxLevel = 1000;
        private static int growthPerTick = 100;
        private static int checkDuration = 250;
        private static float minSpread = 0.75f;


        public Map Map(int tile) => Find.World.worldObjects.WorldObjectAt<MapParent>(tile)?.Map;
        public bool HasMap(int tile) => Map(tile) != null;
        public float CoverageAt(int tile) => HasMap(tile) ? Map(tile).Tiberium().TiberiumInfo.InfestationPercent : (tiberiumGrid[tile] / (float)maxLevel);
        public float WorldCoverageAt(int tile) => (tiberiumGrid[tile] / (float)maxLevel);
        
        public bool FullyInfected(int tile)
        {
            return CoverageAt(tile) >= 1;
        }

        public TiberiumWorldInfo(World world) : base(world)
        {
            worldTiles = world.grid.TilesCount;
            tiberiumGrid = new ushort[worldTiles];
            dataBytes = new byte[worldTiles * 4];
        }

        private ushort[] tiberiumGrid;
        private byte[] dataBytes;

        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                dataBytes = DataSerializeUtility.SerializeUshort(worldTiles, (int i) => tiberiumGrid[i]);
            }
            
            DataExposeUtility.ByteArray(ref dataBytes, "tiberiumWorldBytes");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                DataSerializeUtility.LoadUshort(this.dataBytes, this.worldTiles, delegate(int i, ushort data)
                {
                    this.tiberiumGrid[i] =data;
                });
            }
        }

        public override void InfoTick()
        {
            if (Find.TickManager.TicksGame % checkDuration != 0) 
                return;
            
            return;
            //TODO: re-add world ticking of tib
            for (int i = 0; i < worldTiles; ++i)
            {
                if (tiberiumGrid[i] <= 0) continue;
                if (Rand.MTBEventOccurs(20000, 60000, checkDuration))
                {
                    TrySpread(i);
                }

                if (Rand.MTBEventOccurs(40000, 60000, checkDuration))
                {
                    Grow(i);
                }
            }
        }

        private void TrySpread(int tile)
        {
            if (CoverageAt(tile) < minSpread) return;
            int newTile = FindNewNeighbourFor(tile);
            if (newTile > -1)
            {
                SpawnTiberiumTile(newTile);
            }
        }

        private void Grow(int tile)
        {
            AdjustTiberiumLevelAt(tile, GrowValueAt(tile));
        }

        private int FindNewNeighbourFor(int origin)
        {
            List<int> tempNeighbors = new List<int>();
            Find.WorldGrid.GetTileNeighbors(origin, tempNeighbors);
            if (tempNeighbors.Any())
                return tempNeighbors.RandomElement();
            return -1;
        }

        //The value the specific tile grows with each grow-tick
        private int GrowValueAt(int tile)
        {
            Tile worldTile = Find.WorldGrid[tile];
            if (Current.Game.Maps.Any(m => m.TileInfo == worldTile)) return 0;
            float value = growthPerTick;
            value *= TemperatureFactor(worldTile.temperature);
            value *= HillinessFactor(worldTile.hilliness);
            //TOOD: Add more effects (biome, elevation..)
            return (int)value;
        }

        private float HillinessFactor(Hilliness hilliness)
        {
            switch (hilliness)
            {
                case Hilliness.Flat:
                    return 2;
                case Hilliness.Undefined:
                    return 1f;
                case Hilliness.SmallHills:
                    return 0.8f;
                case Hilliness.LargeHills:
                    return 0.6f;
                case Hilliness.Mountainous:
                    return 0.4f;
                case Hilliness.Impassable:
                    return 0.2f;
                default:
                     return 1;
            }
        }

        private SimpleCurve TemperatureCurve
        {
            get
            {
                var curve = new SimpleCurve();
                curve.Add(-100, 0);
                curve.Add(-50,0.1f);
                curve.Add(0, 1);
                curve.Add(20, 1.15f);
                curve.Add(30, 1.5f);
                curve.Add(100, 0.75f);
                return curve;
            }
        }

        private float TemperatureFactor(float temperature)
        {
            return TemperatureCurve.Evaluate(temperature);
        }

        public void SpawnTiberiumTile(int tile)
        {
            AdjustTiberiumLevelAt(tile, 100);
            //TiberiumTile tibTile = (TiberiumTile)WorldObjectMaker.MakeWorldObject(TiberiumDefOf.TiberiumTile);
            //tibTile.Tile = tile;
            //Find.WorldObjects.Add(tibTile);
        }

        public void AdjustTiberiumLevelAt(int tile, int level)
        {
            tiberiumGrid[tile] = (ushort)Mathf.Clamp(tiberiumGrid[tile] + level, 0, maxLevel);
            Find.World.renderer.SetDirty<WorldLayer_Tiberium>();
        }
    }
}
