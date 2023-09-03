using System.Collections;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TR
{
    public class WorldLayer_Tiberium : WorldLayer
    {
        public override IEnumerable Regenerate()
        {
            foreach (object obj in base.Regenerate())
            {
                yield return obj;
            }
            Rand.PushState();
            Rand.Seed = Find.World.info.Seed;
            WorldGrid worldGrid = Find.WorldGrid;
            TiberiumWorldInfo tibInfo = TRUtils.Tiberium().TiberiumInfo; 
            int tilesCount = worldGrid.TilesCount;
            int i = 0;
            while (i < tilesCount)
            {
                float coverage = tibInfo.CoverageAt(i);
                if (coverage <= 0)
                {
                    i++;
                    continue;
                }
                //TiberiumTile tibTile = Find.World.worldObjects.WorldObjectAt<TiberiumTile>(i);

                LayerSubMesh subMesh = base.GetSubMesh(Material(coverage, i));
                Vector3 vector = worldGrid.GetTileCenter(i);
                Vector3 posForTangents = vector;
                //float magnitude = vector.magnitude;
                //vector = (vector + Rand.UnitVector3 * worldGrid.averageTileSize).normalized * magnitude;
                WorldRendererUtility.PrintQuadTangentialToPlanet(vector, posForTangents, 1.35f * worldGrid.averageTileSize, 0.005f, subMesh, false, true, false);
                WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, 2), Rand.Range(0, 2), 2, 2, subMesh);

                i++;
            }
            Rand.PopState();
            base.FinalizeMesh(MeshParts.All);
            yield break;
        }

        private static Material Material(float coverage, int tile)
        {

            if (coverage > 0 && Find.WorldGrid.tiles[tile].WaterCovered)
            {
                return TiberiumContent.TibTile_Glacier;
            }

            if (coverage > 0.875)
            {
                return TiberiumContent.Infested_8;
            }
            if (coverage > 0.75f)
            {
                return TiberiumContent.Infested_7;
            }
            else if (coverage > 0.625f)
            {
                return TiberiumContent.Infested_6;
            }
            else if (coverage > 0.5f)
            {
                return TiberiumContent.Infested_5;
            }
            else if (coverage > 0.375f)
            {
                return TiberiumContent.Infested_4;
            }
            else if (coverage > 0.25f)
            {
                return TiberiumContent.Infested_3;
            }
            else if (coverage > 0.125f)
            {
                return TiberiumContent.Infested_2;
            }
            return TiberiumContent.Infested_1;
        }
    }
}
