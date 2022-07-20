using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using TeleCore;
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
        private RoomComponent_AirLock airlockComp;

        public CompProperties_ANS_AirVent Props => (CompProperties_ANS_AirVent)base.props;

        public bool CanVent
        {
            get
            {
                return this[TiberiumDefOf.AtmosphericNetwork].ContainerSet[NetworkRole.Controller].Any(c => !c.Full);
            }
        }

        public override float? FX_GetRotationSpeedAt(int index)
        {
            return index switch
            {
                2 => speedControl.OutputValue,
                _ => null
            };
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            speedControl = new FloatControl(5, 1);
        }

        private bool CanWork
        {
            get
            {
                if (!IsPowered) return false;
                switch (Props.ventMode)
                {
                    case AtmosphericVentMode.Intake:
                        if (Atmospheric.ActualValue <= 0) return false;
                        if (AtmosphericComp.Container.Full) return false;
                        break;
                    case AtmosphericVentMode.Output:
                        if (Atmospheric.Saturation >= 1) return false;
                        if (AtmosphericComp.Container.Empty) return false;
                        break;
                    case AtmosphericVentMode.Dynamic:
                        break;
                }
                return true;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            speedControl.Tick();
            if (!Atmospheric.IsOutdoors)
            {
                if (CanWork)
                {
                    speedControl.Start();
                    if(speedControl.ReachedPeak)
                        _ = ManipulatePollution(1);
                    return;
                }
                speedControl.Stop();
            }
        }

        private bool ManipulatePollution(int tick)
        {
            int totalThroughput = Props.gasThroughPut * tick;
            switch (Props.ventMode)
            {
                case AtmosphericVentMode.Intake:
                    if (Atmospheric.UsedContainer.Container.TryTransferTo(AtmosphericComp.Container, TiberiumDefOf.TibPollution, totalThroughput))
                    {
                        return true;
                    }
                    break;
                case AtmosphericVentMode.Output:
                    if (AtmosphericComp.Container.TryConsume(TiberiumDefOf.TibPollution, totalThroughput))
                    {
                        parent.Map.Tiberium().AtmosphericInfo.TrySpawnGasAt(parent.Position, ThingDef.Named("Gas_TiberiumGas"), totalThroughput * 100);
                        return true;
                    }

                    break;
                case AtmosphericVentMode.Dynamic:
                    break;
            }
            return false;
        }

        public override string CompInspectStringExtra()
        {

            return base.CompInspectStringExtra();
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

        public void SetAirLock(RoomComponent_AirLock roomComponentAirLock)
        {
            airlockComp = roomComponentAirLock;
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
