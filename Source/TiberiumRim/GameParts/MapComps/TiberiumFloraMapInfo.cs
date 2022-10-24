using System.Collections.Generic;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class TiberiumFloraMapInfo : MapInformation
    {
        private TiberiumFloraGrid floraGrid;
        public List<TiberiumGarden> gardens;

        public TiberiumFloraMapInfo(Map map) : base(map)
        {
            floraGrid = new TiberiumFloraGrid(map);
            gardens = new List<TiberiumGarden>();
        }

        public override void ExposeData()
        {
            base.ExposeData();

        }

        public override void InfoInit(bool initAfterReload = false)
        {
            base.InfoInit();
            if (initAfterReload) return;
            LongEventHandler.QueueLongEvent(delegate ()
            {
                FloodFiller filler = map.floodFiller;
                foreach (IntVec3 cell in map.AllCells)
                {
                    if (ShouldGrowFloraAt(cell)) continue;
                    TerrainDef terrain = cell.GetTerrain(map);
                    if (IsGarden(terrain))
                    {
                        //TiberiumGarden garden = new TiberiumGarden(map);
                        filler.FloodFill(cell, ((IntVec3 c) => c.GetTerrain(map) == terrain), delegate (IntVec3 cell) {
                            floraGrid.SetGrow(cell, true);
                            //garden.AddCell(cell);
                        });
                        //gardens.Add(garden);
                    }
                }
            }, "SettingFloraBools", false, null);
            
        }

        public override void Tick()
        {
            foreach (var garden in gardens)
            {
                garden.GardenTick();
            }
        }

        [TweakValue("[TR]FloraInfo", 0f, 100f)]
        public static bool DrawBool = false;
        
        public override void Update()
        {
            if (DrawBool)
            {
                floraGrid.drawer.RegenerateMesh();
                floraGrid.drawer.MarkForDraw();
                floraGrid.drawer.CellBoolDrawerUpdate();
            }
        }

        public void RegisterTiberiumPlant(TiberiumPlant plant)
        {
            floraGrid.SetFlora(plant.Position, true);
            Notify_PlantSpawned(plant);
        }

        public void DeregisterTiberiumPlant(TiberiumPlant plant)
        {
            floraGrid.SetFlora(plant.Position, false);
        }

        public void MakeGarden(List<IntVec3> gardenCells)
        {
            gardens.Add(new TiberiumGarden(Map, gardenCells));
            gardenCells.ForEach(c => floraGrid.SetGrow(c, true));
        }

        //Bools
        public bool HasFloraAt(IntVec3 c)
        {
            return floraGrid.floraBools[c];
        }

        public bool ShouldGrowFloraAt(IntVec3 c)
        {
            return floraGrid.growBools[c];
        }

        private bool IsGarden(TerrainDef def)
        {
            return def.IsMoss() || (def.IsSoil() && (def.fertility >= 1.2f));
        }

        private bool IsPond(TerrainDef def)
        {
            return def.IsWater && !def.IsRiver;
        }

        public void Notify_PlantSpawned(TiberiumPlant plant)
        {
            floraGrid.Notify_PlantSpawned(plant);
        }
    }
}
