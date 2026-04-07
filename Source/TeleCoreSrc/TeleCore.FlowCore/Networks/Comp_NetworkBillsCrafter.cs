using RimWorld;
using TeleCore.FlowCore.Bills;
using UnityEngine;
using Verse;

namespace TeleCore.FlowCore;

public class Comp_NetworkBillsCrafter : CompNetwork
{
    public NetworkBillStack billStack;
    public new Building_WorkTable parent;

    //CompFX
    public Color CurColor => Color.clear;

    //Crafter Code
    public new CompProperties_NetworkBillsCrafter Props => (CompProperties_NetworkBillsCrafter)base.Props;

    public bool IsWorkedOn => BillStack.CurrentBill != null;
    public NetworkBillStack BillStack => billStack;

    public override bool? FX_ShouldDraw(FXLayerArgs args)
    {
        return args.index switch
        {
            0 => IsWorkedOn,
            _ => base.FX_ShouldDraw(args)
        };
    }

    public override Color? FX_GetColor(FXLayerArgs args)
    {
        return args.index switch
        {
            0 => CurColor,
            _ => base.FX_GetColor(args)
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
            billStack = new NetworkBillStack(this);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Deep.Look(ref billStack, "tiberiumBillStack", this);
    }
}