using Verse;

namespace TeleCore.FlowCore;

internal enum DelayedNetworkActionType
{
    Register,
    Deregister
}

internal struct DelayedNetworkAction
{
    public DelayedNetworkActionType type;
    public INetworkPart Part;
    public IntVec3 pos;

    public DelayedNetworkAction(DelayedNetworkActionType type, INetworkPart part, IntVec3 pos)
    {
        this.type = type;
        Part = part;
        this.pos = pos;
    }
}