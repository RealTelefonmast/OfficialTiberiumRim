using System;
using RimWorld;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class PortableNetworkContainer : FXThing
{
    private LocalTargetInfo currentDesignatedTarget = LocalTargetInfo.Invalid;

    //Portable Data

    //Targeting
    private TargetingParameters? paramsInt;

    //
    public NetworkDef NetworkDef { get; }

    public NetworkVolume NetworkVolume { get; }

    public float EmptyPercent => (float)NetworkVolume.FillPercent - 1f;
    public bool HasValidTarget { get; set; }
    public LocalTargetInfo TargetToEmptyAt { get; set; }

    public void Notify_FinishEmptyingToTarget()
    {
        throw new NotImplementedException();
    }
}