using RimWorld;
using UnityEngine;

namespace TeleCore.FlowCore;

public class Comp_Network_Valve : CompNetwork
{
    private const float _valveOpenState = 0;
    private const float _valveClosedState = 66;
    private const int _ticksToTurn = 120;
    private const float radsPerTick = _valveClosedState / _ticksToTurn;
    private float _curState;

    private CompFlickable Flick { get; set; }
    protected override bool IsWorkingOverride => Flick.SwitchIsOn;

    public override Color? FX_GetColor(FXLayerArgs args)
    {
        if (args.layerTag == "Valve") return Color.white;
        return base.FX_GetColor(args);
    }

    public override float? FX_GetRotation(FXLayerArgs args)
    {
        if (args.layerTag == "Valve") return _curState;
        return null;
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        Flick = parent.TryGetComp<CompFlickable>();
    }

    internal override void TeleTick()
    {
        if (Flick.SwitchIsOn)
        {
            if (_curState > _valveOpenState)
                _curState -= radsPerTick;
        }
        else
        {
            if (_curState < _valveClosedState) _curState += radsPerTick;
        }
    }

    public override void ReceiveCompSignal(string signal)
    {
        var flicked = signal is KnownCompSignals.FlickedOff or KnownCompSignals.FlickedOn;
        if (flicked)
        {
            foreach (var part in NetworkParts)
            {
                part.SetPassThrough(signal is KnownCompSignals.FlickedOff ? 0f : 1f);
                part.Network.System.Notify_PassThroughChanged(part);
            }
        }
    }
}