using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public class PortableContainer : FXThing
{
    public TiberiumContainer Container;

    public override float[] OpacityFloats => new float[1] { Container.StoredPct };
    public override Color[] ColorOverrides => new Color[1] { Container.Color };
    public override bool[] DrawBools => new bool[1] { true };

    public void PostSetup(TiberiumContainer container)
    {
        Container = new TiberiumContainer();
        Container.MakeCopy(container);
    }

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
        var sb = new StringBuilder();
        sb.AppendFormat(base.GetInspectString());
        sb.AppendLine("TR_PortableContainer".Translate() + ": " + Container.GetTotalStorage + "/" +
                      Container.maxCapacity);
        return sb.ToString().TrimEndNewlines();
    }
}