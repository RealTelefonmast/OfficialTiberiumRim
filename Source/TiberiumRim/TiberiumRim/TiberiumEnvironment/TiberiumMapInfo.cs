using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public enum HarvestType
    {
        Valuable,
        Unvaluable,
        Unharvestable
    }

    public class TiberiumMapInfo
    {
        public Map map;
        //Saved as Thing to be compatible with Thing Enumerators
        public HashSet<Thing> AllTiberiumCrystals = new HashSet<Thing>();
        public List<TiberiumCrystal> TickList = new List<TiberiumCrystal>();

        //Tiberium Access Lists
        public Dictionary<HarvestType, List<TiberiumCrystal>> TiberiumCrystals = new Dictionary<HarvestType, List<TiberiumCrystal>>();
        public Dictionary<HarvestType, List<TiberiumCrystalDef>> TiberiumCrystalTypes = new Dictionary<HarvestType, List<TiberiumCrystalDef>>();
        public Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>> TiberiumCrystalsByDef = new Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>>();
        public Dictionary<Region, List<TiberiumCrystal>> TiberiumByRegion = new Dictionary<Region, List<TiberiumCrystal>>();

        //Grids
        private readonly TiberiumGrid TiberiumGrid;
        public int TotalCount;

        public TiberiumMapInfo(Map map)
        {
            this.map = map;
            TiberiumGrid = new TiberiumGrid(map);
            for (int i = 0; i < 3; i++)
            {
                HarvestType type = (HarvestType)i;
                TiberiumCrystals.Add(type, new List<TiberiumCrystal>());
                TiberiumCrystalTypes.Add(type, new List<TiberiumCrystalDef>());
            }
        }

        public TiberiumCrystalDef MostValuableType => TiberiumCrystalTypes[HarvestType.Valuable].MaxBy(t => t.tiberium.harvestValue);

        public float Coverage => TotalCount / (float) map.Area;

        public void Tick()
        {
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                TiberiumGrid.UpdateDirties();
            }
        }

        public void Update()
        {
            TiberiumGrid.drawer.RegenerateMesh();
            TiberiumGrid.drawer.MarkForDraw();
            TiberiumGrid.drawer.CellBoolDrawerUpdate();
        }

        public TiberiumGrid GetGrid()
        {
            return TiberiumGrid;
        }

        public TiberiumCrystal TiberiumAt(IntVec3 cell)
        {
            return TiberiumGrid.TiberiumCrystals[map.cellIndices.CellToIndex(cell)];
        }

        public bool HasTiberiumAt(IntVec3 cell)
        {
            return TiberiumGrid.tiberiumGrid[cell];
        }

        public bool CanGrowFrom(IntVec3 cell)
        {
            return TiberiumGrid.growFromGrid[cell];
        }

        public bool CanGrowTo(IntVec3 cell)
        {
            return TiberiumGrid.growToGrid[cell] || TiberiumGrid.forceGrow[cell];
        }

        public bool IsAffectedCell(IntVec3 cell)
        {
            return TiberiumGrid.affectedCells[cell];
        }

        public bool ForceGrowAt(IntVec3 cell)
        {
            return TiberiumGrid.forceGrow[cell];
        }

        public void SetForceGrowBool(IntVec3 c, bool val)
        {
            TiberiumGrid.forceGrow[c] = val;
        }

        public void SetFieldColor(IntVec3 cell, bool value, TiberiumValueType type)
        {
            TiberiumGrid.SetFieldColor(cell, value, type);
        }

        public void RegisterTiberium(TiberiumCrystal crystal)
        {
            var type = crystal.def.HarvestType;
            AllTiberiumCrystals.Add(crystal);
            if (!TiberiumCrystals[type].Contains(crystal))
            {
                TiberiumGrid.SetCrystal(crystal.Position, true, crystal);
                TiberiumCrystals[type].Add(crystal);
                TotalCount++;
                if (!TiberiumCrystalTypes[type].Contains(crystal.def))
                {
                    TiberiumCrystalTypes[type].Add(crystal.def);
                }
                if (TiberiumCrystalsByDef.ContainsKey(crystal.def))
                {
                    TiberiumCrystalsByDef[crystal.def].Add(crystal);
                }
                else
                {
                    TiberiumCrystalsByDef.Add(crystal.def, new List<TiberiumCrystal> { crystal });
                }
            }
        }

        public void DeregisterTiberium(TiberiumCrystal crystal)
        {
            var def = crystal.def;
            AllTiberiumCrystals.Remove(crystal);
            TiberiumGrid.SetCrystal(crystal.Position, false, null);
            TiberiumCrystals[def.HarvestType].Remove(crystal);
            TiberiumCrystalsByDef[def].Remove(crystal);
            TotalCount--;
            if (!TiberiumCrystalTypes.TryGetValue(crystal.def.HarvestType).Any(c => c == crystal.def))
            {
                TiberiumCrystalTypes[def.HarvestType].Remove(crystal.def);
            }
        }
    }
}
