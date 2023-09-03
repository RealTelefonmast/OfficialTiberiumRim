using System;
using System.Linq;
using TeleCore;
using UnityEngine;
using Verse;

namespace TR
{
    public class TiberiumBlossomInfo : MapInformation, ICellBoolGiver
    {
        private readonly CellBoolDrawer drawer;
        private readonly float mapRadius;
        private static readonly float minDistance = 30;

        private TiberiumBlossom[] blossomGrid;
        private BoolGrid blossomPositionGrid;
        private BoolGrid positionGrid;
        //private static List<IntVec3> positions;

        public TiberiumBlossomInfo(Map map) : base(map)
        {
            drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 0.4f);
            mapRadius = map.Center.DistanceToEdge(map);
        }

        public override void InfoInit(bool initAfterReload = false)
        {
            base.InfoInit();
            if (initAfterReload) return;
            blossomGrid = new TiberiumBlossom[map.cellIndices.NumGridCells];
            blossomPositionGrid = new BoolGrid(map);
            positionGrid = new BoolGrid(map);
        }

        public override void ExposeDataExtra()
        {
            Scribe_Deep.Look(ref blossomPositionGrid, "blossomPositionGrid");
            Scribe_Deep.Look(ref positionGrid, "positionGrid");
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void Update()
        { 
            drawer.RegenerateMesh(); 
            drawer.MarkForDraw();
            drawer.CellBoolDrawerUpdate();
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
            return positionGrid[index] ? Color.green : Color.red;
        }

        public Color Color => Color.white;

        public void RegisterBlossom(TiberiumBlossom blossom)
        {
            if (!blossomPositionGrid[blossom.Position])
                blossomPositionGrid.Set(blossom.Position, true);
        }

        public void DeregisterBlossom(TiberiumBlossom blossom)
        {
            if(blossomPositionGrid[blossom.Position])
                blossomPositionGrid.Set(blossom.Position, false);
        }

        [Obsolete]
        private void GetPositions()
        {
            Predicate<IntVec3> BlossomCheck = delegate(IntVec3 c)
            {
                if (!c.Standable(map) || c.Fogged(map) || c.Roofed(map)) return false;
                if (c.DistanceToEdge(map) <= 10) return false;
                if (!TiberiumDefOf.TerrainFilter_Soil.Allows(c.GetTerrain(map))) return false;
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

        [Obsolete]
        private bool HasConflict(IntVec3 pos)
        {
            return positionGrid.ActiveCells.Any(c => pos.DistanceTo(c) < 30f); //positionsBySize[size].Any(c => c.DistanceTo(pos) < radiusBySize[size]);
        }

        [Obsolete]
        //Get The Percentual Value Of The Position Based On The Distance From Center To Edge
        private float PercentileAt(IntVec3 pos)
        {
            return Mathf.InverseLerp(0, mapRadius, map.Center.DistanceTo(pos));
        }
    }
}
