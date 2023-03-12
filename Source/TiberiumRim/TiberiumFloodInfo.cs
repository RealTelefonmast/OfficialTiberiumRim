namespace TiberiumRim
{
    /*
    public class TiberiumFloodInfo
    {
        private readonly Map map;
        private readonly Predicate<IntVec3> validator;
        private readonly Action<IntVec3> processor;
        private BoolGrid floodBools;

        public TiberiumFloodInfo()
        {
        }

        public TiberiumFloodInfo(Map map, Predicate<IntVec3> validator, Action<IntVec3> Action)
        {
            this.map = map;
            this.validator = validator;
            processor = Action;
            floodBools = new BoolGrid(map);
        }

        public void TryMakeConnection(out List<IntVec3> cells, IntVec3 start, IntVec3 end)
        {
            //Step One - Line
            List<IntVec3> Positions = new List<IntVec3>();
            IntVec3 current = start;
            while (current.DistanceTo(end) > 1)
            {
                Positions.Add(current);
                var adj = current.CellsAdjacent8Way();
                float Min = adj.Min(c => c.DistanceTo(end));
                current = adj.ToList().Find(c => c.DistanceTo(end) == Min);
                float Max = adj.Max(c => c.DistanceTo(end));
                IntVec3 extra = adj.ToList().Find(x => x.DistanceTo(end) == Max);
                if(!Positions.Contains(extra))
                    Positions.Add(extra);
            }
            cells = Positions;
            //Step Three - Iterate
            foreach (IntVec3 cell in Positions)
            {
                processor(cell);
            }
            
            //Step Two - THICCening      
            for (int i = 0; i < Positions.Count; i++)
            {
                IntVec3 cell = Positions[i];
                for (int ii = 0; ii < 2; ii++)
                {
                    int iii = TRandom.Chance(0.5f) ? 1 : -1;
                    IntVec3 vec1 = cell + new IntVec3(iii, 0, 0);
                    if (!Positions.Contains(vec1))
                    {
                        Positions.Add(vec1);
                    }
                    else
                    {
                        vec1 = cell + new IntVec3(iii * 2, 0, 0);
                        Positions.Add(vec1);
                    }
                    IntVec3 vec2 = cell + new IntVec3(0, 0, iii);
                    if (!Positions.Contains(vec2))
                    {
                        Positions.Add(vec2);
                    }
                    else
                    {
                        vec2 = cell + new IntVec3(0, 0, iii * 2);
                        Positions.Add(vec1);
                    }
                }
            }
            cells = Positions;

            //Step Three - Iterate
            foreach (IntVec3 cell in Positions)
            {
                FloodFillAction(cell);
            }
            
        }

        public void MakeFlood()
        {

        }

        public bool TryMakeFlood(out List<IntVec3> floodedCells, IntVec3 root, int maxTries = 9999)
        {
            floodedCells = Flood(root, maxTries);
            return true;
        }

        public bool TryMakeFlood(out List<IntVec3> floodedCells, CellRect rect, int maxTries = 9999)
        {
            floodedCells = Flood(rect, maxTries);
            return true;
        }

        public void MakeRoom(IntVec3 sourceCell, int roomSize)
        {
            var curCell = sourceCell;
            int cells = 0;
            while (cells < roomSize)
            {
                Room room = curCell.GetRoom(map);
                cells = room.CellCount;
                if (room.CellCount < roomSize)
                {
                    Building building = room.BorderCells.RandomElement().GetFirstBuilding(map);
                    building?.Destroy();
                }
            }
        }

        private readonly Queue<IntVec3> openSet = new Queue<IntVec3>();

        private List<IntVec3> Flood(IntVec3 root, int maxTries = 9999)
        {
            return Flood(new CellRect(root.x, root.z, 1, 1), maxTries);
        }

        private List<IntVec3> Flood(CellRect rect, int maxTries = 9999)
        {
            List<IntVec3> flood = new List<IntVec3>();
            int num = 0;
            openSet.Clear();
            foreach (var cell in rect.Cells)
            {
               openSet.Enqueue(cell);
            }
            while (openSet.Count > 0)
            {
                if (num >= maxTries)
                    break;
                IntVec3 curCell = openSet.Dequeue();
                if (floodBools[curCell]) continue;

                flood.Add(curCell);
                floodBools[curCell] = true;
                processor?.Invoke(curCell);

                foreach (var adjCell in curCell.CellsAdjacent8Way().Where(c => c.InBounds(map) && !floodBools[c] && validator(c)).InRandomOrder())
                {
                    if(TRandom.RandValue > 0.4)
                        openSet.Enqueue(adjCell);
                }
                num++;
            }
            return flood;
        }

        private bool GetFloodCells(CellRect Rect, int MaxCells, out List<IntVec3> final, int maxTries = 9999)
        {
            telef
            final = new List<IntVec3>();
            List<IntVec3> Flood = new List<IntVec3>();
            Flood.AddRange(Rect.Cells);
            int count = MaxCells + Rect.Cells.Count();
            int tries = 0;
            while (Flood.Count < count)
            {
                tries++;
                if (tries == maxTries)
                    break;
                var Cells = Flood.Where(c => c.CellsAdjacent8Way().Any(d => !Flood.Contains(d))).InRandomOrder();
                if (Cells.Any())
                {
                    foreach (IntVec3 cell in Cells)
                    {
                        if (Flood.Count >= count) break;
                        var Cells2 = cell.CellsAdjacent8Way().Where(c => !Flood.Contains(c) && validator(c));
                        if (Cells2.Any())
                            Flood.Add(Cells2.RandomElement());
                    }
                }
                else
                    return false;
            }
            final = Flood;
            return true;
        }
    }
    */
}
