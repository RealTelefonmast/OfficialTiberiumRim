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
            Container = container.MakeCopy();
        }

        public override float[] OpacityFloats => new float[1] { Container.StoredPercent };
        public override Color[] ColorOverrides => new Color[1] { Container.Color };
        public override bool[] DrawBools => new bool[1] { true };

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && TiberiumRimSettings.settings.ShowNetworkValues)
            {
                Vector3 v = GenMapUI.LabelDrawPosFor(Position);
                GenMapUI.DrawThingLabel(v, Container.StoredPercent.ToStringPercent(), Color.white);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("TR_PortableContainer".Translate() + ": " + Container.TotalStorage + "/" + Container.capacity);
            return sb.ToString().TrimEndNewlines();
        }
    }
}
