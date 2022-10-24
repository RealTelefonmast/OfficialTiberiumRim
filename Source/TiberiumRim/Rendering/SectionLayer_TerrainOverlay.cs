using Verse;

namespace TiberiumRim.Rendering;

public class SectionLayer_TerrainOverlay : SectionLayer
{
    public override bool Visible => DebugViewSettings.drawTerrain;

    public SectionLayer_TerrainOverlay(Section section) : base(section)
    {
        relevantChangeTypes = MapMeshFlag.Terrain;
    }

    public override void Regenerate()
    {
        
    }
}