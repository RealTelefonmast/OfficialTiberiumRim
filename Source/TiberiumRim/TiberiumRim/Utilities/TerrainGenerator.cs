using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
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
                        w => WeightByDis(root, w, radius, Mathf.Clamp(w.DistanceTo(root) - currentDistance, 0, 2)),
                        out currentCell);
                    if (!currentCell.IsValid || !validator(currentCell)) goto RETRY2;
                    visitedCells.Add(currentCell);
                    action(currentCell);
                }
            }

            return visitedCells;
        }

        private static float WeightByDis(IntVec3 root, IntVec3 current, float radius, float weight)
        {
            float currentDistance = root.DistanceTo(current);
            float densityCurve = Mathf.Lerp(0.7f,0, currentDistance / radius);
            return Mathf.Lerp(weight, 1, densityCurve);
        }

        public static IEnumerable<IntVec3> RandomRootPatch(CellRect root, Map map, float radius, int rootCount, Predicate<IntVec3> validator, Action<IntVec3> action)
        {
            HashSet<IntVec3> visitedCells = new HashSet<IntVec3>();
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
                    lastCell.CellsAdjacent8Way().
                        TryRandomElementByWeight(w => WeightByDis(pusher, w, radius, Mathf.Clamp(w.DistanceTo(pusher) - currentDistance, 0, 2)), out currentCell);
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
