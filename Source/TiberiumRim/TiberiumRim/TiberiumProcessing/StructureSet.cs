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
        public CompTNW parent;
        public List<CompTNW_Pipe> Pipes = new List<CompTNW_Pipe>();
        public List<CompTNW_Silo> Silos = new List<CompTNW_Silo>();
        public List<CompTNW_Refinery> Refineries = new List<CompTNW_Refinery>();
        public List<CompTNW_Crafter> Crafters = new List<CompTNW_Crafter>();

        public List<CompTNW> FullList = new List<CompTNW>();
        public List<CompTNW> Storages = new List<CompTNW>();
        public List<CompTNW> Consumers = new List<CompTNW>();
        public List<CompTNW> Producers = new List<CompTNW>();

        public StructureSet() {}

        public StructureSet(CompTNW parent)
        {
            this.parent = parent;
        }

        public void AddNewStructure(CompTNW tnwb)
        {
            if (!FullList.Contains(tnwb) && tnwb != null)
            {
                if (tnwb is CompTNW_Pipe pipe)
                {
                    Pipes.Add(pipe);
                }
                if (tnwb is CompTNW_Silo silo)
                {
                    Silos.Add(silo);
                }
                if (tnwb is CompTNW_Refinery refinery)
                {
                    Refineries.Add(refinery);
                }
                if (tnwb is CompTNW_Crafter crafter)
                {
                    Crafters.Add(crafter);
                }
                FullList.Add(tnwb);
                switch (tnwb.NetworkMode)
                {
                    case TNWMode.None:
                        break;
                    case TNWMode.Storage:
                        Storages.Add(tnwb);
                        break;
                    case TNWMode.Consumer:
                        Storages.Add(tnwb);
                        Consumers.Add(tnwb);
                        break;
                    case TNWMode.Producer:
                        Producers.Add(tnwb);
                        break;
                }
            }
        }

        public void AddNewStructure(CompTNW tnwb, IntVec3 cell)
        {
            if (!FullList.Contains(tnwb) && tnwb != null)
            {
                parent?.StructureSetOnAdd(tnwb, cell);
                AddNewStructure(tnwb);
                tnwb.StructureSet.AddNewStructure(parent, cell + parent?.parent.Position.PositionOffset(cell) ?? IntVec3.Invalid);
            }
        }

        public void RemoveStructure(CompTNW tnwb)
        {
            if (FullList.Contains(tnwb))
            {
                parent?.StructureSetOnRemove(tnwb);
                if (tnwb is CompTNW_Pipe pipe)
                {
                    Pipes.Remove(pipe);                   
                }
                if (tnwb is CompTNW_Silo silo)
                {
                    Silos.Remove(silo);
                }
                if (tnwb is CompTNW_Refinery refinery)
                {
                    Refineries.Remove(refinery);
                }
                FullList.Remove(tnwb);
            }
        }

        public void ParentDestroyed()
        {
            foreach(CompTNW tnw in FullList)
            {
                tnw.StructureSet.RemoveStructure(parent);
            }
        }

        public bool Empty
        {
            get
            {
                return FullList.NullOrEmpty();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Pipes: ");
            foreach(CompTNW_Pipe pipe in Pipes)
            {
                sb.AppendLine("   - " + pipe.parent);
            }
            sb.AppendLine("Silos: ");
            foreach (CompTNW_Silo silo in Silos)
            {
                sb.AppendLine("   - " + silo.parent);
            }
            sb.AppendLine("Refineries: ");
            foreach (CompTNW_Refinery refinery in Refineries)
            {
                sb.AppendLine("   - " + refinery.parent);
            }
            sb.AppendLine("Total Count: " + FullList.Count);
            return sb.ToString();
        }
    }
}
