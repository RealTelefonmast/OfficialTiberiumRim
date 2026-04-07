using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore
{
    public class Bill_Production_Network : Bill_Production
    {
        public RecipeDef_Network def;
        public bool isBeingDone = false;

        public Bill_Production_Network(RecipeDef_Network def) : base(def as RecipeDef)
        {
            this.def = def;
        }

        public Bill_Production_Network() : base() { }

        public Comp_NetworkBillsCrafter CompTNW => ((Building)billStack.billGiver).GetComp<Comp_NetworkBillsCrafter>();
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

        public bool BaseShouldDo => base.ShouldDoNow();

        public override bool ShouldDoNow()
        {
            if (base.ShouldDoNow())
            {
                if (CompTNW is { IsPowered: true })
                {
                    return def.networkCost.CanPayWith(CompTNW);
                    //if (Network != null && Network.IsWorking)
                }
            }
            return false;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            if (def.networkCost.CanPayWith(CompTNW))
            {
                def.networkCost.DoPayWith(CompTNW);
                isBeingDone = false;
                base.Notify_IterationCompleted(billDoer, ingredients);
            }
        }

        public Color BillColor
        {
            get
            {
                Color color = Color.white;
                foreach (NetworkValueDef valueDef in def.networkCost.Cost.AcceptedValueTypes)
                {
                    color *= valueDef.valueColor;
                }
                return color;
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
