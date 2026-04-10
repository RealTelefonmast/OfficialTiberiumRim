using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Building_TNC : Building
    {
        public List<Building_Connector> Connectors = new List<Building_Connector>();

        public List<CellRect> Positions = new List<CellRect>();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Connectors, "Connectors", LookMode.Reference);
        }

        public override void DeSpawn()
        {
            foreach(Building_Connector c in Connectors)
            {
                if (!c.Destroyed)
                {
                    c.Destroy(DestroyMode.Deconstruct);
                }
            }
            Connectors.Clear();
            base.DeSpawn();
        }

        public List<Comp_TNW> AllStorages
        {
            get
            {
                List<Comp_TNW> compList = new List<Comp_TNW>();
                foreach (Building_Connector b in Connectors)
                {
                    if (b != null)
                    {
                        Comp_TNW comp = b.Parent.GetComp<Comp_TNW>();
                        if (comp != null)
                        {
                            if (comp.props.isGlobalStorage || comp.props.isLocalStorage)
                            { compList.Add(comp); }
                        }
                    }
                }
                return compList;
            }
        }

        public List<Comp_TNW> AllGlobalStorages
        {
            get
            {
                List<Comp_TNW> compList = new List<Comp_TNW>();
                foreach(Building_Connector b in Connectors)
                {
                    if (b != null)
                    {
                        Comp_TNW comp = b.Parent.GetComp<Comp_TNW>();
                        if (comp != null)
                        {
                            if (comp.props.isGlobalStorage)
                            { compList.Add(comp); }
                        }
                    }
                }
                return compList;
            }
        }

        public float TotalStoredTiberium
        {
            get
            {
                float storage = 0f;            
                for(int i = 0; i < Connectors.Count; i++) 
                {                   
                    if (Connectors[i] != null)
                    {
                        Building_Connector b = Connectors[i];
                        Comp_TNW comp = b.Parent.GetComp<Comp_TNW>();
                        if (comp != null)
                        {
                            if (comp.Container.GetTotalStorage > 0)
                            {
                                storage += comp.Container.GetTotalStorage;
                            }
                        }
                    }
                }
                return storage;
            }
        }

        public List<TiberiumType> ContainedTiberiumTypes
        {
            get
            {
                List<TiberiumType> types = new List<TiberiumType>();
                foreach(Building_Connector connector in Connectors)
                {
                    List<TiberiumType> types2 = connector.Parent.GetComp<Comp_TNW>().Container.GetTypes;
                    foreach(TiberiumType type in types2)
                    {
                        if (!types.Contains(type))
                        {
                            types.Add(type);
                        }
                    }
                }
                return types;
            }
        }
        
        public float TotalStoredForType(TiberiumType type)
        {
            float flt = 0f;
            for (int i = 0; i < Connectors.Count; i++)
            {                
                if (Connectors[i] != null)
                {
                    Building_Connector b = Connectors[i];
                    Comp_TNW comp = b.Parent.GetComp<Comp_TNW>();
                    if (comp != null)
                    {
                        if (comp.Container.GetTotalStorage > 0)
                        {
                            flt += comp.Container.ValueForType(type);
                        }
                    }
                }
            }
            return flt;
        }

        public override void Tick()
        {
            base.Tick();
            if (Positions.Count > 0)
            {
                for (int i = 0; i < Positions.Count; i++)
                {
                    CellRect cells = Positions[i];
                    Building building = cells.GetBuilding(DefDatabase<ThingDef>.GetNamed("Connector_TNW"), this.Map);
                    if (building != null)
                    {
                        Building_Connector connector = building as Building_Connector;
                        this.Connectors.Add(connector);
                        connector.Network = this;
                        this.Positions.Remove(cells);
                    }
                }
            }      
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            Designator_BuildWithParent TibConneeeeect = new Designator_BuildWithParent(DefDatabase<ThingDef>.GetNamed("Connector_TNW"), this)
            {
            };
            yield return TibConneeeeect;
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            stringBuilder.AppendLine("TotalStoredTib".Translate()+": " + TotalStoredTiberium);
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }
}
