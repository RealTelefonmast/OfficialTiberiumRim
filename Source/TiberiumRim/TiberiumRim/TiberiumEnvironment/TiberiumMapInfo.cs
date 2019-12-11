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

        public Dictionary<HarvestType, List<TiberiumCrystal>> TiberiumCrystals = new Dictionary<HarvestType, List<TiberiumCrystal>>();
        public Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>> TiberiumCrystalsPerType = new Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>>();
        public Dictionary<HarvestType, List<TiberiumCrystalDef>> TiberiumCrystalTypes = new Dictionary<HarvestType, List<TiberiumCrystalDef>>();
        public Dictionary<Region, List<TiberiumCrystal>> TiberiumByRegion = new Dictionary<Region, List<TiberiumCrystal>>();

        public TiberiumGrid TiberiumGrid;
        public TiberiumFloraGrid FloraGrid;
        public int TotalCount;

        public TiberiumMapInfo(Map map)
        {
            this.map = map;
            TiberiumGrid = new TiberiumGrid(map);
            FloraGrid = new TiberiumFloraGrid(map);
            for (int i = 0; i < 3; i++)
            {
                HarvestType type = (HarvestType)i;
                TiberiumCrystals.Add(type, new List<TiberiumCrystal>());
                TiberiumCrystalTypes.Add(type, new List<TiberiumCrystalDef>());
            }
        }

        public TiberiumCrystalDef MostValuableType => TiberiumCrystalTypes[HarvestType.Valuable].MaxBy(t => t.tiberium.harvestValue);

        public float Coverage => TotalCount / (float) map.cellIndices.NumGridCells;

        public void TickTiberium()
        {
            
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
                if (TiberiumCrystalsPerType.ContainsKey(crystal.def))
                {
                    TiberiumCrystalsPerType[crystal.def].Add(crystal);
                }
                else
                {
                    TiberiumCrystalsPerType.Add(crystal.def, new List<TiberiumCrystal> { crystal });
                }
            }
        }

        public void DeregisterTiberium(TiberiumCrystal crystal)
        {
            var def = crystal.def;
            AllTiberiumCrystals.Remove(crystal);
            TiberiumGrid.SetCrystal(crystal.Position, false, null);
            TiberiumCrystals[def.HarvestType].Remove(crystal);
            TiberiumCrystalsPerType[def].Remove(crystal);
            TotalCount--;
            if (!TiberiumCrystalTypes.TryGetValue(crystal.def.HarvestType).Any(c => c == crystal.def))
            {
                TiberiumCrystalTypes[def.HarvestType].Remove(crystal.def);
            }
        }

        public void RegisterTiberiumPlant(TiberiumPlant plant)
        {
            TiberiumGrid.SetPlant(plant.Position, true);
        }

        public void DeregisterTiberiumPlant(TiberiumPlant plant)
        {
            TiberiumGrid.SetPlant(plant.Position, false);
        }
    }
}
