using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_Connector : Building
    {
        public Building_TNC Network;

        public Building Parent;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                foreach (IntVec3 c in this.Position.CellsAdjacent8Way())
                {
                    List<Thing> thingList = c.GetThingList(map);
                    foreach (Thing thing in thingList)
                    {
                        Building building = thing as Building;
                        if (building?.TryGetComp<Comp_TNW>() != null)
                        {
                            if (building != null)
                            {
                                this.Parent = building;
                                if (building.TryGetComp<Comp_TNW>().Connector == null)
                                {
                                    this.Parent.TryGetComp<Comp_TNW>().Connector = this;
                                    return;
                                }
                            }
                        }
                    }
                }
                if(Parent == null)
                {
                    base.Destroy();
                }
            }
        }

        public Comp_GraphicExtra GraphicComp
        {
            get
            {
                Comp_GraphicExtra gc = Parent.TryGetComp<Comp_GraphicExtra>();
                if (gc != null)
                {
                    return gc;
                }
                return null;
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            this.Parent.TryGetComp<Comp_TNW>().Connector = null;
            Network.Connectors.Remove(this);
            base.Destroy(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.Parent, "Parent");
            Scribe_References.Look(ref this.Network, "Network");
        }

        public Graphic Notify_GraphicsUpdated(Graphic graphic)
        {
            Graphic output = graphic;
            base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Buildings, true, false);
            return output;
        }

        public override void Draw()
        {
            if ((Find.Selector.IsSelected(this) || Find.Selector.IsSelected(Network)) && Parent != null)
            {
                this.Graphic.GetColoredVersion(Graphic.Shader, Color.green, Color.green).DrawFromDef(NewDrawPos, this.Parent.Rotation, this.def);
                return;
            }
            this.Graphic.DrawFromDef(NewDrawPos, this.Parent.Rotation, this.def);
        }

        public override void Print(SectionLayer layer)
        {
            return;
        }

        public Vector3 NewDrawPos
        {
            get
            {
                if (Parent != null)
                {
                    Vector3 exactPos = new Vector3
                    {
                        x = Parent.DrawPos.x,
                        z = Parent.DrawPos.z,
                        y = Altitudes.AltitudeFor(AltitudeLayer.BuildingOnTop)
                    };
                    return exactPos;
                }
                return this.DrawPos;
            }
        }

        public override Graphic Graphic
        {
            get
            {
                if (GraphicComp != null)
                {
                    if(GraphicComp.OverlayGraphic == null)
                    {
                        GraphicComp.SetOverlayGraphic();
                    }
                    return Notify_GraphicsUpdated(GraphicComp.OverlayGraphic);
                }
                return Notify_GraphicsUpdated(base.Graphic);
            }
        }
    }
}
