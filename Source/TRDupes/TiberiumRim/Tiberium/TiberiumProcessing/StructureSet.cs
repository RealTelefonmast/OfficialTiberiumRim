using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class StructureSet
    {
        public TiberiumNetworkBuilding parent;
        public List<TNW_Pipe> Pipes = new List<TNW_Pipe>();
        public List<TNW_Silo> Silos = new List<TNW_Silo>();
        public List<TNW_Refinery> Refineries = new List<TNW_Refinery>();
        //public List<>

        public List<TiberiumNetworkBuilding> FullList = new List<TiberiumNetworkBuilding>();

        public List<IntVec3> ConnectedCells = new List<IntVec3>();

        public StructureSet() {}

        public StructureSet(TiberiumNetworkBuilding parent)
        {
            this.parent = parent;
        }

        public StructureSet(IEnumerable<IntVec3> cells, Map map, TiberiumNetworkBuilding parent)
        {
            this.parent = parent;
            bool flag = true;
            foreach(IntVec3 cell in cells)
            {
                if (cell.GetFirstBuilding(map) is TiberiumNetworkBuilding tnwb && tnwb.CanConnectTo(cell, parent))
                {
                    if (!FullList.Contains(tnwb))
                    {
                        if (tnwb is TNW_Pipe pipe)
                        {
                            if (parent is TNW_Pipe)
                            {
                                Pipes.Add(pipe);
                            }
                            else if (pipe.DirectParent.DestroyedOrNull())
                            {
                                Pipes.Add(pipe);
                                pipe.UpdateDirectParent(parent, GenAdj.CellsAdjacentCardinal(pipe).Where(c => parent.ConnectableCells.Contains(c)).First());
                                pipe.UpdatePipeMode();
                            }
                            else
                            { flag = false; }
                        }
                        else if(parent is TNW_Pipe p)
                        {
                            p.UpdateDirectParent(tnwb, cell);
                        }
                        if (tnwb is TNW_Silo silo)
                        {
                            Silos.Add(silo);
                        }
                        if (tnwb is TNW_Refinery refinery)
                        {
                            Refineries.Add(refinery);
                        }
                        if (flag)
                        {
                            ConnectedCells.Add(cell);
                            FullList.Add(tnwb);
                            tnwb.StructureSet.AddNewStructure(parent);
                        }
                    }
                }
            }
        }

        public void MergeWith(StructureSet other, TiberiumNetwork newNet = null)
        {
            foreach(TiberiumNetworkBuilding tnwb in other.FullList)
            {
                if (newNet != null)
                {
                    tnwb.Network = newNet;
                }
                AddNewStructure(tnwb);
            }
        }

        public void AddNewStructure(TiberiumNetworkBuilding tnwb, IntVec3 newPos = new IntVec3())
        {
            if (!FullList.Contains(tnwb))
            {
                if (tnwb is TNW_Pipe pipe)
                {
                    Pipes.Add(pipe);
                }
                else if(parent is TNW_Pipe p)
                {
                    if (p.DirectParent.DestroyedOrNull())
                    {
                        p.UpdateDirectParent(tnwb, newPos);
                        p.UpdatePipeMode();
                    }
                }
                if (tnwb is TNW_Silo silo)
                {
                    Silos.Add(silo);
                }
                if (tnwb is TNW_Refinery refinery)
                {
                    Refineries.Add(refinery);
                }
                FullList.Add(tnwb);
            }
        }

        public void RemoveStructure(TiberiumNetworkBuilding tnwb)
        {
            if (FullList.Contains(tnwb))
            {
                if (tnwb is TNW_Pipe pipe)
                {
                    Pipes.Remove(pipe);                   
                }
                if (tnwb is TNW_Silo silo)
                {
                    Silos.Remove(silo);
                }
                if (tnwb is TNW_Refinery refinery)
                {
                    Refineries.Remove(refinery);
                }
                FullList.Remove(tnwb);
            }
        }

        public void RemovingSet(TiberiumNetworkBuilding tnwb)
        {
            foreach(TiberiumNetworkBuilding tnb in FullList)
            {
                tnb.StructureSet.RemoveStructure(tnwb);
                if(tnb is TNW_Pipe pipe && pipe.DirectParent == tnwb)
                {
                    pipe.Notify_DirectParentGone(tnwb);
                    pipe.UpdatePipeMode();
                }
            }
        }

        public void RemoveIO(TNW_Pipe parent)
        {
            for (int i = Pipes.Count - 1; i > -1; i--)
            {
                var pipe = Pipes[i];
                if (pipe.IsIOPipe)
                {
                    Pipes.Remove(pipe);
                    FullList.Remove(pipe);
                    pipe.StructureSet.RemoveStructure(parent);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Pipes: ");
            foreach(TNW_Pipe pipe in Pipes)
            {
                sb.AppendLine("   - " + pipe);
            }
            sb.AppendLine("Silos: ");
            foreach (TNW_Silo silo in Silos)
            {
                sb.AppendLine("   - " + silo);
            }
            sb.AppendLine("Refineries: ");
            foreach (TNW_Refinery refinery in Refineries)
            {
                sb.AppendLine("   - " + refinery);
            }
            return sb.ToString();
        }
    }
}
