using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    public class LayerSubMeshColor : LayerSubMesh
    {
        public Color color;

        public LayerSubMeshColor(Mesh mesh, Material material, Bounds? bounds = null, Color? color = null) : base(mesh, material, bounds)
        {
            this.color = color ?? Color.white;
        }

        public void SetColor(Color color)
        {
            this.color = color;
        }
    }

    public class SimpleWorldLayer_ModuleResult
    {
        protected List<LayerSubMeshColor> subMeshes = new List<LayerSubMeshColor>();
        private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        private bool dirty = true;

        //Data
        private SimpleWorldView simpleWorld;
        private SimpleWorldGrid WorldGrid => simpleWorld.WorldGrid;
        private ModuleBase ModuleResult => simpleWorld.ModuleResult;
        private int Seed => simpleWorld.Seed;

        //
        private List<MeshCollider> meshCollidersInOrder = new List<MeshCollider>();
        private List<List<int>> triangleIndexToTileID = new List<List<int>>();
        private List<Vector3> elevationValues = new List<Vector3>();

        public virtual bool ShouldRegenerate => dirty;
        protected virtual int Layer => 7;
        protected virtual Quaternion Rotation => Quaternion.identity;
        protected virtual float Alpha => 1f;
        public bool Dirty => dirty;

        public SimpleWorldLayer_ModuleResult(SimpleWorldView parentWorld)
        {
            simpleWorld = parentWorld;
        }

        public void RegenerateNow()
        {
            dirty = false;
            //Generate Mesh OneTime
            GenerateMesh().ExecuteEnumerable();

            Regenerate().ExecuteEnumerable();
        }

        private bool meshGenerated = false;
        private IEnumerable GenerateMesh()
        {
            if (meshGenerated)
            {
                yield break;
            }
            ClearSubMeshes(MeshParts.All);

            int tilesCount = WorldGrid.TilesCount;
            List<int> tileIDToVerts_offsets = WorldGrid.tileIDToVerts_offsets;
            List<Vector3> verts = WorldGrid.verts;
            triangleIndexToTileID.Clear();
            foreach (object obj2 in CalculateInterpolatedVerticesParams())
            {
                yield return obj2;
            }
            //
            int num = 0;
            for (int i = 0; i < tilesCount; i++)
            {
                LayerSubMesh subMesh = GetSubMesh(TiberiumContent.WorldTerrain, Color.white, out int j);
                while (j >= triangleIndexToTileID.Count)
                {
                    triangleIndexToTileID.Add(new List<int>());
                }
                int count = subMesh.verts.Count;
                int num2 = 0;
                int num3 = (i + 1 < tileIDToVerts_offsets.Count) ? tileIDToVerts_offsets[i + 1] : verts.Count;
                for (int k = tileIDToVerts_offsets[i]; k < num3; k++)
                {
                    subMesh.verts.Add(verts[k]);
                    subMesh.uvs.Add(elevationValues[num]);
                    num++;
                    if (k < num3 - 2)
                    {
                        subMesh.tris.Add(count + num2 + 2);
                        subMesh.tris.Add(count + num2 + 1);
                        subMesh.tris.Add(count);
                        triangleIndexToTileID[j].Add(i);
                    }
                    num2++;
                }
            }
            FinalizeMesh(MeshParts.All);
            foreach (object obj3 in RegenerateMeshColliders())
            {
                yield return obj3;
            }
            //
            elevationValues.Clear();
            elevationValues.TrimExcess();

            //
            meshGenerated = true;
        }

        public IEnumerable Regenerate()
        {
            TLog.Message($"Submeshes: {subMeshes.Count}");
            dirty = false;
            //
            int tilesCount = WorldGrid.TilesCount;
            for (var i = 0; i < subMeshes.Count; i++)
            {
                var mesh = subMeshes[i];
                mesh.SetColor(Color.Lerp(Color.green, Color.red, (float)(i/(float)subMeshes.Count)));
            }

            /*
            for (int i = 0; i < tilesCount; i++)
            {
                var pos = WorldGrid.GetTileCenter(i);
                var val = ModuleResult.GetValue(pos);

                var color = val > 0 ? Color.white : Color.clear;

                subMeshes[i].SetColor(color);
            }
            */
            yield break;
        }

        private IEnumerable RegenerateMeshColliders()
        {
            meshCollidersInOrder.Clear();
            GameObject gameObject = WorldTerrainColliderManager.GameObject;
            MeshCollider[] components = gameObject.GetComponents<MeshCollider>();
            int j;
            for (j = 0; j < components.Length; j++)
            {
                UnityEngine.Object.Destroy(components[j]);
            }
            for (int i = 0; i < subMeshes.Count; i = j + 1)
            {
                MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = subMeshes[i].mesh;
                meshCollidersInOrder.Add(meshCollider);
                yield return null;
                j = i;
            }
            yield break;
        }

        private IEnumerable CalculateInterpolatedVerticesParams()
        {
            elevationValues.Clear();
            var grid = WorldGrid;
            int tilesCount = grid.TilesCount;
            List<Vector3> verts = grid.verts;
            List<int> tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
            List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
            List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
            //List<Tile> tiles = grid.tiles;
            int num4;
            for (int i = 0; i < tilesCount; i = num4 + 1)
            {
                //Tile tile = tiles[i];
                //float elevation = tile.elevation;
                int num = (i + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[i + 1] : tileIDToNeighbors_values.Count;
                int num2 = (i + 1 < tilesCount) ? tileIDToVerts_offsets[i + 1] : verts.Count;
                for (int j = tileIDToVerts_offsets[i]; j < num2; j++)
                {
                    Vector3 vector = default(Vector3);
                    vector.x = 0; // tile.elevation
                    bool flag = false;
                    for (int k = tileIDToNeighbors_offsets[i]; k < num; k++)
                    {
                        int num3 = (tileIDToNeighbors_values[k] + 1 < tileIDToVerts_offsets.Count) ? tileIDToVerts_offsets[tileIDToNeighbors_values[k] + 1] : verts.Count;
                        int l = tileIDToVerts_offsets[tileIDToNeighbors_values[k]];
                        while (l < num3)
                        {
                            if (verts[l] == verts[j])
                            {
                                //Tile tile2 = tiles[tileIDToNeighbors_values[k]];
                                /*
                                if (flag)
                                {
                                    break;
                                }
                                if ((tile2.elevation >= 0f && elevation <= 0f) || (tile2.elevation <= 0f && elevation >= 0f))
                                {
                                    flag = true;
                                    break;
                                }
                                if (tile2.elevation > vector.x)
                                {
                                    vector.x = tile2.elevation;
                                    break;
                                }
                                */
                                break;
                            }
                            else
                            {
                                l++;
                            }
                        }
                    }
                    if (flag)
                    {
                        vector.x = 0f;
                    }
                    /*
                    if (tile.biome.DrawMaterial.shader != ShaderDatabase.WorldOcean && vector.x < 0f)
                    {
                        vector.x = 0f;
                    }
                    */
                    elevationValues.Add(vector);
                }
                if (i % 1000 == 0)
                {
                    yield return null;
                }
                num4 = i;
            }
            yield break;
        }

        private IEnumerable OldRegen()
        {
            Rand.PushState();
            Rand.Seed = Seed;
            int tilesCount = WorldGrid.TilesCount;
            int i = 0;
            while (i < tilesCount)
            {
                var pos = WorldGrid.GetTileCenter(i);
                float coverage = ModuleResult.GetValue(pos);
                if (coverage <= 0)
                {
                    i++;
                    continue;
                }
                //TiberiumTile tibTile = Find.World.worldObjects.WorldObjectAt<TiberiumTile>(i);

                LayerSubMesh subMesh = GetSubMesh(TiberiumContent.WorldTerrain, new Color(1,1,1, coverage));
                Vector3 vector = WorldGrid.GetTileCenter(i);
                Vector3 posForTangents = vector;
                //float magnitude = vector.magnitude;
                //vector = (vector + Rand.UnitVector3 * worldGrid.averageTileSize).normalized * magnitude;
                WorldRendererUtility.PrintQuadTangentialToPlanet(vector, posForTangents, 1.35f * WorldGrid.averageTileSize, 0.005f, subMesh, false, true, false);
                WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, 2), Rand.Range(0, 2), 2, 2, subMesh);

                i++;
            }
            Rand.PopState();
            FinalizeMesh(MeshParts.All);
            yield break;
        }

        public void Render()
        {
            if (ShouldRegenerate)
            {
                RegenerateNow();
            }

            int layer = Layer;
            Quaternion rotation = Rotation;
            for (int i = 0; i < subMeshes.Count; i++)
            {
                if (subMeshes[i].finalized)
                {
                    Color color = subMeshes[i].color;
                    propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
                    Graphics.DrawMesh(subMeshes[i].mesh, Vector3.zero, rotation, subMeshes[i].material, layer, null, 0, propertyBlock);
                }
            }
        }

        protected LayerSubMeshColor GetSubMesh(Material material, Color color)
        {
            int num;
            return GetSubMesh(material, color, out num);
        }

        protected LayerSubMeshColor GetSubMesh(Material material, Color color, out int subMeshIndex)
        {
            for (int i = 0; i < subMeshes.Count; i++)
            {
                LayerSubMeshColor layerSubMesh = subMeshes[i];
                if (layerSubMesh.material == material && layerSubMesh.verts.Count < 40000)
                {
                    subMeshIndex = i;
                    return layerSubMesh;
                }
            }

            Mesh mesh = new Mesh();
            if (UnityData.isEditor)
            {
                mesh.name = $"SimpleWorldLayerSubMesh_{GetType().Name}_{Find.World.info.seedString}";
            }

            LayerSubMeshColor layerSubMesh2 = new LayerSubMeshColor(mesh, material, null, color);
            subMeshIndex = subMeshes.Count;
            subMeshes.Add(layerSubMesh2);
            return layerSubMesh2;
        }

        protected void FinalizeMesh(MeshParts tags)
        {
            for (int i = 0; i < subMeshes.Count; i++)
            {
                if (subMeshes[i].verts.Count > 0)
                {
                    subMeshes[i].FinalizeMesh(tags);
                }
            }
        }

        public void SetDirty()
        {
            dirty = true;
        }

        private void ClearSubMeshes(MeshParts parts)
        {
            for (int i = 0; i < subMeshes.Count; i++)
            {
                subMeshes[i].Clear(parts);
            }
        }
    }
}
