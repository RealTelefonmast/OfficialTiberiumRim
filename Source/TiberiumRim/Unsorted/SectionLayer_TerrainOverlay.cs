using RimWorld;
using Verse;

namespace TR;

public class SectionLayer_TerrainOverlay : SectionLayer
{
    public SectionLayer_TerrainOverlay(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlagDefOf.Terrain;
    }

    public override bool Visible => DebugViewSettings.drawTerrain;

    public override void Regenerate()
    {
    }
}