using Verse;

namespace TeleCore.Events.Args;

public struct CellChangedEventArgs
{
    public IntVec3 Cell { get; }
    public CellRect CellRect { get; }

    public Thing? Thing { get; }
    public TerrainDef? Terrain { get; }

    public ThingStateChangedEventArgs? ThingChangedArgs { get; }
    public TerrainChangedEventArgs? TerrainChangedArgs { get; }

    public CellChangedEventArgs(ThingStateChangedEventArgs args)
    {
        ThingChangedArgs = args;
        Cell = args.Thing.Position;
        CellRect = args.Thing.OccupiedRect();
        Thing = args.Thing;
    }

    public CellChangedEventArgs(TerrainChangedEventArgs args)
    {
        TerrainChangedArgs = args;
        Cell = args.Position;
        CellRect = CellRect.SingleCell(Cell);
        Terrain = args.NewTerrain;
    }
}