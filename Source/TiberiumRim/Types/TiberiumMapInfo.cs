using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public enum HarvestType
{
    Valuable,
    Unvaluable,
    Unharvestable
}

public class TiberiumMapInfo
{
    //Saved as Thing to be compatible with Thing Enumerators
    public HashSet<Thing> AllTiberiumCrystals = new();
    public TiberiumFloraGrid FloraGrid;
    public Map map;
    public Dictionary<Region, List<TiberiumCrystal>> TiberiumByRegion = new();

    public Dictionary<HarvestType, List<TiberiumCrystal>> TiberiumCrystals = new();
    public Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>> TiberiumCrystalsByDef = new();
    public Dictionary<HarvestType, List<TiberiumCrystalDef>> TiberiumCrystalTypes = new();

    public TiberiumGrid TiberiumGrid;
    public List<TiberiumCrystal> TickList = new();
    public int TotalCount;

    public TiberiumMapInfo(Map map)
    {
        this.map = map;
        TiberiumGrid = new TiberiumGrid(map);
        FloraGrid = new TiberiumFloraGrid(map);
        for (var i = 0; i < 3; i++)
        {
            var type = (HarvestType)i;
            TiberiumCrystals.Add(type, new List<TiberiumCrystal>());
            TiberiumCrystalTypes.Add(type, new List<TiberiumCrystalDef>());
        }
    }

    public TiberiumCrystalDef MostValuableType =>
        TiberiumCrystalTypes[HarvestType.Valuable].MaxBy(t => t.tiberium.harvestValue);

    public float Coverage => TotalCount / (float)map.cellIndices.NumGridCells;

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
            if (!TiberiumCrystalTypes[type].Contains(crystal.def)) TiberiumCrystalTypes[type].Add(crystal.def);
            if (TiberiumCrystalsByDef.ContainsKey(crystal.def))
                TiberiumCrystalsByDef[crystal.def].Add(crystal);
            else
                TiberiumCrystalsByDef.Add(crystal.def, new List<TiberiumCrystal> { crystal });
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
            TiberiumCrystalTypes[def.HarvestType].Remove(crystal.def);
    }

    public void RegisterTiberiumPlant(TiberiumPlant plant)
    {
        TiberiumGrid.SetPlant(plant.Position, true);
        FloraGrid.Notify_PlantSpawned(plant);
    }

    public void DeregisterTiberiumPlant(TiberiumPlant plant)
    {
        TiberiumGrid.SetPlant(plant.Position, false);
    }
}