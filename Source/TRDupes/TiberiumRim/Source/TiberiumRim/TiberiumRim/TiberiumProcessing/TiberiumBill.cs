using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class TiberiumBill : Bill_Production
    {
        public RecipeDef_Tiberium def;
        public bool isBeingDone = false;

        public TiberiumBill(RecipeDef_Tiberium def) : base(def as RecipeDef)
        {
            this.def = def;
        }

        public TiberiumBill() : base() { }

        private CompTNW_Crafter CompTNW => ((Building) billStack.billGiver).GetComp<CompTNW_Crafter>();
        private TiberiumNetwork Network => CompTNW.Network;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isBeingDone, "isBeingDone");
            Scribe_Defs.Look(ref def, "def");
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
            {
                if (CompTNW != null)
                {
                    if (Network != null && Network.IsWorking)
                    {
                        return CanPay();
                    }
                }
            }
            return false;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            if (CanPay())
            {
                Pay();
                isBeingDone = false;
                base.Notify_IterationCompleted(billDoer, ingredients);
            }
        }

        public Color BillColor
        {
            get
            {
                Color color = Color.white;
                foreach(TiberiumValueType type in def.tiberiumCost.AcceptedTypes)
                {
                    color *= TRUtils.ColorForType(type);
                }
                return color;
            }
        }

        private bool CanPay()
        {
            var cost = def.tiberiumCost;
            var anyCost = cost.cost;
            var totalCost = cost.TotalCost;
            var types = cost.AcceptedTypes;
            var specTypes = cost.SpecificCosts;
            if (specTypes.Any())
            {
                foreach (var pair in specTypes)
                {
                    if (Network.NetworkValueFor(pair.Key) >= pair.Value)
                    {
                        totalCost -= pair.Value;
                    }
                }
                if ((totalCost - anyCost) != 0)
                {
                    return false;
                }
            }
            if(anyCost > 0)
            {
                if (Network.NetworkValueFor(types.ToList()) >= anyCost)
                {
                    totalCost -= anyCost;
                }
            }
            return totalCost == 0;
        }

        private void Pay()
        {
            var totalCost = def.tiberiumCost.TotalCost;
            var silos = Network.NetworkSet.Silos.ToArray();
            foreach (CompTNW_Silo silo in silos)
            {
                if (totalCost <= 0)
                { return; }

                foreach (var spec in def.tiberiumCost.SpecificCosts)
                {
                    if (silo.Container.TryConsume(spec.Key, spec.Value))
                    {
                        totalCost -= spec.Value;
                    }
                }
                foreach(var type in def.tiberiumCost.AcceptedTypes)
                {
                    if(silo.Container.TryRemoveValue(type, totalCost, out float leftOver))
                    {
                        totalCost = leftOver;
                    }
                }
            }
        }
    }
}
