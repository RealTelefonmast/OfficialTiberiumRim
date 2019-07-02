using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class TiberiumFloodInfo
    {
        private Map map;
        private int cellAmount = -1;
        private Predicate<IntVec3> validator;
        private FloodFiller FloodFiller;
        private Action<IntVec3> FloodFillAction;

        public TiberiumFloodInfo() { }

        public TiberiumFloodInfo(Map map, int count, Predicate<IntVec3> validator, Action<IntVec3> Action)
        {
            this.map = map;
            cellAmount = count;
            this.validator = validator;
            FloodFillAction = Action;
            FloodFiller = new FloodFiller(map);
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
                FloodFillAction(cell);
            }
            /*
            //Step Two - THICCening      
            for (int i = 0; i < Positions.Count; i++)
            {
                IntVec3 cell = Positions[i];
                for (int ii = 0; ii < 2; ii++)
                {
                    int iii = TRUtils.Chance(0.5f) ? 1 : -1;
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
            */
        }

        public void MakeFlood()
        {

        }

        public bool TryMakeFlood(out List<IntVec3> floodedCells, CellRect rect, bool ignoreCount = true, int maxTries = 9999)
        {
            if (!GetFloodCells(rect, cellAmount, out floodedCells, maxTries) && !ignoreCount)
                return false;

            if (FloodFillAction != null)
            {
                foreach (IntVec3 cell in floodedCells)
                    FloodFillAction(cell);
            }
            return true;
        }

        public void MakeRoom(IntVec3 sourceCell)
        {
            var curCell = sourceCell;
            int cells = 0;
            while (cells < cellAmount)
            {
                Room room = curCell.GetRoom(map);
                cells = room.CellCount;
                if (room.CellCount < cellAmount)
                {
                    Building building = room.BorderCells.RandomElement().GetFirstBuilding(map);
                    if (building != null)
                        building.Destroy();
                }
            }
        }

        private bool GetFloodCells(CellRect Rect, int MaxCells, out List<IntVec3> final, int maxTries = 9999)
        {
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
}
