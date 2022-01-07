using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_TurretHub : Building_TRTurret
    {
        public List<Building_HubTurret> hubTurrets = new List<Building_HubTurret>();

        //Anticipated Connections
        private readonly List<IntVec3> anticipatingPositions = new List<IntVec3>();

        public List<Thing> AnticipatedBlueprintsOrFrames
        {
            get
            {
                List<Thing> things = new List<Thing>();
                for (var i = anticipatingPositions.Count - 1; i >= 0; i--)
                {
                    var position = anticipatingPositions[i];
                    var thing = position.GetThingList(Map).Find(t =>
                           (t is Blueprint_Build b && b.def.entityDefToBuild is TRThingDef trDef && trDef.turret.hub.hubDef == this.def)
                        || (t is Frame f && f.def.entityDefToBuild is TRThingDef trDef2 && trDef2.turret.hub.hubDef == this.def));
                    if (thing != null)
                    {
                        things.Add(thing);
                    }
                    else
                    {
                        anticipatingPositions.Remove(position);
                    }
                }
                return things;
            }
        }

        public void Upgrade_AddTurret()
        {

        }

        public void AnticipateTurretAt(IntVec3 pos)
        {
            anticipatingPositions.Add(pos);
        }

        public void AddHubTurret(Building_HubTurret t)
        {
            if (hubTurrets.Contains(t)) return;
            hubTurrets.Add(t);
            t.parentHub = this;
            anticipatingPositions.Remove(t.Position);
        }


        public void RemoveHubTurret(Building_HubTurret turret)
        {
            hubTurrets.Remove(turret);
        }
        
        public Building_HubTurret DestroyedChild => hubTurrets.First(c => c.NeedsRepair);

        public bool AcceptsTurrets => hubTurrets.Count + AnticipatedBlueprintsOrFrames.Count < def.turret.hub.maxTurrets;

        public override void Draw()
        {
            base.Draw();
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
            foreach (var turret in hubTurrets)
            {
                PrintTurretCable(layer, this, turret);
            }
        }


        private void PrintTurretCable(SectionLayer layer, Thing A, Thing B)
        {
            Material mat = MaterialPool.MatFrom(def.turret.hub.cableTexturePath);
            float y = AltitudeLayer.SmallWire.AltitudeFor();
            Vector3 center = (A.TrueCenter() + B.TrueCenter()) / 2f;
            center.y = y;
            Vector3 v = B.TrueCenter() - A.TrueCenter();
            Vector2 size = new Vector2(1.5f, v.MagnitudeHorizontal());
            float rot = v.AngleFlat();
            Printer_Plane.PrintPlane(layer, center, size, mat, rot, false, null, null, 0.01f, 0f);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.AppendLine("Anticipated Connections: " + AnticipatedBlueprintsOrFrames.Count);
            return sb.ToString().TrimEnd();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            yield return StaticData.GetDesignatorFor<Designator_Build>(def); //new Designator_BuildFixed(def);

        }
    }
}
