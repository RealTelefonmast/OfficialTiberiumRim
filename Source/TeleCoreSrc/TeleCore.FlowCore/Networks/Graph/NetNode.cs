using System.Diagnostics;

namespace TeleCore.FlowCore.Graph;

[DebuggerDisplay("{Value}")]
public struct NetNode
{
    public NetworkPart Value { get; }

    public static implicit operator NetworkPart(NetNode node)
    {
        return node.Value;
    }

    public static implicit operator NetNode(NetworkPart node)
    {
        return new NetNode(node);
    }

    public NetNode(NetworkPart value)
    {
        Value = value;
    }

    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }
}