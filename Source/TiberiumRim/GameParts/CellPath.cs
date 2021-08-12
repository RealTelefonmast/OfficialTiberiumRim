using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class CellPath : IExposable
    {
        public Map map;
        //The cell the path originates from
        public IntVec3 origin;
        //The cell the path is growing to
        public IntVec3 puller;
        //The cell the path is growing away from
        public IntVec3 pusher;
        //The length of the path
        public float pathLength;

        private readonly List<IntVec3> pathCells = new List<IntVec3>();

        private readonly Action<IntVec3> processor;
        private readonly Predicate<IntVec3> validator;
        private readonly Predicate<IntVec3> endCondition;

        private IntVec3 currentCell;

        private bool shouldFinish = false;

        public CellPath() { }

        public CellPath(Map map, IntVec3 origin, IntVec3 puller, IntVec3 pusher, float pathLength, Predicate<IntVec3> validator, Predicate<IntVec3> endCondition, Action<IntVec3> processor = null)
        {
            this.map = map;
            this.origin = origin;
            this.puller = puller;
            this.pusher = pusher;
            this.pathLength = pathLength;

            this.validator = validator;
            this.endCondition = endCondition;
            this.processor = processor;

            currentCell = origin;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref currentCell, "currentCell");
        }

        public void CreatePath()
        {
            while (!shouldFinish && !endCondition(currentCell))
            {
                processor(currentCell);
                pathCells.Add(currentCell);

                if (pusher.IsValid)
                    PushNext();
            }
        }

        private void PushNext()
        {
            var cells = currentCell.CellsAdjacent8Way().Where(c => !pathCells.Contains(c) && validator(c));
            if(!cells.Any())
            {
                shouldFinish = true;
                return;
            }
            currentCell = WeightedCellFor(cells);
        }

        private IntVec3 WeightedCellFor(IEnumerable<IntVec3> cells)
        {
            var min = cells.Min(c => c.DistanceTo(pusher));
           // var max = cells.Max(c => c.DistanceTo(pusher));
           //var diff = max - min;
            //var half = diff / 2;

            cells.TryRandomElementByWeight(delegate (IntVec3 t)
            {
                return pusher.DistanceTo(t) - min;
            }, out IntVec3 cell);
            return cell;
        }

        private void PullNext()
        {

        }
    }
}
