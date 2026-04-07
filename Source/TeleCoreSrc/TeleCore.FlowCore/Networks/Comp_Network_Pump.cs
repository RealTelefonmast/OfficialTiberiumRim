using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace TeleCore.FlowCore;

public class Comp_Network_Pump : CompNetwork
{
    //TODO: Handle multi in-output
    //Multi Steps:
    //Pull all In
    // merge one
    // split among outputs --- Mergers/Splitters?
    private IOCell _input;
    private IOCell _output;

    private Vector3 positionOff = Vector3.zero;
    private FloatRange zMoveRange = new FloatRange(0, -0.6f);
    private FloatRange uvMoveRange = new FloatRange(0, -0.3f);

    private FXLayer _layer;

    private FXLayer StolenLayer
    {
        get
        {
            if (_layer == null)
            {
                _layer = parent.GetComp<CompFX>().FXLayers.Find(l => l.data.layerTag == "PumpPiston");
            }
            return _layer;
        }
    }

    public override Vector3? FX_GetDrawPosition(FXLayerArgs args)
    {
        switch (args.layerTag)
        {
            case "PumpPiston" or "PumpPistonCap":
                {
                    StolenLayer.PropertyBlock.SetVector(TeleShaderIDs.OffsetID, positionOff / 2f);
                    return parent.DrawPos + new Vector3(0, 0, -0.125f) + positionOff;
                }
        }

        return base.FX_GetDrawPosition(args);
    }


    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        //TODO: Handle sub-part-specific IO
        _input = GeneralIO.Connections.Find(c => c.Mode == NetworkIOMode.Input);
        _output = GeneralIO.Connections.Find(c => c.Mode == NetworkIOMode.Output);
    }

    private bool resetting;
    private bool wantsPump;
    private int pumpTick;

    internal override void TeleTick()
    {
        if (wantsPump && !resetting)
        {
            positionOff.z = Mathf.Lerp(0, zMoveRange.max, pumpTick / 15f);
            pumpTick++;
            if (pumpTick > 15)
            {
                wantsPump = false;
                resetting = true;
                pumpTick = 0;
            }
            return;
        }
        else if (resetting)
        {
            positionOff.z = Mathf.Lerp(zMoveRange.max, 0, pumpTick / 90f);
            pumpTick++;
            if (pumpTick > 90)
            {
                resetting = false;
                pumpTick = 0;
            }
        }
    }

    public override void NetworkPostTick(INetworkPart netPart, bool isPowered)
    {
        if (!IsPowered) return;
        if (wantsPump || resetting) return;

        var output = netPart.Network.Graph.GetEdgeOnCell((NetworkPart)netPart, _output);
        if (output.IsValid && !netPart.Volume.Empty && !output.To.Volume.Full)
        {
            netPart.Network.System.TransferFromTo((NetworkPart)netPart, output.To, 1);
            wantsPump = true;
        }
    }
}

public class CompProperties_Pump : CompProperties_Network
{
    public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
    {
        return base.ConfigErrors(parentDef);
    }
}