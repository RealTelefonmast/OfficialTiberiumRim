namespace TeleCore.FlowCore;

public class FlowVolume<T> : FlowVolumeBase<T> where T : FlowValueDef
{
    public FlowVolume() : base()
    {
    }

    public FlowVolume(FlowVolumeConfig<T> config) : base(config)
    {
    }
}