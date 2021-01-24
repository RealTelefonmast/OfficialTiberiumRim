using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class CompPower_Tiberium : CompPowerPlant
    {
        private CompTNW CompTNW;
        private int powerProductionTicks = 0;

        public PowerProperties TNWProps => (PowerProperties)CompTNW.Props;
        public bool GeneratesPowerNow => powerProductionTicks > 0;

        protected override float DesiredPowerOutput => GeneratesPowerNow ? base.DesiredPowerOutput : 0f;


        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref powerProductionTicks, "powerTicks");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            CompTNW = parent.GetComp<CompTNW>();
        }

        public override void CompTick()
        {
            base.CompTick();
            PowerTick();
        }

        private void PowerTick()
        {
            if (powerProductionTicks <= 0)
            {
                if (CompTNW.Container.TryConsume(TNWProps.consumeAmt))
                    powerProductionTicks = (int)(GenDate.TicksPerDay * TNWProps.daysPerLoad);
            }
            else
                powerProductionTicks--;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.CompInspectStringExtra());
            if(GeneratesPowerNow)
                sb.AppendLine("TR_PowerLeft".Translate(powerProductionTicks.ToStringTicksToPeriod()));
            return sb.ToString().TrimEndNewlines();
        }
    }

    public class PowerProperties : CompProperties_TNW
    {
        public int consumeAmt = 0;
        public float daysPerLoad = 1;
    }
}
