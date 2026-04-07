using System.Linq;
using UnityEngine;
using Verse;

namespace TR.Components;

public class MapComponent_TiberiumWater : MapComponent, ICellBoolGiver
{
    private readonly CellBoolDrawer drawer;

    public BoolGrid landableCells;
    public BoolGrid riverCells;
    public BoolGrid waterCells;

    public MapComponent_TiberiumWater(Map map) : base(map)
    {
        waterCells = new BoolGrid(map);
        riverCells = new BoolGrid(map);
        landableCells = new BoolGrid(map);

        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z);
    }

    public bool GetCellBool(int index)
    {
        return waterCells[index] || landableCells[index];
    }

    public Color GetCellExtraColor(int index)
    {
        if (landableCells[index])
            return Color.green;
        if (riverCells[index])
            return Color.cyan;
        if (waterCells[index])
            return Color.blue;
        return Color.clear;
    }

    public Color Color => Color.white;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref waterCells, "waterCells");
        Scribe_Deep.Look(ref riverCells, "riverCells");
        Scribe_Deep.Look(ref landableCells, "landableCells");
    }

    public override void MapGenerated()
    {
        base.MapGenerated();
        foreach (var waterCell in map.AllCells.Where(c => c.GetTerrain(map).IsWater)) waterCells[waterCell] = true;

        foreach (var riverCell in waterCells.ActiveCells.Where(c => c.GetTerrain(map).IsRiver))
            riverCells[riverCell] = true;

        // map.AllCells.Where( c =>  riverCells.Any( r => r.DistanceTo(c) >= 2 && r.DisanceTo(c) <= 6 ));
        var activeCells = riverCells.ActiveCells.ToList();
        foreach (var landableCell in map.AllCells.Where(c =>
                     !landableCells[c] && !riverCells[c] && activeCells.All(r => c.DistanceTo(r) >= 3.9f) &&
                     activeCells.Any(r => c.DistanceTo(r) <= 9.9f))) landableCells[landableCell] = true;
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();
        if (MapComponent_Tiberium.DrawBool && Find.CurrentMap == map)
        {
            drawer.RegenerateMesh();
            drawer.MarkForDraw();
            drawer.CellBoolDrawerUpdate();
        }
    }
}