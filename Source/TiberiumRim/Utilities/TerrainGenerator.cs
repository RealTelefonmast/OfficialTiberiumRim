using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TR
{
    //This utility class is used to generate various terrain shapes in real time
    public static class TerrainGenerator
    {
        public static IEnumerable<IntVec3> RandomRootPatch(IntVec3 root, Map map, float radius, int rootCount,
            Predicate<IntVec3> validator, Action<IntVec3> action)
        {
            if (rootCount > 8)
                Log.Error("More than 8 roots not possible");

            bool[] growthsDone = new bool[rootCount];
            bool[] rootPositions = new bool[8] {true, true, true, true, true, true, true, true};

            HashSet<IntVec3> visitedCells = new HashSet<IntVec3>();

            IntVec3 currentCell;
            IntVec3 lastCell;

            int currentAttempts;

            for (int i = 0; i < rootCount; i++)
            {
                RETRY:
                int rootPos = Rand.Range(0, 8);
                if (!rootPositions[rootPos]) goto RETRY;

                rootPositions[rootPos] = false;
                currentCell = root + GenAdj.AdjacentCells[rootPos];

                while (!growthsDone[i])
                {
                    currentAttempts = 0;

                    lastCell = currentCell;
                    RETRY2:
                    currentAttempts++;
                    float currentDistance = lastCell.DistanceTo(root);
                    if (currentDistance > radius || currentAttempts > 100)
                    {
                        growthsDone[i] = true;
                        break;
                    }

                    lastCell.CellsAdjacent8Way().Except(visitedCells).TryRandomElementByWeight(
                        w => WeightByDist(root, w, radius, Mathf.Clamp(w.DistanceTo(root) - currentDistance, 0, 2)),
                        out currentCell);
                    if (!currentCell.IsValid || !validator(currentCell)) goto RETRY2;
                    visitedCells.Add(currentCell);
                    action(currentCell);
                }
            }

            return visitedCells;
        }

        private static float WeightByDist(IntVec3 start, IntVec3 end, float radius, float weight)
        {
            float currentDistance = start.DistanceTo(end);
            float densityCurve = Mathf.Lerp(0.7f,0, currentDistance / radius);
            return Mathf.Lerp(weight, 1, densityCurve);
        }

        public static float WeightByPushNPull(IntVec3 checkingCell, IntVec3 lastCell, IntVec3 pusher, IntVec3 puller)
        {
            //TODO: Figure weighting out
            float pushWeight = pusher.IsValid ? checkingCell.DistanceTo(pusher) - lastCell.DistanceTo(pusher) : 0;
            float pullWeight = puller.IsValid ? lastCell.DistanceTo(puller) - checkingCell.DistanceTo(puller) : 0;
            //Log.Message(checkingCell + ": " + (pushWeight * pullWeight) + " = " + pushWeight + " + " + pullWeight);
            return pushWeight + pullWeight;
        }

        //Get Patch 

        //Get Cell
        public static bool GetNextRandomCell(IntVec3 from, IntVec3 pusher, List<IntVec3> visited, Predicate<IntVec3> validator, Action<IntVec3> action, out IntVec3 nextCell)
        {
            //from.CellsAdjacent8Way().TryRandomElementByWeight(adj => WeightByDist(pusher, adj, radius, Mathf.Clamp(adj.DistanceTo(pusher) - currentDistance, 0, 2)), out nextCell);
            from.CellsAdjacent8Way().Where(t => !visited.Contains(t)).TryRandomElement(out nextCell);
            return nextCell.IsValid && validator(nextCell);
        }

        public static IEnumerable<IntVec3> RandomRootPatch(CellRect root, Map map, float radius, int rootCount, Predicate<IntVec3> validator, Action<IntVec3> action)
        {
            List<IntVec3> visitedCells = new List<IntVec3>();
            bool[] doneParts = new bool[root.EdgeCellsCount];

            IntVec3 pusher = root.CenterCell;

            IntVec3 currentCell = IntVec3.Invalid;
            IntVec3 lastCell = IntVec3.Invalid;

            int currentAttempts;

            int i = 0;
            foreach (var rootCell in root.EdgeCells)
            {
                currentCell = rootCell;
                while (!doneParts[i])
                {
                    currentAttempts = 0;

                    lastCell = currentCell;
                    RETRY2:
                    float currentDistance = lastCell.DistanceTo(pusher);
                    if (currentDistance > radius || currentAttempts > 100)
                    {
                        doneParts[i] = true;
                        break;
                    }

                    EXTRACELL:
                    lastCell.CellsAdjacent8Way().TryRandomElementByWeight(w => WeightByDist(pusher, w, radius, Mathf.Clamp(w.DistanceTo(pusher) - currentDistance, 0, 2)), out currentCell);
                    if (!currentCell.IsValid || !validator(currentCell))
                    {
                        currentAttempts++;
                        goto RETRY2;
                    }
                    if (!visitedCells.Contains(currentCell))
                    {
                        visitedCells.Add(currentCell);
                        if(Rand.Chance(1f - (currentCell.DistanceTo(pusher)/radius)))
                            goto EXTRACELL;
                    }
                    action(currentCell);
                }
                i++;
            }

            return visitedCells;
        }
    }
}
