using Verse;

namespace TeleCore.FlowCore.Utility;

public static class PipeNetworkFactory
{
    internal static int MasterNetworkID = 0;

    /// <summary>
    /// Checks whether or not a thing is part of a specific network.
    /// </summary>
    internal static bool Fits(Thing thing, NetworkDef network, out INetworkPart part)
    {
        part = null;
        if (thing is not ThingWithComps compThing)
            return false;

        var networkComp = compThing.GetComp<CompNetwork>();
        if (networkComp == null) return false;

        part = networkComp[network];
        return part != null;
    }
}