using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class PortableContainer : FXThing
    {
        public TiberiumContainer Container;

        public void PostSetup(TiberiumContainer container)
        {
            Container = new TiberiumContainer();
            Container.MakeCopy(container);
        }

        public override float[] OpacityFloats => new float[1] { Container.StoredPct };
        public override Color[] ColorOverrides => new Color[1] { Container.Color };
        public override bool[] DrawBools => new bool[1] { true };

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && TiberiumRimSettings.settings.ShowNetworkValues)
            {
                Vector3 v = GenMapUI.LabelDrawPosFor(Position);
                GenMapUI.DrawThingLabel(v, Container.StoredPct.ToStringPercent(), Color.white);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("TR_PortableContainer".Translate() + ": " + Container.GetTotalStorage + "/" + Container.maxCapacity);
            return sb.ToString().TrimEndNewlines();
        }
    }
}
