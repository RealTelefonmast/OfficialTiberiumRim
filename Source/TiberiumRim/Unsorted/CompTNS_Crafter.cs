using RimWorld;
using UnityEngine;
using Verse;

namespace TR.Networks.TiberiumNetwork;

public class CompTNS_Crafter : Comp_TiberiumNetworkStructure
{
    public TiberiumBillStack billStack;
    public new Building_WorkTable parent;

    public bool IsWorkedOn => billStack?.CurrentBill != null;

    public TiberiumBillStack BillStack => billStack;

    public Color CurColor => Color.clear;

    public override bool? FX_ShouldDraw(FXLayerArgs args)
    {
        return args.index switch
        {
            0 => IsWorkedOn,
            _ => base.FX_ShouldDraw(args)
        };
    }

    public override bool? FX_ShouldThrowEffects(FXEffecterArgs args)
    {
        return IsWorkedOn;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        parent = base.parent as Building_WorkTable;
        if (!respawningAfterLoad)
            billStack = new TiberiumBillStack(this);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref billStack, "tiberiumBillStack", this);
    }
}