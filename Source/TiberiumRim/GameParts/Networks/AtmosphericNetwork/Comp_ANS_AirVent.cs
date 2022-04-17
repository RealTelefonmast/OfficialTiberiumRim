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
        private FCSimple speedControl;

        public override float?[] AnimationSpeeds => new float?[4] { null, null, speedControl.OutputValue, null };

        public CompProperties_ANS_AirVent Props => (CompProperties_ANS_AirVent)base.props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            speedControl = new FCSimple(5, 1);
        }

        public override void CompTick()
        {
            base.CompTick();
            speedControl.Tick();
            if (!Atmospheric.IsOutdoors)
            {
                if (Atmospheric.ActualValue > 0 && !AtmosphericComp.Container.CapacityFull)
                {
                    speedControl.Start();
                    if (speedControl.ReachedPeak)
                    {
                        _ = ManipulatePollution(1);
                    }
                    return;
                }
                speedControl.Stop();
            }
        }

        private bool ManipulatePollution(int tick)
        {
            if (!IsPowered) return false;

            int totalThroughput = Props.gasThroughPut * tick;
            switch (Props.ventMode)
            {
                case AtmosphericVentMode.Intake:
                    if (AtmosphericComp.Container.CapacityFull) return false;
                    if (Atmospheric.UsedContainer.Container.TryTransferTo(AtmosphericComp.Container, TiberiumDefOf.TibPollution, totalThroughput))
                    {
                        return true;
                    }

                    break;
                case AtmosphericVentMode.Output:
                    if (AtmosphericComp.Container.Empty) return false;
                    if (AtmosphericComp.Container.TryTransferTo(Atmospheric.UsedContainer.Container, TiberiumDefOf.TibPollution, totalThroughput))
                    {
                        return true;
                    }

                    break;
                case AtmosphericVentMode.Dynamic:
                    break;
            }
            return false;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {
                defaultLabel = "Toggle Overlay",
                action = delegate
                {
                    Atmospheric.ToggleOverlay();
                }
            };
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
