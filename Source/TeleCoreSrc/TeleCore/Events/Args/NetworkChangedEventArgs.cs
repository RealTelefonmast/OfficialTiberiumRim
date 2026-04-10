using TeleCore.Network;

namespace TeleCore.Events.Args;

public enum NetworkChangeType
{
    Created,
    Destroyed,
    AddedPart,
    RemovedPart
}

public struct NetworkChangedEventArgs
{
    public NetworkChangedEventArgs(NetworkChangeType changeType)
    {
        ChangeType = changeType;
        Part = null;
    }

    public NetworkChangedEventArgs(NetworkChangeType changeType, INetworkPart part)
    {
        ChangeType = changeType;
        Part = part;
    }

    public NetworkChangeType ChangeType { get; }
    public INetworkPart Part { get; }
}