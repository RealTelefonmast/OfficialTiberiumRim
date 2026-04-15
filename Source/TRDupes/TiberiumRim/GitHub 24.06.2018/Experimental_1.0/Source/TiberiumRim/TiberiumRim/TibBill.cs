using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class TibBill : Bill_Production
    {
        public bool isBeingDone = false;
        public RecipeDef_Tiberium def;

        public TibBill(RecipeDef_Tiberium def) : base(def as RecipeDef)
        {
            this.def = def;
        }

        public TibBill() : base()
        {
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref isBeingDone, "isBeingDone");
            Scribe_Defs.Look(ref def, "def");
            base.ExposeData();
        }

        public override void Notify_DoBillStarted(Pawn billDoer)
        {
            this.isBeingDone = true;
            base.Notify_DoBillStarted(billDoer);
        }

        public bool HasDrainType
        {
            get
            {
                return this.def.drainType != TiberiumType.None;
            }
        }

        private float RecipeCost
        {
            get
            {
                return (this.recipe as RecipeDef_Tiberium).tiberiumCost;
            }
        }

        public override bool ShouldDoNow()
        {
            Building b = this.billStack.billGiver as Building;
            Comp_TNW comp = b.GetComp<Comp_TNW>();
            if (comp != null)
            {
                Building_TNC network = comp.Connector?.Network;
                if (network != null && comp.NetworkIsPowered)
                {
                    if (network.TotalStoredTiberium >= RecipeCost)
                    {
                        if (HasDrainType)
                        {
                            if (network.ContainedTiberiumTypes.Contains(this.def.drainType))
                            {
                                if (network.TotalStoredForType(this.def.drainType) < RecipeCost)
                                {
                                    return false;
                                }                 
                            }
                            else { return false; }
                        }
                        else 
                        if (network.TotalStoredTiberium - network.TotalStoredForType(TiberiumType.Sludge) < RecipeCost)
                        {
                            return false; 
                        }
                        if (base.ShouldDoNow())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            if(this.def.tiberiumCost == 0)
            {
                isBeingDone = false;
                base.Notify_IterationCompleted(billDoer, ingredients);
                return;
            }
            Building_TiberiumCrafter crafter = null;
            foreach (IntVec3 c in billDoer.CellsAdjacent8WayAndInside())
            {
                Thing thing = c.GetFirstBuilding(billDoer.Map);
                if (thing != null)
                {
                    if (thing is Building_TiberiumCrafter)
                    {
                        crafter = thing as Building_TiberiumCrafter;
                    }
                }
            }
            Comp_TNW comp = crafter.TryGetComp<Comp_TNW>()?.Connector?.Network?.AllStorages.Find((Comp_TNW x) => HasDrainType ? x.Container.ValueForType(def.drainType) >= RecipeCost : x.Container.GetTotalStorage - x.Container.ValueForType(TiberiumType.Sludge) >= RecipeCost);
            TiberiumType type = comp.Container.MainType;
            if (HasDrainType)
            {
                type = def.drainType;
            }
            List<TiberiumType> types = comp.Container.GetTypes.Where((TiberiumType x) => HasDrainType ? x == def.drainType : x != TiberiumType.Sludge).ToList();

            float priceLeft = 0f;
            priceLeft += RecipeCost;
            foreach (TiberiumType type2 in types)
            {
                if (priceLeft > 0)
                {
                    float value = comp.Container.ValueForType(type2) >= priceLeft ? priceLeft : comp.Container.ValueForType(type2);
                    comp.Container.RemoveCrystal(type2, value);
                    priceLeft -= value;
                }
                if(priceLeft <= 0)
                {
                    isBeingDone = false;
                    base.Notify_IterationCompleted(billDoer, ingredients);
                }
            }     
        }
    }
}
