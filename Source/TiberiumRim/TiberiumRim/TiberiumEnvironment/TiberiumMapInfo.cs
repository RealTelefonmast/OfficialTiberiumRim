using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace TiberiumRim
{
    public enum HarvestType
    {
        Valuable,
        Unvaluable,
        Unharvestable
    }

    public class TiberiumMapInfo : MapInformation
    {
        //Saved as Thing to be compatible with Thing Enumerators
        public HashSet<Thing> AllTiberiumCrystals = new HashSet<Thing>();
        public List<TiberiumCrystal> TickList = new List<TiberiumCrystal>();

        //Tiberium Access Lists
        public Dictionary<HarvestType, List<TiberiumCrystal>> TiberiumCrystals = new Dictionary<HarvestType, List<TiberiumCrystal>>();
        public Dictionary<HarvestType, List<TiberiumCrystalDef>> TiberiumCrystalTypes = new Dictionary<HarvestType, List<TiberiumCrystalDef>>();
        public Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>> TiberiumCrystalsByDef = new Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>>();

        //Grids
        private TiberiumGrid tiberiumGrid;
        public int TotalCount;

        //Harvest Information

        public TiberiumMapInfo(Map map) : base(map)
        {
            tiberiumGrid = new TiberiumGrid(map);
            for (int i = 0; i < 3; i++)
            {
                HarvestType type = (HarvestType)i;
                TiberiumCrystals.Add(type, new List<TiberiumCrystal>());
                TiberiumCrystalTypes.Add(type, new List<TiberiumCrystalDef>());
            }
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref tiberiumGrid, "tiberiumGrid", map);
        }

        public TiberiumCrystalDef MostValuableType => TiberiumCrystalTypes[HarvestType.Valuable].MaxBy(t => t.tiberium.harvestValue);

        public float Coverage => TotalCount / (float) map.Area;

        public void Tick()
        {
            if (Find.TickManager.TicksGame % 250 == 0)
            {
                tiberiumGrid.UpdateDirties();
            }
        }

        public void Update()
        {
            tiberiumGrid.drawer.RegenerateMesh();
            tiberiumGrid.drawer.MarkForDraw();
            tiberiumGrid.drawer.CellBoolDrawerUpdate();
        }

        public TiberiumGrid GetGrid()
        {
            return tiberiumGrid;
        }

        public TiberiumCrystal TiberiumAt(IntVec3 cell)
        {
            return tiberiumGrid.TiberiumCrystals[map.cellIndices.CellToIndex(cell)];
        }
        
        public bool HasTiberiumAt(IntVec3 cell)
        {
            return tiberiumGrid.tiberiumGrid[cell];
        }

        public bool CanGrowFrom(IntVec3 cell)
        {
            return tiberiumGrid.growFromGrid[cell];
        }

        public bool CanGrowTo(IntVec3 cell)
        {
            return tiberiumGrid.growToGrid[cell] || tiberiumGrid.alwaysGrowFrom[cell];
        }

        public bool IsAffectedCell(IntVec3 cell)
        {
            return tiberiumGrid.affectedCells[cell];
        }

        public bool ForceGrowAt(IntVec3 cell)
        {
            return tiberiumGrid.alwaysGrowFrom[cell];
        }

        public void SetForceGrowBool(IntVec3 c, bool val)
        {
            tiberiumGrid.alwaysGrowFrom[c] = val;
        }

        public void SetFieldColor(IntVec3 cell, bool value, TiberiumValueType type)
        {
            tiberiumGrid.SetFieldColor(cell, value, type);
        }

        public void RegisterTiberium(TiberiumCrystal crystal)
        {
            var type = crystal.def.HarvestType;
            AllTiberiumCrystals.Add(crystal);
            if (!TiberiumCrystals[type].Contains(crystal))
            {
                tiberiumGrid.SetCrystal(crystal.Position, true, crystal);
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
            tiberiumGrid.SetCrystal(crystal.Position, false, null);
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
