using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    /* Tiberium Map Component
     * Description:
     * In this component all the major Tiberium-related mechanics get managed
     * Tiberium Information - Main Info on tiberium positions, and cell-states
     *
     */

    public class MapComponent_Tiberium : MapComponentWithDraw
    {
        public TiberiumMapInfo TiberiumInfo;
        public TiberiumFloraMapInfo FloraInfo;

        public TiberiumAffecter TiberiumAffecter;
        public TiberiumSpreader TiberiumSpreader;

        public TiberiumStructureInfo StructureInfo;
        public TiberiumInfectionInfo InfectionInfo;

        public HashSet<IntVec3> AffectedCells = new HashSet<IntVec3>();
        public HashSet<IntVec3> IteratorTiles = new HashSet<IntVec3>();

        //Affected Objects Iterator
        private IEnumerator<IntVec3> TileIterator;
        private bool dirtyIterator = false;

        //Debug
        public Region currentDebugRegion;
        public IntVec3 currentDebugCell;

        public MapComponent_Suppression Suppression => map.GetComponent<MapComponent_Suppression>();
        public MapComponent_TNWManager TNWManager => map.GetComponent<MapComponent_TNWManager>();

        public MapComponent_Tiberium(Map map) : base(map)
        {
            TiberiumInfo  = new TiberiumMapInfo(map);
            FloraInfo     = new TiberiumFloraMapInfo(map);
            StructureInfo = new TiberiumStructureInfo(map);
            InfectionInfo = new TiberiumInfectionInfo(map);
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
            base.ExposeData();
        }

        [TweakValue("MapComponent_TibDrawBool", 0f, 100f)]
        public static bool DrawBool = false;

        /*
        [TweakValue("MapComponent_QuadTreePoints", 0, 100)]
        public static int QuadTreePoints = 1;

        [TweakValue("MapComponent_QuadDrawBool", 0f, 100f)]
        public static bool QuadDrawReset = false;

        //TEST QuadTree
        public QuadTree TestTree;
        */

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (DrawBool)
            {
                TiberiumInfo.Update();


                //Suppression.SuppressionGrid.drawer.RegenerateMesh();
                //Suppression.SuppressionGrid.drawer.MarkForDraw();
                //Suppression.SuppressionGrid.drawer.CellBoolDrawerUpdate();
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            IterateThroughTiles();
            TiberiumInfo.Tick();
        }

        public override void MapComponentDraw()
        {

        }

        public bool TiberiumAvailable => TiberiumInfo.TiberiumCrystals[HarvestType.Valuable].Count > TNWManager.ReservationManager.ReservedTypes[HarvestType.Valuable];

        public bool MossAvailable => TiberiumInfo.TiberiumCrystals[HarvestType.Unvaluable].Count > TNWManager.ReservationManager.ReservedTypes[HarvestType.Unvaluable];

        public void RegisterTiberiumThing(Thing thing)
        {
        }

        public void DeregisterTiberiumThing(Thing thing)
        {
        }

        public void RegisterTiberiumCrystal(TiberiumCrystal crystal)
        {
            TiberiumInfo.RegisterTiberium(crystal);
            AddCells(crystal);
        }

        public void DeregisterTiberiumCrystal(TiberiumCrystal crystal)
        {
            TiberiumInfo.DeregisterTiberium(crystal);
            RemoveCells(crystal);
        }

        public void RegisterTiberiumPlant(TiberiumPlant plant)
        {
            FloraInfo.RegisterTiberiumPlant(plant);
        }

        public void DeregisterTiberiumPlant(TiberiumPlant plant)
        {
            FloraInfo.DeregisterTiberiumPlant(plant);
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
                dirtyIterator = true;
        }

        private void AffectPotentialObject(IntVec3 cell, TiberiumCrystal affecter)
        {
            if (affecter == null) return;
            ThingDef newThing = null;
            float damageFactor = 1;
            var haulable = cell.GetFirstHaulable(map);
            if (haulable != null && affecter.def.tiberium.entityDamage.Average > 0 && haulable.CanBeDamagedByTib(out damageFactor))
            {
                if (haulable.def.IsNutritionGivingIngestible)
                    damageFactor += 0.33f;
                if (haulable.IsCorruptableChunk())
                    newThing = affecter.def.chunk;
                haulable.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, damageFactor * TRUtils.Range(affecter.def.tiberium.entityDamage)));
                if (newThing != null && TRUtils.Chance(MainTCD.Main.ChunkCorruptionChance))
                {
                    GenSpawn.Spawn(newThing, haulable.Position, map);
                    if (!haulable.DestroyedOrNull())
                        haulable.DeSpawn();
                }
                return;
            }

            Building building = cell.GetFirstBuilding(map);
            if (building != null && affecter.def.tiberium.buildingDamage.Average > 0 && building.CanBeDamagedByTib(out damageFactor))
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
                if (building.def.IsWall())
                {
                    newThing = affecter.def.wall;
                    chance *= MainTCD.Main.WallCorruptionChance;
                }
                building.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, damageFactor * TRUtils.Range(affecter.def.tiberium.buildingDamage)));
                if (newThing != null && TRUtils.Chance(chance))
                {
                    GenSpawn.Spawn(newThing, building.Position, map);
                    if (!building.DestroyedOrNull())
                        building.DeSpawn();
                }
            }
        }

        /*
        public void AddTiberiumPlant(TiberiumPlant plant)
        {
            FloraInfo.RegisterTiberiumPlant(plant);
        }

        public void RemoveTiberiumPlant(TiberiumPlant plant)
        {
            FloraInfo.DeregisterTiberiumPlant(plant);
        }

        public void AddTiberium(TiberiumCrystal crystal)
        {
            TiberiumInfo.RegisterTiberium(crystal);
            AddCells(crystal);
        }

        public void RemoveTiberium(TiberiumCrystal crystal)
        {
            TiberiumInfo.DeregisterTiberium(crystal);
            RemoveCells(crystal);
        }
        */

        private void AddCells(TiberiumCrystal crystal)
        {
            List<IntVec3> cells = crystal.CellsAdjacent8WayAndInside().ToList();
            for (int i = 0; i < cells.Count; i++)
            {
                IntVec3 cell = cells[i];
                if (!cell.InBounds(map)) continue;
                if (cell.GetTiberium(map) == null)
                    IteratorTiles.Add(cell);
                else
                    IteratorTiles.Remove(cell);

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
                        flag = false;
                }

                if (!flag) continue;
                AffectedCells.Remove(cell);
                IteratorTiles.Remove(cell);
                dirtyIterator = true;
            }
        }
    }
}
