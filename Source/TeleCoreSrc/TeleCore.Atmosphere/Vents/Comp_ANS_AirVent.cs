using System.Collections.Generic;
using System.Text;
using TeleCore.Events;
using TeleCore.Types;
using Verse;

namespace TeleCore.Atmosphere.Vents;

public class Comp_ANS_AirVent : Comp_ANS_Vent
{
    private FloatControl speedControl;

    /// <summary>
    /// Sus?
    /// </summary>
    public bool CanVent
    {
        get
        {
            return false;
            //TODO: return this[TiberiumDefOf.AtmosphericNetwork].ContainerSet[NetworkRole.Controller].Any(c => !c.Full);
        }
    }

    public override bool FX_ProvidesForLayer(FXArgs args)
    {
        return args.categoryTag == "AirVent";
    }

    public override float? FX_GetRotationSpeedOverride(FXLayerArgs args)
    {
        return args.layerTag switch
        {
            "AirVentRotationLayer" => speedControl.OutputValue,
            _ => null
        };
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        speedControl = new FloatControl(5, 1);
    }

    public override bool CanManipulateNow => speedControl.ReachedPeak;

    public override void CompTick()
    {
        if (CanTickNow)
        {
            speedControl.Start();
            return;
        }
        speedControl.Stop();

        //
        speedControl.Tick();
        base.CompTick();
    }

    public override string CompInspectStringExtra()
    {
        var sb = new StringBuilder(base.CompInspectStringExtra());

        sb.AppendLine($"## Speed Controller ##\n{speedControl}");

        return sb.ToString().Trim();
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
                //Atmospheric.ToggleOverlay();
            }
        };
    }
}

public class CompProperties_ANS_AirVent : CompProperties_ANS_Vent
{
    public CompProperties_ANS_AirVent()
    {
        compClass = typeof(Comp_ANS_AirVent);
    }
}
