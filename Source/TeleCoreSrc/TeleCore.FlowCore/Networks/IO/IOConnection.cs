using System.Diagnostics;
using TeleCore.FlowCore.Utility;
using TeleCore.RWLib;

namespace TeleCore.FlowCore.IO;

[DebuggerDisplay("({From}[{FromMode}]>{To}[{ToMode}])")]
public record struct IOConnection
{
    public NetworkPart From { get; private set; }
    public NetworkPart To { get; private set; }

    public NetworkIOMode FromMode { get; set; }
    public NetworkIOMode ToMode { get; set; }

    public IntVec3Rot FromPos { get; set; }
    public IntVec3Rot ToPos { get; set; }

    public IOCell FromIOCell { get; set; }
    public IOCell ToIOCell { get; set; }

    public bool IsValid => FromPos.Pos.IsValid && ToPos.Pos.IsValid && FromMode != NetworkIOMode.None && ToMode != NetworkIOMode.None;

    public static implicit operator bool(IOConnection result)
    {
        return result.IsValid;
    }

    public static IOConnection TryCreate(NetworkPart from, NetworkPart to)
    {
        var ioFrom = from.PartIO;
        var ioTo = to.PartIO;

        foreach (var fromConn in ioFrom.Connections)
        {
            foreach (var toConn in ioTo.Connections)
            {
                //If these cannot interact, skip
                if (fromConn.Pos != toConn.Interface) continue;
                if (fromConn.Interface != toConn.Pos) continue;

                if (fromConn.Mode.MatchesIO(toConn.Mode))
                {
                    return new IOConnection
                    {
                        From = from,
                        To = to,
                        FromMode = fromConn.Mode,
                        ToMode = toConn.Mode,
                        FromPos = fromConn.Pos,
                        ToPos = toConn.Pos,
                        FromIOCell = fromConn,
                        ToIOCell = toConn
                    };
                }
            }
        }

        return Invalid;
    }

    public static IOConnection Invalid => new()
    {
        From = null,
        To = null,
        FromMode = NetworkIOMode.None,
        ToMode = NetworkIOMode.None,
        FromPos = IntVec3Rot.Invalid,
        ToPos = IntVec3Rot.Invalid
    };

    public bool IsBiDirectional => FromMode == NetworkIOMode.TwoWay && ToMode == NetworkIOMode.TwoWay;

    public IOConnection Reverse => new IOConnection
    {
        From = To,
        To = From,
        FromMode = ToMode,
        ToMode = FromMode,
        FromPos = ToPos,
        ToPos = FromPos,
        FromIOCell = ToIOCell,
        ToIOCell = FromIOCell
    };


    public (NetworkIOMode, IntVec3Rot) IOFor(NetworkPart part)
    {
        if (part == From)
        {
            return (FromMode, FromPos);
        }
        if (part == To)
        {
            return (ToMode, ToPos);
        }
        return (NetworkIOMode.None, IntVec3Rot.Invalid);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashFrom = From.GetHashCode();
            var hashTo = To.GetHashCode();
            var compare = hashFrom.CompareTo(hashTo);

            var hash = 17;
            if (compare > 0)
            {
                hash = hash * 31 + (From == null ? 0 : hashFrom);
                hash = hash * 31 + (To == null ? 0 : hashTo);
            }
            else
            {
                hash = hash * 31 + (To == null ? 0 : hashTo);
                hash = hash * 31 + (From == null ? 0 : hashFrom);
            }

            return hash;
        }
    }

    public bool Equals(IOConnection other)
    {
        if (From == other.From)
            return To == other.To;
        if (From == other.To)
            return To == other.From;
        return false;
    }

    public override string ToString()
    {
        return GetHashCode().ToString();
    }
}