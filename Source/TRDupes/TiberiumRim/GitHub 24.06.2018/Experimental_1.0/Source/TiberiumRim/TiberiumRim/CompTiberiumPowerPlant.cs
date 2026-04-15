using System;
using RimWorld;

namespace TiberiumRim
{
    public class CompTiberiumPowerPlant : CompPowerTrader
    {
        protected Comp_TNW NetworkComp;

        protected CompBreakdownable breakdownableComp;

        protected virtual float DesiredPowerOutput
        {
            get
            {
                return -base.Props.basePowerConsumption;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.NetworkComp = this.parent.GetComp<Comp_TNW>();
            this.breakdownableComp = this.parent.GetComp<CompBreakdownable>();
            if (base.Props.basePowerConsumption < 0f && !this.parent.IsBrokenDown() && FlickUtility.WantsToBeOn(this.parent))
            {
                base.PowerOn = true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            this.UpdateDesiredPowerOutput();
        }

        public void UpdateDesiredPowerOutput()
        {
            if ((this.breakdownableComp != null && this.breakdownableComp.BrokenDown) || (this.NetworkComp != null && !this.NetworkComp.IsGeneratingPower) || (this.flickableComp != null && !this.flickableComp.SwitchIsOn) || !base.PowerOn)
            {
                base.PowerOutput = 0f;
            }
            else
            {
                base.PowerOutput = this.DesiredPowerOutput;
            }
        }
    }
}
