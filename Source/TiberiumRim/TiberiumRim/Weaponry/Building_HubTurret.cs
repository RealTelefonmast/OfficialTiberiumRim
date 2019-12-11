using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_HubTurret : Building_TRTurret
    {
        public Building_TurretHub parentHub;

        public override CompRefuelable RefuelComp => parentHub.RefuelComp;
        public override CompPowerTrader PowerComp => parentHub.PowerComp;
        public override CompMannable MannableComp => parentHub.MannableComp;
        public override StunHandler Stunner => parentHub.Stunner;
        public override CompPower ForcedPowerComp => PowerComp;

        public bool NeedsRepair => false;


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            ConnectToParent();
        }

        public void ConnectToParent()
        {
            var numCells = GenRadial.NumCellsInRadius(10);
            for (int i = 1; i < numCells; i++)
            {
                IntVec3 cell = GenRadial.RadialPattern[i] + Position;
                Building_TurretHub hub = (Building_TurretHub)cell.GetFirstThing(Map, def.turret.hub.hubDef);
                if (hub != null && hub.NeedsTurrets)
                {
                    hub.AddHubTurret(this);
                    return;
                }
            }
        }

        public override void Print(SectionLayer layer)
        {
            base.Print(layer);
            PrintTurretCable(layer, this, parentHub);
        }

        private void PrintTurretCable(SectionLayer layer, Thing A, Thing B)
        {
            Material mat = TiberiumContent.TurretCable;
            float y = AltitudeLayer.SmallWire.AltitudeFor();
            Vector3 center = (A.TrueCenter() + B.TrueCenter()) / 2f;
            center.y = y;
            Vector3 v = B.TrueCenter() - A.TrueCenter();
            Vector2 size = new Vector2(1.5f, v.MagnitudeHorizontal());
            float rot = v.AngleFlat();
            Printer_Plane.PrintPlane(layer, center, size, mat, rot, false, null, null, 0.01f, 0f);
        }
    }
}
