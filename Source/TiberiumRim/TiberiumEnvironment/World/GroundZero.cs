using RimWorld.Planet;
using TR.GameParts.Interfaces;
using TR.TiberiumObjects;
using Verse;

namespace TR.TiberiumEnvironment.World;

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