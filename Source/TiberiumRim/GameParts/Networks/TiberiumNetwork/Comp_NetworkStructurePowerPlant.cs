using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Comp_NetworkStructurePowerPlant : CompPowerPlant
    {
        private int powerProductionTicks = 0;
        private Comp_NetworkStructure compNetworkStructure;

        public new CompProperties_NetworkStructurePowerPlant Props => (CompProperties_NetworkStructurePowerPlant)compNetworkStructure.Props;

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
            compNetworkStructure = parent.GetComp<Comp_NetworkStructure>();
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
                if (compNetworkStructure.Container.TryConsume(Props.consumeAmt))
                    powerProductionTicks = (int)(GenDate.TicksPerDay * Props.daysPerLoad);
            }
            else
                powerProductionTicks--;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.CompInspectStringExtra());
            if (GeneratesPowerNow)
                sb.AppendLine("TR_PowerLeft".Translate(powerProductionTicks.ToStringTicksToPeriod()));
            return sb.ToString().TrimEndNewlines();
        }
    }

    public class CompProperties_NetworkStructurePowerPlant : CompProperties_NetworkStructure
    {
        public int consumeAmt = 0;
        public float daysPerLoad = 1;
    }
}
