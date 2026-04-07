using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using TR.TiberiumObjects;
using UnityEngine;
using Verse;
using ICellBoolGiver = Verse.ICellBoolGiver;
using IntVec3 = Verse.IntVec3;

namespace TR.Info;

public class TiberiumWaterInfo : MapInformation, ICellBoolGiver
{
    [TweakValue("MapComponent_ShowWater")] public static bool DrawBool = false;

    private readonly List<IntVec3> dirtyCells = new();

    private readonly CellBoolDrawer drawer;
    public BoolGrid corruption;

    public IntGrid corruptionInt;

    //
    public BoolGrid lakeCells;
    public BoolGrid landableCells;
    public BoolGrid riverCells;
    public BoolGrid waterCells;

    public TiberiumWaterInfo(Map map) : base(map)
    {
        waterCells = new BoolGrid(map);
        lakeCells = new BoolGrid(map);
        riverCells = new BoolGrid(map);
        landableCells = new BoolGrid(map);

        corruption = new BoolGrid(map);
        corruptionInt = new IntGrid(map);
        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z);
    }

    public bool GetCellBool(int index)
    {
        return waterCells[index] || landableCells[index];
    }

    public Color GetCellExtraColor(int index)
    {
        if (corruption[index])
            return Color.magenta;
        if (landableCells[index])
            return Color.green;
        if (riverCells[index])
            return Color.cyan;
        if (waterCells[index])
            return Color.blue;
        return Color.clear;
    }

    public Color Color => Color.white;

    public override void ExposeDataExtra()
    {
        Scribe_Deep.Look(ref waterCells, "waterCells");
        Scribe_Deep.Look(ref lakeCells, "lakeCells");
        Scribe_Deep.Look(ref riverCells, "riverCells");
        Scribe_Deep.Look(ref landableCells, "landableCells");
    }

    public override void InfoInit(bool initAfterReload = false)
    {
        base.InfoInit(initAfterReload);
        LongEventHandler.QueueLongEvent(delegate
        {
            foreach (var waterCell in map.AllCells.Where(c => c.GetTerrain(map).IsWater)) waterCells[waterCell] = true;

            foreach (var riverCell in waterCells.ActiveCells.Where(c => c.GetTerrain(map).IsRiver))
                riverCells[riverCell] = true;

            foreach (var lakeCell in waterCells.ActiveCells.Where(c =>
                         !c.GetTerrain(map).IsRiver && !c.GetTerrain(map).defName.Contains("Ocean")))
                lakeCells[lakeCell] = true;

            var activeCells = riverCells.ActiveCells.ToList();
            foreach (var landableCell in map.AllCells.Where(c =>
                         !landableCells[c] && !riverCells[c] && activeCells.All(r => c.DistanceTo(r) >= 3.9f) &&
                         activeCells.Any(r => c.DistanceTo(r) <= 9.9f))) landableCells[landableCell] = true;
        }, "SettingWaterData", false, null);
    }

    public override void Tick()
    {
    }

    public override void Update()
    {
        if (DrawBool && Find.CurrentMap == map)
        {
            drawer.RegenerateMesh();
            drawer.MarkForDraw();
            drawer.CellBoolDrawerUpdate();
        }
    }

    public bool IsLake(IntVec3 cell)
    {
        return lakeCells[cell];
    }

    public void Notify_TibSpawned(TiberiumCrystal crystal)
    {
        if (!riverCells[crystal.Position]) return;
        corruption[crystal.Position] = true;
        dirtyCells.Add(crystal.Position);
    }

    private void UpdateCorruption()
    {
        if (!Enumerable.Any(dirtyCells)) return;

        for (var i = dirtyCells.Count - 1; i >= 0; i--)
        {
            var pos = dirtyCells[i];
            CorruptCell(pos);
            foreach (var intVec3 in SelectFlowCellsFrom(pos))
            {
                if (dirtyCells.Contains(intVec3)) continue;
                dirtyCells.Add(intVec3);
            }

            dirtyCells.RemoveAt(i);
        }
    }

    private IEnumerable<IntVec3> SelectFlowCellsFrom(IntVec3 pos)
    {
        var waterInfo = map.waterInfo;
        var movementAt = waterInfo.GetWaterMovement(pos.ToVector3());

        foreach (var c in pos.CellsAdjacent8Way())
        {
            if (!riverCells[c]) continue;

            var posDiff = (pos - c).ToVector3();
            var xDiff = movementAt.x - posDiff.x;
            var yDiff = movementAt.y - posDiff.y;

            Log.Message("Trying: " + movementAt + " - " + posDiff + " |x: " + xDiff + " |y: " + yDiff);
            if (xDiff >= 0 && yDiff >= 0)
                yield return c;
        }
    }

    private void CorruptCell(IntVec3 c)
    {
        corruption[c] = true;
        map.terrainGrid.SetTerrain(c, DefDatabase<TerrainDef>.GetNamed("TiberiumShallowWater"));
    }
}