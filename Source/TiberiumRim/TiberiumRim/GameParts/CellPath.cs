using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private IntVec3 lastCell;
        private float currentDistance;
        private float lastDistance;

        private int attempts = 0;

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
            Scribe_Values.Look(ref lastCell, "lastCell");
            Scribe_Values.Look(ref attempts, "attempts");
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

        /*
        public bool ShouldFinish()
        {
            return origin.DistanceTo(currentCell) > pathLength || attempts > 8 || (endCondition != null && endCondition(currentCell));
        }

        public void GrowFully(ref List<IntVec3> cells)
        {
            for (;;)
            {
                if (ShouldFinish()) break;
                Grow(ref cells);
            }
        }

        public void Grow(float radius, ref List<IntVec3> cells)
        {
            for (; ; )
            {
                if (radius < origin.DistanceTo(currentCell) || attempts > 8) break;
                Grow(ref cells);
            }
        }

        public void Grow(ref List<IntVec3> cells) 
        {
            attempts++;
            if (pusher.IsValid)
            {
                IntVec3 cell = GrowAway();
                if (!cell.IsValid) return;

                attempts = 0;
                cells.Add(cell);
                pathCells.Add(cell);
                processor?.Invoke(cell);
            }
            else if (puller.IsValid)
            {
                GrowTo();
                attempts = 0;
            }
        }

        private IntVec3 GrowAway()
        {
            if (currentCell.InBounds(map) && currentCell.Standable(map) && !pathCells.Contains(currentCell))
            {
                lastCell = currentCell;
                currentCell = currentCell.RandomAdjacentCell8Way();
                return lastCell;
            }
            currentCell = lastCell.RandomAdjacentCell8Way();
            return IntVec3.Invalid;
        }

        private void GrowTo()
        {

        }

        public List<IntVec3> CurrentPath => pathCells;
        */
    }
}
