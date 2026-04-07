using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TeleCore.Utility;

public static class TeleFlooder
{
    private static readonly HashSet<IntVec3> _PassedCells = new();
    private static readonly Queue<IntVec3> _OpenSet = new();

    public static IEnumerable<IntVec3> TryMakeConnection(IntVec3 start, IntVec3 end, Action<IntVec3> processor)
    {
        _PassedCells.Clear();
        var current = start;
        while (current.DistanceTo(end) > 1)
        {
            yield return current;
            processor?.Invoke(current);
            _PassedCells.Add(current);

            var adj = current.CellsAdjacent8Way();
            var Min = adj.Min(c => c.DistanceTo(end));
            current = adj.ToList().Find(c => c.DistanceTo(end) <= Min);
            var Max = adj.Max(c => c.DistanceTo(end));
            var extra = adj.ToList().Find(x => x.DistanceTo(end) >= Max);
            if (!_PassedCells.Contains(extra))
            {
                yield return current;
                processor?.Invoke(current);
                _PassedCells.Add(extra);
            }
        }
    }

    public static IEnumerable<IntVec3> Flood(Verse.Map map, IntVec3 originCell, Action<IntVec3> processor,
        Predicate<IntVec3> validator, int maxCells = 9999)
    {
        return Flood(map, new CellRect(originCell.x, originCell.z, 1, 1), processor, validator, maxCells);
    }

    /// <summary>
    ///     Floods an area from an initial <see cref="CellRect" />, processing and validating cells as it goes.
    /// </summary>
    public static IEnumerable<IntVec3> Flood(Verse.Map map, CellRect originRect, Action<IntVec3> processor,
        Predicate<IntVec3> validator, int maxCells = 9999)
    {
        var num = -1;
        _PassedCells.Clear();
        _OpenSet.Clear();
        foreach (var cell in originRect.Cells) _OpenSet.Enqueue(cell);
        while (_OpenSet.Count > 0)
        {
            num++;
            if (num >= maxCells)
                break;
            var curCell = _OpenSet.Dequeue();
            if (_PassedCells.Contains(curCell)) continue;

            yield return curCell;
            processor?.Invoke(curCell);
            _PassedCells.Add(curCell);

            foreach (var adjCell in curCell.CellsAdjacent8Way()
                         .Where(c => c.InBounds(map) && !_PassedCells.Contains(c) && validator.Invoke(c))
                         .InRandomOrder()) _OpenSet.Enqueue(adjCell);
        }
    }
}