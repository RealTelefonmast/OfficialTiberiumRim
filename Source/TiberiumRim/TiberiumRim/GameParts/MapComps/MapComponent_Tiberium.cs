using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class MapComponent_Tiberium : MapComponent
    {
        public TiberiumMapInfo TiberiumInfo;
        public TiberiumStructureInfo StructureInfo;

        public HashSet<IntVec3> AffectedCells = new HashSet<IntVec3>();
        public HashSet<IntVec3> IteratorTiles = new HashSet<IntVec3>();

        public HashSet<IntVec3> InhibitedCells = new HashSet<IntVec3>();
        private IEnumerator<IntVec3> TileIterator;
        private bool dirtyIterator = false;
        private int TiberiumArrivalTick = 0;

        //Debug
        public Region currentDebugRegion;
        public IntVec3 currentDebugCell;

        public MapComponent_Tiberium(Map map) : base(map)
        {
            TiberiumInfo = new TiberiumMapInfo(map);
            StructureInfo = new TiberiumStructureInfo(map);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void MapGenerated()
        {
            base.MapGenerated();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref TiberiumArrivalTick, "arrivalTick");
            base.ExposeData();
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (TiberiumRimSettings.settings.ShowNetworkValues)
            {
                
                TiberiumInfo.TiberiumGrid.drawer.RegenerateMesh();
                TiberiumInfo.TiberiumGrid.drawer.MarkForDraw();
                TiberiumInfo.TiberiumGrid.drawer.CellBoolDrawerUpdate();
                
                //Suppression.SuppressionGrid.drawer.RegenerateMesh();
                //Suppression.SuppressionGrid.drawer.MarkForDraw();
                //Suppression.SuppressionGrid.drawer.CellBoolDrawerUpdate();
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            IterateThroughTiles();
            if (Find.TickManager.TicksGame % 750 == 0)
                TiberiumInfo.TiberiumGrid.UpdateDirties();
        }

        public MapComponent_Suppression Suppression => map.GetComponent<MapComponent_Suppression>();

        public MapComponent_TNWManager TNWManager => map.GetComponent<MapComponent_TNWManager>();

        public bool TiberiumAvailable
        {
            get
            {
                return TiberiumInfo.TiberiumCrystals[HarvestType.Valuable].Count > TNWManager.ReservationManager.ReservedTypes[HarvestType.Valuable];
            }
        }

        public bool MossAvailable
        {
            get
            {
                return TiberiumInfo.TiberiumCrystals[HarvestType.Unvaluable].Count > TNWManager.ReservationManager.ReservedTypes[HarvestType.Unvaluable];
            }
        }

        public IEnumerable<Thing> TiberiumSetForHarvester(Harvester harvester)
        {
            Log.Message("Getting tiberium set for " + harvester + " with mode "+ harvester.harvestMode);
            Log.Message("Count for that mode: " + TiberiumInfo.TiberiumCrystals[HarvestType.Valuable]?.Count);
            List<TiberiumCrystal> things = new List<TiberiumCrystal>();
            switch (harvester.harvestMode)
            {
                case HarvestMode.Nearest:
                    things = TiberiumInfo.TiberiumCrystals[HarvestType.Valuable];break;
                case HarvestMode.Value:
                    things = TiberiumInfo.TiberiumCrystals[HarvestType.Valuable].Where(t => t.def == TiberiumInfo.MostValuableType).ToList(); break;
                case HarvestMode.Moss:
                    things = TiberiumInfo.TiberiumCrystals[HarvestType.Unvaluable]; break;
            }
            Log.Message("Revisited count on things list " + things.Count());
            return things.Select(t => t as Thing);;
        }

        public void IterateThroughTiles()
        {
            if (!IteratorTiles.Any())
                return;
            //Setup Iterator
            if(TileIterator == null || dirtyIterator)
            {
                TileIterator = IteratorTiles.InRandomOrder().GetEnumerator();
                dirtyIterator = false;
            }
            //Affect Objects
            if(TileIterator?.Current.IsValid ?? false)
            {
                currentDebugCell = TileIterator.Current;
                TiberiumCrystal affecter = currentDebugCell.CellsAdjacent8Way().Select(c => c.GetTiberium(map)).FirstOrDefault();
                AffectPotentialObject(currentDebugCell, affecter);
            }
            if (!TileIterator.MoveNext())
            {
                dirtyIterator = true;
            }
        }

        private void AffectPotentialObject(IntVec3 cell, TiberiumCrystal affecter)
        {
            if (affecter == null) return;
            ThingDef newThing = null;
            float damageFactor = 1;
            var haulable = cell.GetFirstHaulable(map);
            if (haulable != null && haulable.CanBeAffected(out damageFactor))
            {
                if (haulable.def.IsNutritionGivingIngestible)
                {
                    damageFactor += 0.33f;
                }
                if (haulable.IsCorruptableChunk())
                {
                    newThing = affecter.def.chunk;
                }
                haulable.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, TRUtils.Range(affecter.def.tiberium.entityDamage) * damageFactor));
                if (newThing != null && TRUtils.Chance(MainTCD.Main.ChunkCorruptionChance))
                {
                    GenSpawn.Spawn(newThing, haulable.Position, map);
                    if (!haulable.DestroyedOrNull())
                    {
                        haulable.DeSpawn();
                    }
                }
                return;
            }
            Building building = cell.GetFirstBuilding(map);
            if (building != null && building.CanBeAffected(out damageFactor))
            {
                float chance = 1f;
                if (building is Building_SteamGeyser)
                {
                    newThing = TiberiumDefOf.TiberiumGeyser;
                    chance *= MainTCD.Main.GeyserCorruptionChance;
                }
                if (building.def.mineable)
                {
                    newThing = affecter.def.rock;
                    chance *= MainTCD.Main.RockCorruptionChance;
                }
                if (building.def.defName.Contains("Wall"))
                {
                    newThing = affecter.def.wall;
                    chance *= MainTCD.Main.WallCorruptionChance;
                }
                building.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, TRUtils.Range(affecter.def.tiberium.buildingDamage)));
                if (newThing != null && TRUtils.Chance(chance))
                {
                    GenSpawn.Spawn(newThing, building.Position, map);
                    if (!building.DestroyedOrNull())
                    {
                        building.DeSpawn();
                    }
                }
            }
        }     

        public IEnumerable<IntVec3> PawnCells
        {
            get
            {
                return map.mapPawns.AllPawnsSpawned.SelectMany(p => p.CellsAdjacent8WayAndInside().Where(c => AffectedCells.Contains(c)));
            }
        }
        
        public IEnumerable<Region> PawnRegions
        {
            get
            {
                return map.mapPawns.AllPawnsSpawned.Select(p => p.GetRegion()).Where(r => AffectedCells.Contains(r.Cells.RandomElement()));
            }
        }

        public void AddTiberium(TiberiumCrystal crystal, bool respawn)
        {
            TiberiumInfo.RegisterTiberium(crystal);
            AddCells(crystal);
        }

        public void RemoveTiberium(TiberiumCrystal crystal)
        {
            TiberiumInfo.DeregisterTiberium(crystal);
            RemoveCells(crystal);
        }

        private void AddCells(TiberiumCrystal crystal)
        {
            List<IntVec3> cells = crystal.CellsAdjacent8WayAndInside().ToList();
            for (int i = 0; i < cells.Count; i++)
            {
                IntVec3 cell = cells[i];
                if (!cell.InBounds(map)) continue;
                if (cell.GetTiberium(map) == null)
                {
                    IteratorTiles.Add(cell);
                }
                else
                {
                    IteratorTiles.Remove(cell);
                }

                if (AffectedCells.Contains(cell)) continue;
                AffectedCells.Add(cell);
                dirtyIterator = true;
            }
        }

        private void RemoveCells(TiberiumCrystal crystal)
        {
            IEnumerable<IntVec3> cells = crystal.CellsAdjacent8WayAndInside();
            IteratorTiles.Add(crystal.Position);
            for (int i = 0; i < cells.Count(); i++)
            {
                IntVec3 cell = cells.ElementAt(i);
                if (!cell.InBounds(map)) continue;
                CellRect rect = new CellRect(cell.x - 1, cell.z - 1, 3, 3);
                bool flag = true;
                for (int ii = 0; ii < rect.Cells.Count(); ii++)
                {
                    IntVec3 cell2 = rect.Cells.ElementAt(ii);
                    TiberiumCrystal crystal2 = cell2.GetTiberium(map);
                    if (crystal2 != null && crystal2 != crystal)
                    {
                        flag = false;
                    }
                }

                if (!flag) continue;
                AffectedCells.Remove(cell);
                IteratorTiles.Remove(cell);
                dirtyIterator = true;
            }
        }
    }
}
