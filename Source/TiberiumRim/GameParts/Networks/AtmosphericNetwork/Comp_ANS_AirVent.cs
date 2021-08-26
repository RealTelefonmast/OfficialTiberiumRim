using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public enum AtmosphericVentMode
    {
        Intake,
        Output,
        Dynamic
    }

    public class Comp_ANS_AirVent : Comp_AtmosphericNetworkStructure
    {
        public CompProperties_ANS_AirVent Props => (CompProperties_ANS_AirVent)base.props;
        public override void CompTick()
        {
            base.CompTick();
            ManipulatePollution(1);
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            ManipulatePollution(GenTicks.TickRareInterval);
        }

        private void ManipulatePollution(int tick)
        {
            if (!IsPowered || AtmosphericComp.Container.CapacityFull) return;
            int totalThroughput = Props.gasThroughPut * tick;
            switch (Props.ventMode)
            {
                case AtmosphericVentMode.Intake:
                    if(Pollution.TryRemovePollution(totalThroughput, out int actuallyRemoved))
                        AtmosphericComp.Container.TryAddValue(TiberiumDefOf.TibPollution, actuallyRemoved, out _);
                    break;
                case AtmosphericVentMode.Output:
                    if (AtmosphericComp.Container.TryRemoveValue(TiberiumDefOf.TibPollution, totalThroughput, out float actualValue)) 
                        Pollution.TryAddPollution((int)actualValue, out _);
                    
                    break;
                case AtmosphericVentMode.Dynamic:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class CompProperties_ANS_AirVent : CompProperties_ANS
    {
        public AtmosphericVentMode ventMode = AtmosphericVentMode.Intake;
        public int gasThroughPut = 1;

        public CompProperties_ANS_AirVent()
        {
            compClass = typeof(Comp_ANS_AirVent);
        }
    }
}
