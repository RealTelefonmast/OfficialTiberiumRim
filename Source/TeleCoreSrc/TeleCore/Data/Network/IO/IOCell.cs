using System;
using TeleCore.Primitive;
using Verse;

namespace TeleCore.Network.IO;

/// <summary>
///     Meant to be set and created in XML
/// </summary>
public struct IOCellPrototype
{
    public IntVec3 offset;
    public Rot4 direction;
    public NetworkIOMode mode;
}

/// <summary>
///     Implements the actual IO Cell of NetworkPart
/// </summary>
public readonly struct IOCell : IEquatable<IOCell>
{
    public IntVec3Rot Pos { get; }
    public NetworkIOMode Mode { get;  }

    public static implicit operator IntVec3(IOCell cell)
    {
        return cell.Pos.Pos;
    }

    public IOCell(IntVec3Rot pos, NetworkIOMode mode)
    {
        Pos = pos;
        Mode = mode;
    }
    
    /// <summary>
    /// The position INSIDE of the IO-Holder of the IO-Cell
    /// </summary>
    public IntVec3 Interface => Pos.Pos + Pos.Dir.Opposite.FacingCell;

    public bool Equals(IOCell other)
    {
        return Pos.Equals(other.Pos) && Mode == other.Mode;
    }
}