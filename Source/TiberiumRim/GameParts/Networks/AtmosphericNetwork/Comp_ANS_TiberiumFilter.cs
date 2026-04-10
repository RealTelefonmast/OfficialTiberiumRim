using TeleCore.Atmosphere.Static;
using TeleCore.Atmosphere.Vents;
using TeleCore.Events.Args;
using TeleCore.Network;
using TeleCore.Network.Flow.Values;
using UnityEngine;
using Verse;

namespace TR.Networks.AtmosphericNetwork;

public class Comp_ANS_TiberiumFilter : Comp_AtmosphericNetworkStructure
{
    private IntRange animationRange = new(15, 45);
    private int curAnimLength;

    public SimpleCurve FlickerCurve = new()
    {
        new CurvePoint(0, 0.4f),
        new CurvePoint(0.6f, 0.6f),
        new CurvePoint(0.75f, 1),
        new CurvePoint(0.8f, 0.6f),
        new CurvePoint(1, 0.4f)
    };

    private int ticksLeft;

    public INetworkPart AtmosphericComp => this[AtmosDefOf.AtmosphericNetwork];
    public INetworkPart ProcessingComp => this[Defs.TiberiumDefOf.TiberiumNetwork];

    private bool ShouldProcess => !AtmosphericComp.Volume.Empty && !ProcessingComp.Volume.Full;

    private float Alpha =>
        ticksLeft > 0 ? FlickerCurve.Evaluate((curAnimLength - ticksLeft) / (float)curAnimLength) : 0.4f;

    public override Vector3? FX_GetDrawPosition(FXLayerArgs args)
    {
        return args.index switch
        {
            0 => parent.DrawPos,
            _ => null
        };
        return base.FX_GetDrawPosition(args);
    }

    public override Color? FX_GetColor(FXLayerArgs args)
    {
        return args.index switch
        {
            0 => Color.white,
            _ => null
        };
        return base.FX_GetColor(args);
    }

    public override float? FX_GetOpacity(FXLayerArgs args)
    {
        return args.index switch
        {
            0 => Alpha,
            _ => 1
        };
        return base.FX_GetOpacity(args);
    }

    //
    public override void CompTick()
    {
        base.CompTick();
        if (ticksLeft > 0)
            ticksLeft--;
    }

    public override void Notify_ReceivedValue()
    {
        if (ticksLeft > 0) return;
        ticksLeft = curAnimLength = animationRange.RandomInRange;
    }

    public override bool AcceptsValue(NetworkValueDef value)
    {
        return !AtmosphericComp.Volume.Full;
    }

    public override void NetworkPostTick(INetworkPart networkSubPart, bool isPowered)
    {
        if (!ShouldProcess) return;
        if (AtmosphericComp.Volume.TryRemove(Defs.TiberiumDefOf.Atmospheric_TibPollution, 10, out var result))
            ProcessingComp.Volume.TryAdd(Defs.TiberiumDefOf.TibSludge, result.Actual * 0.125f, out _);
    }

    public override string CompInspectStringExtra()
    {
        return base.CompInspectStringExtra();
    }
}