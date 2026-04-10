using System.Collections.Generic;
using TeleCore.Atmosphere.Static;
using TeleCore.FlowCore;
using TeleCore.Network.Flow.Values;
using TeleCore.Rendering;
using Verse;

namespace TeleCore.Atmosphere.Defs;

public class AtmosphericValueDef : FlowValueDef
{
    /// <summary>
    ///     The tag group this atmospheric def belongs to.
    /// </summary>
    public string atmosphericTag;

    /// <summary>
    ///     The atmospheric group tags that will be displaced by this atmospheric def.
    /// </summary>
    public List<string> displaceTags;

    public DissipationConfig dissipation;

    /// <summary>
    ///     Sets the physical elevation range of the gas within a cell. 0 being floor and 1 being ceiling.
    ///     This can be used whether or not a gas affects a Pawn.
    /// </summary>
    public FloatRange fillRange = new(0, 1);

    public double friction;

    //Rendering
    public NaturalOverlayProperties naturalOverlay;

    //The corresponding network value (if available)
    public NetworkValueDef networkValue;
    public RoomOverlayProperties roomOverlay;
    public bool useRenderLayer = false;

    public override void PostLoad()
    {
        //
        base.PostLoad();
        AtmosphericReferenceCache.RegisterDef(this);
    }
}