using TeleCore.FlowCore;
using Verse;

namespace TeleCore.HelperClasses;

public static class Scribe_FlowCore
{
    public static bool InvalidState { get; private set; }
    
    public static void Look<TDef>(ref FlowVolume<TDef> volume, string label, FlowVolumeConfig<TDef> config)
    where TDef : FlowValueDef
    {
        InvalidState = false;
        Scribe_Deep.Look(ref volume, label, config);
        InvalidState = true;
    }
}