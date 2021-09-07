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
        private FloatControl speedControl;

        public override float?[] AnimationSpeeds => new float?[4] { null, null, speedControl.CurrentValue, null };

        public CompProperties_ANS_AirVent Props => (CompProperties_ANS_AirVent)base.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            speedControl = new FloatControl(0.5f, 0, 10);
        }

        public override void CompTick()
        {
            base.CompTick();
            speedControl.Tick();
            if (ManipulatePollution(1))
            {
                speedControl.Start();
                return;
            }
            speedControl.Stop();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (ManipulatePollution(GenTicks.TickRareInterval))
            {
                speedControl.Start();
                return;
            }
            speedControl.Stop();
        }

        private bool ManipulatePollution(int tick)
        {
            if (!IsPowered) return false;

            int totalThroughput = Props.gasThroughPut * tick;
            switch (Props.ventMode)
            {
                case AtmosphericVentMode.Intake:
                    if (AtmosphericComp.Container.CapacityFull) return false;
                    if (Pollution.TryRemovePollution(totalThroughput, out int actuallyRemoved))
                    {
                        AtmosphericComp.Container.TryAddValue(TiberiumDefOf.TibPollution, actuallyRemoved, out _);
                        return true;
                    }

                    break;
                case AtmosphericVentMode.Output:
                    if (AtmosphericComp.Container.Empty) return false;
                    if (AtmosphericComp.Container.TryRemoveValue(TiberiumDefOf.TibPollution, totalThroughput, out float actualValue))
                    {
                        Pollution.TryAddPollution((int) actualValue, out _);
                        return true;
                    }

                    break;
                case AtmosphericVentMode.Dynamic:
                    break;
            }
            return false;
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
