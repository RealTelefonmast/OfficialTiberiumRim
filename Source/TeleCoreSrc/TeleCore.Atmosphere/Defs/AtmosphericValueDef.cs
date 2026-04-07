using System;
using System.Collections.Generic;
using TeleCore.Atmosphere.Grid;
using TeleCore.Atmosphere.Static;
using TeleCore.FlowCore;
using TeleCore.Network.Flow.Values;
using TeleCore.Rendering;
using TeleCore.Static;
using Verse;

namespace TeleCore.Atmosphere.Defs;

public enum AtmosphericRealm
{
    AnyBiome,
    SpecificBiome
}

public enum AtmosphericType
{
    Gas,
    Fluid
}

[Flags]
public enum DissipationMode
{
    None,
    IntoAir,
    IntoGround
}

public class RealmConfig : Editable
{
    public AtmosphericRealm realmType;
    public List<AtmosphericValueDef> requiresAtmospheres;
}

/// <summary>
/// Defines properties of any gas or fluid that can dissipate into air or ground.
/// </summary>
public class DissipationConfig : Editable
{
    public SpreadingGasTypeDef toGas;
    public DissipationMode mode;

    //TODO: Add terrainfilter from TR
    public List<string> terrainFilter;
}

public class AtmosphericValueDef : FlowValueDef
{
    //The corresponding network value (if available)
    public NetworkValueDef networkValue;
    public DissipationConfig dissipation;

    /// <summary>
    /// The tag group this atmospheric def belongs to.
    /// </summary>
    public string atmosphericTag;

    /// <summary>
    /// The atmospheric group tags that will be displaced by this atmospheric def.
    /// </summary>
    public List<string> displaceTags;

    /// <summary>
    /// Sets the physical elevation range of the gas within a cell. 0 being floor and 1 being ceiling.
    /// This can be used whether or not a gas affects a Pawn.
    /// </summary>
    public FloatRange fillRange = new(0, 1);

    //Rendering
    public NaturalOverlayProperties naturalOverlay;
    public RoomOverlayProperties roomOverlay;
    public bool useRenderLayer = false;
    public double friction;
    public override void PostLoad()
    {
        //
        base.PostLoad();
        AtmosphericReferenceCache.RegisterDef(this);
    }
}
