using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.FlowCore.Bills;

public class Bill_Production_Network : Bill_Production
{
    public RecipeDef_Network def;
    public bool isBeingDone;

    public Bill_Production_Network(RecipeDef_Network def) : base(def)
    {
        this.def = def;
    }

    public Bill_Production_Network()
    {
    }

    public Comp_NetworkBillsCrafter CompTNW => ((Building)billStack.billGiver).GetComp<Comp_NetworkBillsCrafter>();

    public bool BaseShouldDo => base.ShouldDoNow();

    public Color BillColor
    {
        get
        {
            var color = Color.white;
            foreach (var valueDef in def.networkCost.Cost.AcceptedValueTypes) color *= valueDef.valueColor;

            return color;
        }
    }
    //public NetworkComponent ParentTibComp => CompTNW[TiberiumDefOf.TiberiumNetwork];
    //private Network Network => ParentTibComp.Network;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref isBeingDone, "isBeingDone");
        Scribe_Defs.Look(ref def, "props");
    }

    public override void Notify_DoBillStarted(Pawn billDoer)
    {
        base.Notify_DoBillStarted(billDoer);
        isBeingDone = true;
    }

    public override void Notify_PawnDidWork(Pawn p)
    {
        //Log.Message("Notify Pawn Did Work");
        base.Notify_PawnDidWork(p);
    }

    public override bool ShouldDoNow()
    {
        if (base.ShouldDoNow())
            if (CompTNW is { IsPowered: true })
                return def.networkCost.CanPayWith((CompNetwork)CompTNW);
        //if (Network != null && Network.IsWorking)
        return false;
    }

    public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
    {
        if (def.networkCost.CanPayWith((CompNetwork)CompTNW))
        {
            def.networkCost.DoPayWith(CompTNW);
            isBeingDone = false;
            base.Notify_IterationCompleted(billDoer, ingredients);
        }
    }

    public override string ToString()
    {
        return base.ToString();
    }
}