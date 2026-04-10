using System;
using System.Collections.Generic;
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
        private readonly Predicate<IntVec3> predicate;

        private IntVec3 currentCell;
        private IntVec3 lastCell;
        private float lastDist;
        private int attempts = 0;
        private bool finished = false;

        public CellPath() { }

        public CellPath(Map map, IntVec3 origin, IntVec3 puller, IntVec3 pusher, float pathLength, Predicate<IntVec3> endCondition, Action<IntVec3> processor = null)
        {
            this.map = map;
            this.origin = origin;
            this.puller = puller;
            this.pusher = pusher;
            this.pathLength = pathLength;
            this.predicate = endCondition;
            this.processor = processor;

            if (pusher.IsValid)
                lastDist = pusher.DistanceTo(origin);

            currentCell = origin;
        }

        public void ExposeData()
        {

        }

        public void Grow(float radius, ref List<IntVec3> cells)
        {
            for (; ; )
            {
                if (lastDist >= radius || attempts > 8) break;
                Grow(ref cells);
            }
        }

        public void Grow(int amount, ref List<IntVec3> cells)
        {
            for (int i = 0; i < amount; i++)
            {
                Grow(ref cells);
            }
        }

        public void Grow(ref List<IntVec3> cells)
        {
            if (pusher.IsValid)
            {
                IntVec3 cell = GrowAway();
                if (cell.IsValid)
                {
                    cells.Add(cell);
                    pathCells.Add(cell);
                }
            }
            else if (puller.IsValid)
            {
                GrowTo();
            }
        }

        private IntVec3 GrowAway()
        {
            float dist = pusher.DistanceTo(currentCell);
            var curDist = lastDist;
            lastDist = dist;

            if ((predicate != null && predicate(currentCell)) || dist >= pathLength)
                return IntVec3.Invalid;

            if (dist >= curDist && currentCell.InBounds(map) && currentCell.Standable(map) && !pathCells.Contains(currentCell))
            {
                lastCell = currentCell;
                currentCell = currentCell.RandomAdjacentCell8Way();
                attempts = 0;
                return lastCell;
            }
            currentCell = lastCell.RandomAdjacentCell8Way();
            attempts++;
            return IntVec3.Invalid;
        }

        private void GrowTo()
        {

        }

        public List<IntVec3> CurrentPath => pathCells;
    }
}
