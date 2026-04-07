using Verse;

namespace TeleCore.Events.Args;

public struct TerrainChangedEventArgs
{
    public TerrainChangedEventArgs(IntVec3 pos, bool isSubTerrain, TerrainDef previous, TerrainDef terrain)
    {
        Position = pos;
        PreviousTerrain = previous;
        NewTerrain = terrain;
        IsSubTerrain = isSubTerrain;
    }

    public IntVec3 Position { get; }

    public TerrainDef PreviousTerrain { get; }
    public TerrainDef NewTerrain { get; }

    public bool IsSubTerrain { get; }
}