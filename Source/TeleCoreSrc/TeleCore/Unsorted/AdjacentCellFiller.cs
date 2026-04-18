using System;
using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

//TODO: Fill out for tiberium cell filling
public static class AdjacentCellFiller
{
    private static FastPriorityQueue<IntVec3> tempWorkableCells;
    private static readonly List<IntVec3> tempFilledCells = new();

    public static void FillAdjacentCellsAround(IntVec3 root, Verse.Map map, int cellCount, Action<IntVec3> fillAction,
        Predicate<IntVec3> fillValidator, Predicate<IntVec3> cellToSkipValidator)
    {
        if (cellCount > 0 && fillValidator(root))
        {
            fillAction(root);
            cellCount--;
        }

        if (cellCount <= 0) return;

        tempWorkableCells =
            new FastPriorityQueue<IntVec3>(new RandomAdjacentCellComparer(root, map, cellToSkipValidator));

        //Sellect all cells
        map.floodFiller.FloodFill(root, cellToSkipValidator, delegate (IntVec3 x) { tempFilledCells.Add(x); });

        if (tempFilledCells.Count == 0) return;

        //
        tempWorkableCells.Clear();
        for (var i = 0; i < tempFilledCells.Count; i++)
            foreach (var adjacentCell in GetAdjacentFillableCells(tempFilledCells[i], map))
                if (!tempWorkableCells.Contains(adjacentCell))
                    tempWorkableCells.Push(adjacentCell);

        tempFilledCells.Clear();
        while (cellCount > 0 && tempWorkableCells.Count > 0)
        {
            var intVec = tempWorkableCells.Pop();
            fillAction.Invoke(intVec);
            foreach (var adjacentCell in GetAdjacentFillableCells(intVec, map))
                if (!tempWorkableCells.Contains(adjacentCell))
                    tempWorkableCells.Push(adjacentCell);
            cellCount--;
        }

        //
        IEnumerable<IntVec3> GetAdjacentFillableCells(IntVec3 c, Verse.Map m)
        {
            for (var i = 0; i < GenAdj.CardinalDirections.Length; i++)
            {
                var adjacentCell = c + GenAdj.CardinalDirections[i];
                if (adjacentCell.InBounds(map) && fillValidator(adjacentCell)) yield return adjacentCell;
            }
        }
    }
}