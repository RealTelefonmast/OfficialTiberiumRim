using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Building_TiberiumPowerPlant : Building_GraphicSwitchable
    {
        public Comp_TNW Network
        {
            get
            {
                return this.GetComp<Comp_TNW>();
            }
        }

        public override void Tick()
        {
            CompFX FX = this.TryGetComp<CompFX>();
            FX.ShouldDoEffectNow = this.TryGetComp<Comp_TNW>().Container.StoredPct > 0f;
            base.Tick();
        }

        public override bool IsActivated => false;
    }
}
