using System.Collections.Generic;
using System.Text;
using TeleCore.RWExtended.ThingClasses;
using UnityEngine;
using Verse;

namespace TR.TiberiumProcessing;

public class PortableContainer : FXThing
{
    public TiberiumContainer Container;

        public void PostSetup(TiberiumContainer container)
        {
            Container = container.MakeCopy(this);
        }

        public override float[] OpacityFloats => new float[1] { Container?.StoredPercent ?? 0f };
        public override Color[] ColorOverrides => new Color[1] { Container?.Color ?? Color.white };
        public override bool[] DrawBools => new bool[1] { true };

    public void PostSetup(TiberiumContainer container)
    {
        Container = container.MakeCopy(this);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref Container, "tibContainer");
    }

    public override void DrawGUIOverlay()
    {
        base.DrawGUIOverlay();
        if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
        {
            Vector3 v = GenMapUI.LabelDrawPosFor(Position);
            GenMapUI.DrawThingLabel(v, Container.StoredPercent.ToStringPercent(), Color.white);
        }
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(base.GetInspectString());
        sb.AppendLine("TR_PortableContainer".Translate() + ": " + Container.TotalStorage + "/" + Container.capacity);
        return sb.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;
        foreach (var g in Container.GetGizmos()) yield return g;
    }
}