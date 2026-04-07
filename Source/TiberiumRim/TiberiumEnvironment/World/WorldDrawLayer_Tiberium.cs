using System.Collections;
using RimWorld.Planet;
using TR.Rendering.TextureContent;
using TR.Util;
using UnityEngine;
using Verse;

namespace TR.TiberiumEnvironment.World;

public class WorldDrawLayer_Tiberium : WorldDrawLayer
{
    public override IEnumerable Regenerate()
    {
        foreach (var obj in base.Regenerate()) yield return obj;
        Rand.PushState();
        Rand.Seed = Find.World.info.Seed;
        var worldGrid = Find.WorldGrid;
        var tibInfo = TRUtils.Tiberium().TiberiumInfo;
        var tilesCount = worldGrid.TilesCount;
        var i = 0;
        while (i < tilesCount)
        {
            var coverage = tibInfo.CoverageAt(i);
            if (coverage <= 0)
            {
                i++;
                continue;
            }
            //TiberiumTile tibTile = Find.World.worldObjects.WorldObjectAt<TiberiumTile>(i);

            var subMesh = GetSubMesh(Material(coverage, i));
            var vector = worldGrid.GetTileCenter(i);
            var posForTangents = vector;
            //float magnitude = vector.magnitude;
            //vector = (vector + Rand.UnitVector3 * worldGrid.averageTileSize).normalized * magnitude;
            WorldRendererUtility.PrintQuadTangentialToPlanet(vector, posForTangents, 1.35f * worldGrid.averageTileSize,
                0.005f, subMesh, false, true, false);
            WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, 2), Rand.Range(0, 2), 2, 2, subMesh);

            i++;
        }

        Rand.PopState();
        FinalizeMesh(MeshParts.All);
        yield break;
        yield break;
    }

    private static Material Material(float coverage, int tile)
    {
        if (coverage > 0 && Find.WorldGrid.Tiles[tile].WaterCovered) return TiberiumContent.TibTile_Glacier;
        if (coverage > 0.75f) return TiberiumContent.TibTile_4;

        if (coverage > 0 && Find.WorldGrid.tiles[tile].WaterCovered) return TiberiumContent.TibTile_Glacier;
        if (coverage > 0.75f) return TiberiumContent.TibTile_4;

        if (coverage > 0.5f) return TiberiumContent.TibTile_3;

        if (coverage > 0.25f) return TiberiumContent.TibTile_2;
        return TiberiumContent.TibTile_1;
    }
}

}