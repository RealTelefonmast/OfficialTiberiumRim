using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Building_TiberiumCrafter : Building_GraphicSwitchable
    {
        public Building_TiberiumCrafter() : base()
        {
            this.billStack = new BillStack(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void Tick()
        {
            Comp_GraphicExtra GraphicComp = this.TryGetComp<Comp_GraphicExtra>();
            CompFX FX = this.TryGetComp<CompFX>();
            GraphicComp.SpecialPct = (IsActive ? 1f : 0f);
            FX.ShouldDoEffectNow = IsActive;
            base.Tick();
        }

        private bool IsActive
        {
            get
            {
                
                return this.billStack.Bills.Find((Bill x) => (x as TibBill).isBeingDone) != null;
            }
        }

        public override bool IsActivated => false;
    }
}
