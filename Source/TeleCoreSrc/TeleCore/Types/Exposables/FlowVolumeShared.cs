using TeleCore.Defs;
using UnityEngine;

namespace TeleCore.Types.Exposables;

public class FlowVolumeShared<T> : FlowVolumeBase<T> where T : FlowValueDef
{
    public FlowVolumeShared(FlowVolumeConfig<T> config) : base(config)
    {
    }

    public override float MaxCapacity => _config.capacity * AllowedValues.Count;

    public override float CapacityOf(T? def)
    {
        return _config.capacity;
    }

    public override bool IsFull(T def)
    {
        return StoredValueOf(def) >= CapacityOf(def);
    }

    protected override float ExcessFor(T def, float amount)
    {
        return Mathf.Max(StoredValueOf(def) + amount - CapacityOf(def), 0);
    }
}