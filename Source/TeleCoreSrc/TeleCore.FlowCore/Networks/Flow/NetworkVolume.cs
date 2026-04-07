using UnityEngine;

namespace TeleCore.FlowCore.Flow;

/// <summary>
///     The logical handler for fluid flow.
///     Area and height define the total content, elevation allows for flow control.
/// </summary>
public class NetworkVolume : FlowVolume<NetworkValueDef>
{
    public override float CapacityOf(NetworkValueDef? def)
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

    protected override float ExcessFor(NetworkValueDef def, float amount)
    {
        if (_config.shareCapacity)
            return Mathf.Max(StoredValueOf(def) + amount - CapacityOf(def), 0f);
        return base.ExcessFor(def, amount);
    }

    public override float MaxCapacity
    {
        get
        {
            if (_config.shareCapacity) return CapacityPerType * _config.AllowedValues.Count;

            return base.MaxCapacity;
        }
    }

    public NetworkVolume()
    {
    }

    public NetworkVolume(FlowVolumeConfig<NetworkValueDef> config) : base(config)
    {
    }
}