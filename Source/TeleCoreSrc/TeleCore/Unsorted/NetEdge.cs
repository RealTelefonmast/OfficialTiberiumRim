using System.Diagnostics;

namespace TeleCore.Unsorted;

public interface IEdge<T>
{
    public T From { get; set; }
    public T To { get; set; }
}

//Node - Edge - Node
//(Anchor < Edge > Anchor)
// (Node<EdgePart < Edge > EdgePart>Node)


[DebuggerDisplay("From: {From} To: {To}")]
public struct NetEdge : IEdge<NetworkPart>
{
    public NetworkPart From { get; set; }
    public NetworkPart To { get; set; }
    
    public IOConnection FromAnchor { get; }
    public IOConnection ToAnchor { get; }

    public NetworkIOMode FromMode => FromAnchor.FromMode;
    public NetworkIOMode ToMode => ToAnchor.ToMode;
    
    public IOCell FromIOCell => FromAnchor.FromIOCell;
    public IOCell ToIOCell => ToAnchor.ToIOCell;
    
    public int Length { get; private set; }
    
    public bool BiDirectional => FromMode == NetworkIOMode.TwoWay && ToMode == NetworkIOMode.TwoWay;
    public bool IsValid => From != null && To != null && 
                           From.IsNode && To.IsNode &&
                           (FromMode & NetworkIOMode.Output) == NetworkIOMode.Output &&
                           (ToMode & NetworkIOMode.Input) == NetworkIOMode.Input;
    
    public bool IsLogical => IsValid && (FromMode == NetworkIOMode.Logical || ToMode == NetworkIOMode.Logical);
    
    public static implicit operator TwoWayKey<NetNode>(NetEdge edge)
    {
        return (edge.From, edge.To);
    }
    
    public static implicit operator (NetworkPart, NetworkPart)(NetEdge edge)
    {
        return (edge.From, edge.To);
    }

    public NetEdge(IOConnection from, IOConnection to, int length)
    {
        //TODO: Simply ignore outright invalid io connections

        //Note: Special case between two direct nodes
        if (from == to)
        {
            if (from.From.IsNode && from.To.IsNode)
            {
                //If we already have From:Output To:Input then we can just use that
                if ((from.FromMode & NetworkIOMode.Output) == NetworkIOMode.Output && 
                    (from.ToMode & NetworkIOMode.Input) == NetworkIOMode.Input)
                {
                    FromAnchor = from;
                    ToAnchor = from;
                    From = from.From;
                    To = from.To;
                    Length = length;
                    return;
                }
                //Otherwise we reverse the connecting connections
                else if ((from.ToMode & NetworkIOMode.Output) == NetworkIOMode.Output && 
                         (from.FromMode & NetworkIOMode.Input) == NetworkIOMode.Input)
                {
                    FromAnchor = from.Reverse;
                    ToAnchor = from.Reverse;
                    From = FromAnchor.From;
                    To = ToAnchor.To;
                    Length = length;
                    return;
                }
                return;
            }
        }
        
        //Resolve Input
        FromAnchor = from.From.IsNode ? from : from.Reverse;
        ToAnchor = to.To.IsNode ? to : to.Reverse;

        //Swap if necessary
        if ((FromAnchor.FromMode & NetworkIOMode.Output) != NetworkIOMode.Output)
        {
            var prevFrom = FromAnchor;
            var prevTo = ToAnchor;
            FromAnchor = prevTo;
            ToAnchor = prevFrom;
        }
        else if ((ToAnchor.ToMode & NetworkIOMode.Input) != NetworkIOMode.Input)
        {
            var prevFrom = FromAnchor;
            var prevTo = ToAnchor;
            FromAnchor = prevTo;
            ToAnchor = prevFrom;
        }

        //Adjust swapped
        FromAnchor = FromAnchor.From.IsNode ? FromAnchor : FromAnchor.Reverse;
        ToAnchor = ToAnchor.To.IsNode ? ToAnchor : ToAnchor.Reverse;

        From = FromAnchor.From;
        To = ToAnchor.To;

        Length = length;
    }

    public NetEdge Reverse => new(ToAnchor.Reverse, FromAnchor.Reverse, Length);
    
    public static NetEdge Invalid => new NetEdge
    {
        From = null,
        To = null,
        Length = -1
    };

    public bool Equals(NetEdge other)
    {
        return From == other.From && To == other.To;
    }

    public override string ToString()
    {
        return $"{From} > {To}\n{FromAnchor} > {ToAnchor}";
    }
}

/*
[DebuggerDisplay("From: {From} To: {To}")]
public struct NetEdge : IEdge<NetworkPart>
{
    #region Properties
    
    public NetworkPart From { get; set; }
    public NetworkPart To { get; set; }
    
    public IntVec3 FromPos { get; set; }
    public IntVec3 ToPos { get; set; }

    public NetworkIOMode FromIO { get; set; }
    public NetworkIOMode ToIO { get; set; }

    public int Length { get; set; }
    
    #endregion

    public bool BiDirectional => FromIO == NetworkIOMode.TwoWay && ToIO == NetworkIOMode.TwoWay;

    public bool IsValid => From != null && To != null && 
                           (FromIO & NetworkIOMode.Output) == NetworkIOMode.Output &&
                           (ToIO & NetworkIOMode.Input) == NetworkIOMode.Input;

    public NetEdge Reverse => new(To, From, ToPos, FromPos, FromIO, ToIO, Length);

    public static NetEdge Invalid => new NetEdge
    {
        From = null,
        To = null,
        FromPos = IntVec3.Invalid,
        ToPos = IntVec3.Invalid,
        FromIO = NetworkIOMode.None,
        ToIO = NetworkIOMode.None,
        Length = -1
    };

    public static implicit operator NetEdge((NetworkPart, NetworkPart) edge)
    {
        return new NetEdge(edge.Item1, edge.Item2);
    }
    
    public static implicit operator TwoWayKey<NetNode>(NetEdge edge)
    {
        return (edge.From, edge.To);
    }

    public static implicit operator (NetworkPart, NetworkPart)(NetEdge edge)
    {
        return (edge.From, edge.To);
    }

    public NetEdge(IOConnection from, IOConnection to, int length)
    {
    }
    
    public NetEdge(NetworkPart from, NetworkPart to)
    {
        From = from;
        To = to;
    }

    public NetEdge(INetworkPart from, INetworkPart to, IntVec3 fromPos, IntVec3 toPos, NetworkIOMode fromMode, NetworkIOMode toMode, int length)
    {
        From = (NetworkPart) from;
        To = (NetworkPart) to;
        FromPos = fromPos;
        ToPos = toPos;
        FromIO = fromMode;
        ToIO = toMode;
        Length = length;
        
        //Correction
        if (!BiDirectional)
        {
            if ((FromIO & NetworkIOMode.Input) == NetworkIOMode.Input && (ToIO & NetworkIOMode.Output) == NetworkIOMode.Output)
            {
                (From, To) = (To, From);
                (FromIO, ToIO) = (ToIO, FromIO);
                (FromPos, ToPos) = (ToPos, FromPos);
            }
        }
    }

    public bool Equals(NetEdge other)
    {
        return Equals(From, other.From)
               && Equals(To, other.To)
               && FromPos.Equals(other.FromPos)
               && ToPos.Equals(other.ToPos)
               && FromIO == other.FromIO
               && ToIO == other.ToIO
               && Length == other.Length;
    }
}*/