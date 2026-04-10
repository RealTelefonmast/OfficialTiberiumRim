using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace TiberiumRim
{
    public class WorldComponent_TiberiumSpread : WorldComponent
    {
        private List<int> tmpNeighbors = new List<int>();

        public Dictionary<int, float> TiberiumTiles = new Dictionary<int, float>();

        public WorldLayer_Tiberium tibLayer;

        //Bools for world
        public bool tiberiumSpawned = false;

        public WorldComponent_TiberiumSpread(World world) : base(world)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look<int, float>(ref this.TiberiumTiles, "TiberiumTiles", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look<int>(ref this.tmpNeighbors, "tmpNeighbors", LookMode.Value);
            Scribe_Values.Look(ref tiberiumSpawned, "tiberiumSpawned");
            base.ExposeData();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void WorldComponentTick()
        {
            if (TiberiumRimSettings.settings.WorldSpread)
            {
                base.WorldComponentTick();
                if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
                {
                    AffectSurroundingTiles();
                }

                if (Find.TickManager.TicksGame % 750 == 0)
                {
                    GrowBiomes();
                    if (tibLayer != null)
                    {
                        tibLayer.RegenerateNow();
                    }
                }
            }
        }

        public void AffectSurroundingTiles()
        {
            Dictionary<int, float> tmpDict = new Dictionary<int, float>();
            tmpDict = TiberiumTiles.CopyDictionary();
            foreach(int tile in tmpDict.Keys)
            {
                if (IsTiberiumTile(tile))
                {
                    Find.WorldGrid.GetTileNeighbors(tile, tmpNeighbors);
                    for (int i = 0; i < tmpNeighbors.Count; i++)
                    {
                        int tile2 = tmpNeighbors[i];
                        if (!TiberiumTiles.ContainsKey(tile2))
                        {
                            Tile tile3 = world.grid.tiles[tile2];
                            if (tile3.biome.defName.Contains("Ice"))
                            {
                                tmpNeighbors.Remove(tile2);
                            }
                            else
                            {
                                if (!TiberiumTiles.ContainsKey(tile2))
                                {
                                    TiberiumTiles.Add(tile2, MainTCD.MainTiberiumControlDef.WorldCorruptAdder * TiberiumRimSettings.settings.SpreadMltp);
                                    tmpNeighbors.Remove(tile2);
                                }
                            }
                        }
                        else
                        {
                            if(TiberiumTiles[tile2] < MainTCD.MainTiberiumControlDef.WorldCorruptMinPct)
                            if (Rand.Chance(0.001f))
                            {
                                Map map = Find.Maps.Find((Map x) => x.Tile == tile2);
                                if (map != null && map.IsPlayerHome)
                                {
                                    TiberiumType type = Rand.Element(TiberiumType.Green, TiberiumType.Blue);
                                    TRUtils.SpawnTiberiumFromMapEdge(map, type, out TiberiumCrystal crystal);
                                    Messages.Message("TiberiumSpawnFromOutside".Translate(), crystal, MessageTypeDefOf.NeutralEvent);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void GrowBiomes()
        {
            Dictionary<int, float> tmpDict = new Dictionary<int, float>();
            tmpDict = TiberiumTiles.CopyDictionary();
            foreach (int tile in tmpDict.Keys)
            {
                if(TiberiumTiles[tile] < 1f)
                {
                    Tile tile2 = Find.WorldGrid.tiles[tile];
                    if (tile2 != Find.AnyPlayerHomeMap.TileInfo)
                    {
                        float add = MainTCD.MainTiberiumControlDef.WorldCorruptAdder;
                        if (tile2.hilliness == Hilliness.Mountainous || tile2.hilliness == Hilliness.Impassable)
                        {
                            TiberiumTiles[tile] += Rand.Range(0f, add/4) * TiberiumRimSettings.settings.SpreadMltp;
                        }
                        else
                        if (tile2.temperature < 10)
                        {
                            TiberiumTiles[tile] += Rand.Range(0f, add/2) * TiberiumRimSettings.settings.SpreadMltp;
                        }
                        else
                        {
                            TiberiumTiles[tile] += Rand.Range(0f, add) * TiberiumRimSettings.settings.SpreadMltp;
                        }
                        if (TiberiumTiles[tile] >= 1f)
                        {
                            TiberiumTiles[tile] = 1f;
                        }
                    }
                }
            }
        }

        public float GetPct(int tile)
        {
            if (TiberiumTiles.ContainsKey(tile))
            {
                return TiberiumTiles[tile];
            }
            return 0f;
        }

        public bool IsTiberiumTile(int tileInt)
        {
            if (TiberiumTiles[tileInt] >= MainTCD.MainTiberiumControlDef.WorldCorruptMinPct)
            {
                return true;
            }
            return false;
        }
    }
}
