using System.Collections.Generic;
using TR.Grids;
using Verse;

namespace TR.Info;

public class TiberiumMapInfo : MapInformation
{
    //Grids
    private TiberiumGrid _tiberiumGrid;


    //Saved as Parent to be compatible with Parent Enumerators
    public HashSet<Thing> AllTiberiumCrystals = new();
    public Dictionary<Region, List<TiberiumCrystal>> TiberiumByRegion = new();

    //Tiberium Access Lists
    public Dictionary<HarvestType, List<TiberiumCrystal>> TiberiumCrystals = new();
    public Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>> TiberiumCrystalsByDef = new();
    public Dictionary<HarvestType, List<TiberiumCrystalDef>> TiberiumCrystalTypes = new();
    public List<TiberiumCrystal> TickList = new();

    public TiberiumMapInfo(Map map) : base(map)
    {
        _tiberiumGrid = new TiberiumGrid(map);
        for (var i = 0; i < 3; i++)
        {
            var type = (HarvestType)i;
            TiberiumCrystals.Add(type, new List<TiberiumCrystal>());
            TiberiumCrystalTypes.Add(type, new List<TiberiumCrystalDef>());
        }
    }

    //Grids
    public TiberiumGrid TiberiumGrid => _tiberiumGrid;

    public int TotalCount => AllTiberiumCrystals.Count;
    public float Coverage => TotalCount / (float)map.Area;

    //TODO: Keep track in a cache
    public TiberiumCrystalDef MostValuableType =>
        TiberiumCrystalTypes[HarvestType.Valuable].MaxBy(t => t.tiberium.harvestValue);


    public override void ExposeData()
    {
        Scribe_Deep.Look(ref _tiberiumGrid, "tiberiumGrid", map);
    }

    public override void Tick()
    {
        if (Find.TickManager.TicksGame % 250 == 0) TiberiumGrid.Tick();
    }

    public override void UpdateOnGUI()
    {
        base.UpdateOnGUI();
    }

    public override void Draw()
    {
        TiberiumGrid.Drawer.RegenerateMesh();
        TiberiumGrid.Drawer.MarkForDraw();
        TiberiumGrid.Drawer.CellBoolDrawerUpdate();
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
        return TiberiumGrid.TiberiumBoolGrid[cell];
    }

    public bool CanGrowFrom(IntVec3 cell)
    {
        return TiberiumGrid.GrowFromGrid[cell];
    }

    public bool CanGrowTo(IntVec3 cell)
    {
        return TiberiumGrid.GrowToGrid[cell] || TiberiumGrid.ForceTo[cell];
    }

    public bool IsAffectedCell(IntVec3 cell)
    {
        return TiberiumGrid.AffectedCells[cell];
    }

    public void SetFieldColor(IntVec3 cell, bool value, TiberiumValueType type)
    {
        //TODO: Check Importance
        //tiberiumGrid.SetFieldColor(cell, value, type);
    }

    //Register new Tiberium crystal in all libraries and map grids
    public void RegisterTiberium(TiberiumCrystal crystal)
    {
        var type = crystal.def.HarvestType;
        if (TiberiumCrystals[type].Contains(crystal)) return;

        AllTiberiumCrystals.Add(crystal); //Add to total crystal list
        TiberiumCrystals[type].Add(crystal); //Add to categorized library
        TiberiumGrid.SetCrystal(crystal); //Register on grid

        if (!TiberiumCrystalTypes[type].Contains(crystal.def)) TiberiumCrystalTypes[type].Add(crystal.def);
        if (TiberiumCrystalsByDef.ContainsKey(crystal.def))
            TiberiumCrystalsByDef[crystal.def].Add(crystal);
        else
            TiberiumCrystalsByDef.Add(crystal.def, new List<TiberiumCrystal> { crystal });
    }

