using RimWorld;
using UnityEngine;

namespace TiberiumRim;

public class CompTNW_Crafter : CompTNW
{
    public new Building_WorkTable parent;

    public bool IsWorkedOn => CurBill != null;

    public TiberiumBill CurBill
    {
        get { return (TiberiumBill)parent.billStack.Bills.Find(b => b is TiberiumBill tb && tb.isBeingDone); }
    }

    public Color CurColor => CurBill?.BillColor ?? Color.clear;

    public override Color[] ColorOverrides => new[] { CurColor, Color.white, Color.white };
    public override float[] OpacityFloats => new[] { 1f, 1f, 1f };
    public override bool[] DrawBools => new[] { IsWorkedOn, base.DrawBools[1], true };
    public override bool ShouldDoEffecters => IsWorkedOn;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        parent = base.parent as Building_WorkTable;
    }
}