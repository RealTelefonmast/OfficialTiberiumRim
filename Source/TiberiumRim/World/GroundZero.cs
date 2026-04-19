using RimWorld.Planet;
using TR.Interfaces;
using Verse;

namespace TR.World;

public class GroundZero : TiberiumTile, IGroundZero
{
    protected TiberiumCrater mainCrater;

    public int TileInt => Tile;
    public LocalTargetInfo LocalTarget { get; set; }
    public GlobalTargetInfo GlobalTarget { get; set; }
    public Thing GZThing => mainCrater;
    public bool IsGroundZero { get; }

    public void PassOnGZTitle()
    {
    }
}