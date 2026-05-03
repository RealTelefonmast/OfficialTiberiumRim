using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using TerrainDef = Verse.TerrainDef;
using ThingDef = Verse.ThingDef;

namespace TiberiumRim;

public class PositionFilter
{
    public List<ThingValue> distanceToThings = new();
    public AreaCheck homeArea = AreaCheck.Avoid;
    public AreaCheck roofs = AreaCheck.Avoid;
    public List<ThingDef> spawnAt = new();
    public List<TerrainDef> terrainToAvoid = new();
    public List<TerrainFloat> terrainToPrefer = new();

    public bool IsDefault => terrainToAvoid.NullOrEmpty() && terrainToPrefer.NullOrEmpty() && spawnAt.NullOrEmpty() &&
                             distanceToThings.NullOrEmpty() && roofs == AreaCheck.Avoid && homeArea == AreaCheck.Avoid;

    public IntVec3 FindCell(Map map, List<ThingValue> rewards = null)
    {
        var cell = IntVec3.Invalid;
        cell = FindCells(map, 1, rewards).FirstOrDefault();
        return cell;
    }

    public List<IntVec3> FindCells(Map map, int count, List<ThingValue> rewards = null, List<ThingDef> things = null)
    {
        var spawnPositions = new List<IntVec3>();
        var minRoomSize = 0;
        if (!rewards.NullOrEmpty())
            foreach (var tv in rewards)
                minRoomSize += (int)Math.Round(tv.value / (double)tv.ThingDef.stackLimit, 0,
                    MidpointRounding.AwayFromZero);

        var AllCells = map.AllCells.Where(c => !c.Fogged(map) && c.Standable(map)).ToList();

        if (minRoomSize > 0f) AllCells.RemoveAll(v => v.GetRoom(map)?.CellCount < minRoomSize);
        if (!terrainToAvoid.NullOrEmpty()) AllCells.RemoveAll(v => terrainToAvoid.Contains(v.GetTerrain(map)));
        if (!terrainToPrefer.NullOrEmpty())
            AllCells.RemoveAll(v => !Rand.Chance(terrainToPrefer.Find(t => t.terrainDef == v.GetTerrain(map)).value));
        if (!spawnAt.NullOrEmpty())
            AllCells.RemoveAll(v => v.GetThingList(map)?.Any(t => !spawnAt.Contains(t.def)) ?? true);
        if (!distanceToThings.NullOrEmpty())
            AllCells.RemoveAll(v => distanceToThings.All(tv =>
                map.listerThings.ThingsOfDef(tv.ThingDef).Any(t => v.DistanceTo(t.Position) < tv.value)));
        if (homeArea == AreaCheck.Avoid) AllCells.RemoveAll(v => map.areaManager.Home[v]);
        if (homeArea == AreaCheck.Prefer) AllCells.RemoveAll(v => !map.areaManager.Home[v]);
        if (roofs == AreaCheck.Avoid) AllCells.RemoveAll(v => v.Roofed(map));
        if (roofs == AreaCheck.Prefer) AllCells.RemoveAll(v => !v.Roofed(map));
        if (!things.NullOrEmpty()) AllCells.RemoveAll(v => Enumerable.Any(things, t => !t.ThingFitsAt(map, v)));
        var i = 0;
        var failsafe = 0;
        while (0 < count && failsafe < count * 10)
        {
            i++;
            failsafe++;
            AllCells.TryRandomElement(out var cell);
            spawnPositions.Add(cell);
        }

        return spawnPositions;
    }
}

public enum AreaCheck
{
    Prefer,
    Avoid,
    Allow
}