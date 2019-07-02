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
        public HashSet<TiberiumProducer> TiberiumProducers = new HashSet<TiberiumProducer>();
        public TiberiumMapInfo TiberiumInfo;
        public TiberiumStructureInfo StructureInfo;

        public HashSet<IntVec3> AffectedCells = new HashSet<IntVec3>();
        public HashSet<IntVec3> IteratorTiles = new HashSet<IntVec3>();

        public HashSet<IntVec3> InhibitedCells = new HashSet<IntVec3>();
        private IEnumerator<IntVec3> TileIterator;
        private bool ShouldUpdate = false;
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
            Scribe_Collections.Look(ref TiberiumProducers, "TiberiumProducers", LookMode.Reference);
            base.ExposeData();
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (TiberiumRimSettings.settings.ShowNetworkValues)
            {
                /*
                TiberiumInfo.TiberiumGrid.drawer.RegenerateMesh();
                TiberiumInfo.TiberiumGrid.drawer.MarkForDraw();
                TiberiumInfo.TiberiumGrid.drawer.CellBoolDrawerUpdate();
                */
                Suppression.SuppressionGrid.drawer.RegenerateMesh();
                Suppression.SuppressionGrid.drawer.MarkForDraw();
                Suppression.SuppressionGrid.drawer.CellBoolDrawerUpdate();
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            IterateThroughTiles();
            if (Find.TickManager.TicksGame % 144 == 0)
            {
                TiberiumInfo.TiberiumGrid.UpdateDirties();
                AffectPawns();
                //IterateThroughRegions();
            }
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

        public TiberiumProducer ClosestProducer(Pawn seeker)
        {
            return TiberiumProducers.MinBy(x => x.Position.DistanceTo(seeker.Position));
        }

        public void IterateThroughTiles()
        {
            //Setup Iterator
            if(TileIterator == null || ShouldUpdate)
            {
                TileIterator = IteratorTiles.InRandomOrder().GetEnumerator();
                ShouldUpdate = false;
            }
            //Affect Objects
            if(TileIterator?.Current.IsValid ?? false)
            {
                IntVec3 cell = TileIterator.Current;
                currentDebugCell = cell;
                TiberiumCrystal affecter = cell.CellsAdjacent8Way().Select(c => c.GetTiberium(map)).RandomElement();
                AffectPotentialObject(cell, affecter);
            }
            if (!TileIterator.MoveNext())
            {
                ShouldUpdate = true;
            }
        }

        public void AffectPawns()
        {
            //Affect Pawns
            HashSet<IntVec3> tempSet = new HashSet<IntVec3>(PawnCells);
            foreach (IntVec3 cell in tempSet)
            {
                if (cell.InBounds(map) && (cell.GetTiberium(map)?.def.IsInfective ?? false))
                {
                    Pawn pawn = cell.GetFirstPawn(map);
                    if (pawn != null && CanBeAffected(pawn))
                    {
                        HediffUtils.TryAffectPawn(pawn, false);
                    }
                }
            }
        }

        private void AffectPotentialObject(IntVec3 cell, TiberiumCrystal affecter)
        {
            if (affecter == null) return;
            var haulable = cell.GetFirstHaulable(map);
            if (haulable != null && CanBeAffected(haulable))
            {
                ThingDef newThing = null;
                float damageFactor = 1f;
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
            }
            Building building = cell.GetFirstBuilding(map);
            if (building != null && CanBeAffected(building))
            {
                float chance = 1f;
                ThingDef newBuilding = null;
                if (building is Building_SteamGeyser)
                {
                    newBuilding = TiberiumDefOf.TiberiumGeyser;
                    chance *= MainTCD.Main.GeyserCorruptionChance;
                }
                if (building.def.mineable)
                {
                    newBuilding = affecter.def.rock;
                    chance *= MainTCD.Main.RockCorruptionChance;
                }
                if (building.def.defName.Contains("Wall"))
                {
                    newBuilding = affecter.def.wall;
                    chance *= MainTCD.Main.WallCorruptionChance;
                }
                building.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, TRUtils.Range(affecter.def.tiberium.buildingDamage)));
                if (newBuilding != null && TRUtils.Chance(chance))
                {
                    GenSpawn.Spawn(newBuilding, building.Position, map);
                    if (!building.DestroyedOrNull())
                    {
                        building.DeSpawn();
                    }
                }
            }
        }     

        public bool CanBeAffected(Thing thing)
        {
            if(thing is TiberiumObject || thing is TiberiumPawn || thing is TiberiumStructure)
            {
                return false;
            }
            if (thing is Pawn)
                return true;

            return thing.GetStatValue(TiberiumDefOf.TiberiumResistance) < 1;
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
                if (cell.InBounds(map))
                {
                    if (cell.GetTiberium(map) == null)
                    {
                        IteratorTiles.Add(cell);
                    }
                    else
                    {
                        IteratorTiles.Remove(cell);
                    }
                    if (!AffectedCells.Contains(cell))
                    {
                        AffectedCells.Add(cell);
                        ShouldUpdate = true;
                    }
                }
            }
        }

        private void RemoveCells(TiberiumCrystal crystal)
        {
            IEnumerable<IntVec3> cells = crystal.CellsAdjacent8WayAndInside();
            IteratorTiles.Add(crystal.Position);
            for (int i = 0; i < cells.Count(); i++)
            {
                IntVec3 cell = cells.ElementAt(i);
                if (cell.InBounds(map))
                {
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
                    if (flag)
                    {
                        AffectedCells.Remove(cell);
                        IteratorTiles.Remove(cell);
                        ShouldUpdate = true;
                    }
                }
            }
        }
    }
}
