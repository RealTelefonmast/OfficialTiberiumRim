using Verse;

namespace TR.GameParts.Interfaces;

public interface IGroundZero
{
    public bool IsGroundZero { get; }

    public Thing GZThing { get; }
}