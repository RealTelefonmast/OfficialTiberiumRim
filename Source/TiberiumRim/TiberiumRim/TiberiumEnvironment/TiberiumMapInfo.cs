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
        //Saved as Parent to be compatible with Parent Enumerators
        public HashSet<Thing> AllTiberiumCrystals = new HashSet<Thing>();
        //public List<TiberiumCrystal> TickList = new List<TiberiumCrystal>();

        //Tiberium Access Lists
        public Dictionary<HarvestType, List<TiberiumCrystal>> TiberiumCrystals = new Dictionary<HarvestType, List<TiberiumCrystal>>();
        public Dictionary<HarvestType, List<TiberiumCrystalDef>> TiberiumCrystalTypes = new Dictionary<HarvestType, List<TiberiumCrystalDef>>();
        public Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>> TiberiumCrystalsByDef = new Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>>();

        //Grids
        private TiberiumGrid tiberiumGrid;

        public int TotalCount => AllTiberiumCrystals.Count;
        public float Coverage => TotalCount / (float)map.Area;
        public TiberiumCrystalDef MostValuableType => TiberiumCrystalTypes[HarvestType.Valuable].MaxBy(t => t.tiberium.harvestValue);

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
            //Scribe_Deep.Look(ref tiberiumGrid, "tiberiumGrid", map);
        }

        public override void Tick()
        {
            //if(Find.TickManager.TicksGame % 1 == 0)
        }

        public override void Draw()
        {
            tiberiumGrid.Drawer.RegenerateMesh();
            tiberiumGrid.Drawer.MarkForDraw();
            tiberiumGrid.Drawer.CellBoolDrawerUpdate();
        }

        public TiberiumGrid TiberiumGrid => tiberiumGrid;

        public TiberiumCrystal TiberiumAt(IntVec3 cell)
        {
            return tiberiumGrid.TiberiumCrystals[map.cellIndices.CellToIndex(cell)];
        }
        
        public bool HasTiberiumAt(IntVec3 cell)
        {
            return tiberiumGrid.TiberiumBoolGrid[cell];
        }

        public bool CanGrowFrom(IntVec3 cell)
        {
            return tiberiumGrid.GrowFromGrid[cell];
        }

        public bool CanGrowTo(IntVec3 cell)
        {
            return tiberiumGrid.GrowToGrid[cell];
        }

        public bool IsAffectedCell(IntVec3 cell)
        {
            return tiberiumGrid.AffectedCells[cell];
        }

        public void SetFieldColor(IntVec3 cell, bool value, TiberiumValueType type)
        {
            //TODO: Check Importance
            //tiberiumGrid.SetFieldColor(cell, value, type);
        }

        public void RegisterTiberium(TiberiumCrystal crystal)
        {
            var type = crystal.def.HarvestType;
            AllTiberiumCrystals.Add(crystal);
            if (!TiberiumCrystals[type].Contains(crystal))
            {
                tiberiumGrid.SetCrystal(crystal); // SetCrystal(crystal.Position, true, crystal);
                TiberiumCrystals[type].Add(crystal);
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
            tiberiumGrid.ResetCrystal(crystal.Position);
            //tiberiumGrid.SetCrystal(crystal.Position, false, null);
            TiberiumCrystals[def.HarvestType].Remove(crystal);
            TiberiumCrystalsByDef[def].Remove(crystal);
            if (!TiberiumCrystalTypes.TryGetValue(crystal.def.HarvestType).Any(c => c == crystal.def))
            {
                TiberiumCrystalTypes[def.HarvestType].Remove(crystal.def);
            }
        }
    }
}
