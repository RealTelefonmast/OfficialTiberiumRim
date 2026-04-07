using System;
using System.Collections.Generic;
using TeleCore.FlowCore;
using TeleCore.Network.Flow.Values;
using TeleCore.Network.IO;
using TeleCore.Primitive;

namespace TeleCore.Network.Flow;

/// <summary>
///     The logical handler for fluid flow.
///     Area and height define the total content, elevation allows for flow control.
/// </summary>
public class NetworkVolume : FlowVolume<NetworkValueDef>
{
    public override double CapacityOf(NetworkValueDef? def)
    {
        if (_config.shareCapacity) 
            return _config.capacity;
        return base.CapacityOf(def);
    }

    public override bool IsFull(NetworkValueDef def)
    {
        if (_config.shareCapacity) 
            return StoredValueOf(def) >= CapacityOf(def);
        return base.IsFull(def);
    }

    protected override double ExcessFor(NetworkValueDef def, double amount)
    {
        if (_config.shareCapacity)
            return Math.Max(StoredValueOf(def) + amount - CapacityOf(def), 0.0);
        return base.ExcessFor(def, amount);
    }

    public override double MaxCapacity
    {
        get
        {
            if (_config.shareCapacity)
            {
                return CapacityPerType * _config.AllowedValues.Count;
            }

            return base.MaxCapacity;
        }
    }

    public NetworkVolume() : base()
    {
    }
    
    public NetworkVolume(FlowVolumeConfig<NetworkValueDef> config) : base(config)
    {
    }
}