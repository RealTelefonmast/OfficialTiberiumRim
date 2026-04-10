using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace TiberiumRim
{
    public class WorldLayer_Tiberium : WorldLayer
    {
        private static readonly FloatRange BaseSizeRange = new FloatRange(0.9f, 1.1f);

        private static readonly IntVec2 TexturesInAtlas = new IntVec2(2, 2);

        private static readonly FloatRange BasePosOffsetRange_Small = new FloatRange(0f, 0.37f);

        private static readonly FloatRange BasePosOffsetRange_Medium = new FloatRange(0f, 0.2f);

        private static readonly FloatRange BasePosOffsetRange_Big = new FloatRange(0f, 0.08f);

        private static readonly FloatRange BasePosOffsetRange_Large = new FloatRange(0f, 0.08f);

        public override IEnumerable Regenerate()
        {
            WorldComponent_TiberiumSpread world = Find.World.GetComponent<WorldComponent_TiberiumSpread>();
            if (world.tibLayer == null)
            {
                Find.World.GetComponent<WorldComponent_TiberiumSpread>().tibLayer = this;
            }
            IEnumerator enumerator = base.Regenerate().GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    object result = enumerator.Current;
                    yield return result;
                }
            }
            finally
            {
                IDisposable disposable;
                if ((disposable = (enumerator as IDisposable)) != null)
                {
                    disposable.Dispose();
                }
            }
            Rand.PushState();
            WorldGrid grid = Find.WorldGrid;            
            int tilesCount = grid.TilesCount;
            int i = 0;
            while (i < tilesCount)
            {
                Tile tile = grid[i];
                Material material;
                FloatRange floatRange;               
                if (world.TiberiumTiles.ContainsKey(i))
                {
                    //TODO: Fix This
                    float flt = world.TiberiumTiles[i];
                    if (tile.biome == BiomeDefOf.Ocean || tile.biome == BiomeDefOf.Lake)
                    {
                        material = TRMats.TiberiumGlacier;                        
                        floatRange = new FloatRange(0f,0f);
                        goto A;
                    }
                    if (flt < 0.25f)
                    {
                        material = TRMats.TiberiumInfSmall;
                        floatRange = BasePosOffsetRange_Small;
                        goto A;
                    }
                    if (flt <= 0.5f)
                    {
                        material = TRMats.TiberiumInfMedium;
                        floatRange = BasePosOffsetRange_Medium;
                        goto A;
                    }
                    if (flt <= 0.75f)
                    {
                        material = TRMats.TiberiumInfBig;
                        floatRange = BasePosOffsetRange_Big;
                        goto A;
                    }
                    if (flt <= 1f)
                    {
                        material = TRMats.TiberiumInfLarge;
                        floatRange = BasePosOffsetRange_Large;
                        goto A;
                    }
                }
                B:
                i++;
                continue;
                A:
                LayerSubMesh subMesh = base.GetSubMesh(material);
                Vector3 vector = grid.GetTileCenter(i);
                Vector3 posForTangents = vector;
                float magnitude = vector.magnitude;
                vector = (vector + Rand.UnitVector3 * floatRange.RandomInRange * grid.averageTileSize).normalized * magnitude;
                WorldRendererUtility.PrintQuadTangentialToPlanet(vector, posForTangents, BaseSizeRange.RandomInRange * grid.averageTileSize, 0.005f, subMesh, false, true, false);
                WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMesh);
                goto B;
            }
            Rand.PopState();
            base.FinalizeMesh(MeshParts.All);
            yield break;
        }
    }
}
