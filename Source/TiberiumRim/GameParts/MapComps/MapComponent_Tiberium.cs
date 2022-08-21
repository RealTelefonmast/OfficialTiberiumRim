using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using HarmonyLib;
using RimWorld;
using TeleCore;
using Verse;

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
        //Map Information - This encloses all the different areas of a map which can be affected by tiberium, and ensures correct and dynamic effects
        // Natural
        public TiberiumMapInfo TiberiumInfo => map.GetMapInfo<TiberiumMapInfo>(); // Tiberium Crystals, Pods, etc, all variations
        public TiberiumFloraMapInfo FloraInfo => map.GetMapInfo<TiberiumFloraMapInfo>();  // Tiberium Plant life, Gardens, Environment
        public TiberiumStructureInfo NaturalTiberiumStructureInfo => map.GetMapInfo<TiberiumStructureInfo>();
        public StructureCacheMapInfo StructureCacheInfo => map.GetMapInfo<StructureCacheMapInfo>();

        //
        public MapPawnInfo MapPawnInfo => map.GetMapInfo<MapPawnInfo>(); // Currently infected pawns, animals, colonists, visitors, etc
        public DangerMapInfo DangerInfo => map.GetMapInfo<DangerMapInfo>();
        public GeneralDataMapInfo GeneralDataInfo => map.GetMapInfo<GeneralDataMapInfo>();
        public DynamicDataCacheInfo DynamicDataInfo => map.GetMapInfo<DynamicDataCacheInfo>();
        public TiberiumTerrainInfo TerrainInfo => map.GetMapInfo<TiberiumTerrainInfo>();

        // Artificial
        public SuppressionMapInfo SuppressionInfo => map.GetMapInfo<SuppressionMapInfo>();
        public HarvesterMapInfo HarvesterInfo => map.GetMapInfo<HarvesterMapInfo>();

        //Active Components
        public TiberiumAffecter TiberiumAffecter => map.GetMapInfo<TiberiumAffecter>();
        public TiberiumProducerInfo TiberiumProducerInfo => map.GetMapInfo<TiberiumProducerInfo>();


        public NetworkMapInfo NetworkInfo => map.GetMapInfo<NetworkMapInfo>();

        public MapComponent_Tiberium(Map map) : base(map)
        {
            TRLog.Debug($"Making new Tiberium MapComp for [{map.uniqueID}]");
            StaticData.Notify_NewTibMapComp(this);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void MapGenerated()
        {
            //Runs once on map generation
            base.MapGenerated();
            FloraInfo.InfoInit(false);
            TerrainInfo.InfoInit(false);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        [TweakValue("MapComponent_TibDrawBool", 0f, 100f)]
        public static bool DrawBool = false;

        [TweakValue("MapComponent_TibHediffBool", 0f, 100f)]
        public static bool HediffBool = false;


        [TweakValue("MapComponent_TibHarvesterBool", 0f, 100f)]
        public static bool HarvesterBool = false;


        public override void MapComponentDraw()
        {
            base.MapComponentDraw();
        }

        public override void MapComponentOnGUI()
        {
            base.MapComponentOnGUI();
            if(HediffBool) 
                TiberiumAffecter.HediffGrid.DrawValues();
            if (HarvesterBool)
            {

            }
        }

        public void TiberiumMapInterfaceUpdate()
        {
            TiberiumAffecter.HediffGrid.Update();
        }

        public void CustomCellSteadyEffect(IntVec3 c)
        {

        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
        }

        public bool TiberiumAvailable => TiberiumInfo.TiberiumCrystals[HarvestType.Valuable].Count > 0;

        public bool MossAvailable => TiberiumInfo.TiberiumCrystals[HarvestType.Unvaluable].Count > 0;

        public void Notify_ThingSpawned(Thing thing)
        {
            //Update MetaData
            DynamicDataInfo.Notify_ThingSpawned(thing);

            if (thing.def is TRThingDef def)
            {
                StructureCacheInfo.RegisterPart(def.TRGroup, thing);
            }
        }

        public void Notify_DespawnedThing(Thing thing)
        {
            //Update MetaData
            DynamicDataInfo.Notify_ThingDespawned(thing);

            if (thing.def is TRThingDef def)
            {
                StructureCacheInfo.DeregisterPart(def.TRGroup, thing);
            }

        }

        public void RegisterTRBuilding(TRBuilding building)
        {
            NaturalTiberiumStructureInfo.TryRegister(building);
            if(building is TiberiumProducer p)
                TiberiumProducerInfo.RegisterProducer(p);
        }

        public void DeregisterTRBuilding(TRBuilding building)
        {
            NaturalTiberiumStructureInfo.Deregister(building);
            if (building is TiberiumProducer p)
                TiberiumProducerInfo.DeregisterProducer(p);
        }

        public void RegisterTiberiumCrystal(TiberiumCrystal crystal)
        {
            TiberiumInfo.RegisterTiberium(crystal);
            TerrainInfo.Notify_TibSpawned(crystal);
            TiberiumAffecter.Notify_TibChanged();
            //AddCells(crystal);
        }

        public void DeregisterTiberiumCrystal(TiberiumCrystal crystal)
        {
            TiberiumInfo.DeregisterTiberium(crystal);
            TiberiumAffecter.Notify_TibChanged();
            //RemoveCells(crystal);
        }

        public void RegisterTiberiumPlant(TiberiumPlant plant)
        {
            FloraInfo.RegisterTiberiumPlant(plant);
        }

        public void DeregisterTiberiumPlant(TiberiumPlant plant)
        {
            FloraInfo.DeregisterTiberiumPlant(plant);
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
        /*
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
        */
    }
}
