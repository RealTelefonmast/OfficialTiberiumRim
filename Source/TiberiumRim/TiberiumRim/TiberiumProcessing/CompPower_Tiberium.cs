using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class CompPower_Tiberium : CompPowerTrader 
    {
        private CompTNW CompTNW;
        private CompBreakdownable CompBreakdownable;
        private int powerProductionTicks = 0;

        public PowerProperties TNWProps => (PowerProperties)CompTNW.Props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref powerProductionTicks, "powerTicks");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            CompTNW = parent.GetComp<CompTNW>();
            CompBreakdownable = parent.GetComp<CompBreakdownable>();
        }

        public override void CompTick()
        {
            base.CompTick();
            PowerTick();
            UpdateOutput();
        }

        private void PowerTick()
        {
            if (powerProductionTicks <= 0)
            {
                if (CompTNW.Container.TryConsume(TNWProps.consumeAmt))
                {
                    powerProductionTicks = (int)(GenDate.TicksPerDay * TNWProps.daysPerLoad);
                }
            }
            else
            {
                powerProductionTicks--;
            }
        }

        public bool GeneratesPowerNow
        {
            get
            {
                return powerProductionTicks > 0;
            }
        }

        private void UpdateOutput()
        {
            if((CompBreakdownable?.BrokenDown ?? false) || (!flickableComp?.SwitchIsOn ?? false) || !GeneratesPowerNow || !PowerOn)
            {
                PowerOutput = 0f;
            }
            else
            {
                PowerOutput = -Props.basePowerConsumption;
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.CompInspectStringExtra().TrimStart().TrimEndNewlines());
            sb.AppendLine("Power left for: " +  GenDate.ToStringTicksToPeriod(powerProductionTicks));
            return sb.ToString().TrimEndNewlines();
        }
    }

    public class PowerProperties : CompProperties_TNW
    {
        public int consumeAmt = 0;
        public float daysPerLoad = 1;
    }
}
