using TeleCore.Atmosphere.Defs;
using TeleCore.Atmosphere.Utils;
using TeleCore.FlowCore;

namespace TeleCore.Atmosphere.Rooms;

/// <summary>
///     Similar to <see cref="FlowBox" /> for volumes of atmospheric values.
/// </summary>
public class AtmosphericVolume : FlowVolumeShared<AtmosphericValueDef>
{
    private int _cells;

    public AtmosphericVolume(FlowVolumeConfig<AtmosphericValueDef> config) : base(config)
    {
    }

    public override float MaxCapacity => CapacityPerType * _config.AllowedValues.Count;
    public override float CapacityPerType => _cells * AtmosResources.CELL_CAPACITY;

    public void UpdateVolume(int cellCount)
    {
        _cells = cellCount;
    }
}