    //Remove crystal from all libraries and clear from grids
    public void DeregisterTiberium(TiberiumCrystal crystal)
    {
        var def = crystal.def;
        AllTiberiumCrystals.Remove(crystal);
        TiberiumGrid.ResetCrystal(crystal.Position);
        TiberiumCrystals[def.HarvestType].Remove(crystal);
        TiberiumCrystalsByDef[def].Remove(crystal);
        if (!TiberiumCrystalTypes.TryGetValue(crystal.def.HarvestType).Any(c => c == crystal.def))
            TiberiumCrystalTypes[def.HarvestType].Remove(crystal.def);
    }
}

/* OLD REF
using System.Collections.Generic;
using TeleCore;
using Verse;

namespace TR;

public class TiberiumMapInfo : MapInformation
{
    //Saved as Parent to be compatible with Parent Enumerators
    public readonly HashSet<Thing> AllTiberiumCrystals = new ();

    //Tiberium Map Library
    public readonly Dictionary<HarvestType, List<TiberiumCrystal>> TiberiumCrystals = new ();
    public readonly Dictionary<HarvestType, List<TiberiumCrystalDef>> TiberiumCrystalTypes = new ();
    public readonly Dictionary<TiberiumCrystalDef, List<TiberiumCrystal>> TiberiumCrystalsByDef = new ();

    //Grids
    private readonly TiberiumGrid tiberiumGrid;

    //
    public TiberiumGrid TiberiumGrid => tiberiumGrid;

    public int TotalCount => AllTiberiumCrystals.Count;
    public float InfestationPercent => TotalCount / (float) map.Area;

    public TiberiumCrystalDef MostValuableType =>
        TiberiumCrystalTypes[HarvestType.Valuable].MaxBy(t => t.tiberium.harvestValue);


    [TweakValue("[TR]TibDrawBool", 0f, 100f)]
    public static bool DrawBool = false;

    public TiberiumMapInfo(Map map) : base(map)
    {
        tiberiumGrid = new TiberiumGrid(map);
        for (int i = 0; i < 3; i++)
        {
            HarvestType type = (HarvestType) i;
            TiberiumCrystals.Add(type, new List<TiberiumCrystal>());
            TiberiumCrystalTypes.Add(type, new List<TiberiumCrystalDef>());
        }
    }

    public override void ExposeDataExtra()
    {
    }

    public override void Tick()
    {
    }

    public override void Update()
    {
        if (DrawBool)
        {
            tiberiumGrid.Drawer.RegenerateMesh();
            tiberiumGrid.Drawer.MarkForDraw();
            tiberiumGrid.Drawer.CellBoolDrawerUpdate();
        }
    }

    public TiberiumCrystal TiberiumAt(IntVec3 cell)
    {
        return tiberiumGrid.TiberiumCrystals[map.cellIndices.CellToIndex(cell)];
    }

    public bool HasTiberiumAt(IntVec3 cell)
    {
        return tiberiumGrid.BoolGrid[cell];
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

    //Register new Tiberium crystal in all libraries and map grids
    public void RegisterTiberium(TiberiumCrystal crystal)
    {
        var type = crystal.def.HarvestType;
        if (TiberiumCrystals[type].Contains(crystal)) return;

        AllTiberiumCrystals.Add(crystal); //Add to total crystal list
        TiberiumCrystals[type].Add(crystal); //Add to categorized library
        tiberiumGrid.SetCrystal(crystal); //Register on grid

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
            TiberiumCrystalsByDef.Add(crystal.def, new List<TiberiumCrystal> {crystal});
        }
    }

    //Remove crystal from all libraries and clear from grids
    public void DeregisterTiberium(TiberiumCrystal crystal)
    {
        var def = crystal.def;
        AllTiberiumCrystals.Remove(crystal);
        tiberiumGrid.ResetCrystal(crystal.Position);
        TiberiumCrystals[def.HarvestType].Remove(crystal);
        TiberiumCrystalsByDef[def].Remove(crystal);
        if (!TiberiumCrystalTypes.TryGetValue(crystal.def.HarvestType).Any(c => c == crystal.def))
        {
            TiberiumCrystalTypes[def.HarvestType].Remove(crystal.def);
        }
    }
}
*/