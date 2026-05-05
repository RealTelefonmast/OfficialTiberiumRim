using System.Collections;
using System.Collections.Generic;
using RimWorld.Planet;
using TeleCore.Types.Utils;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TeleCore.Types;

public class LayerSubMeshColor : LayerSubMesh
{
    public Color color;

    public LayerSubMeshColor(Mesh mesh, Material material, Bounds? bounds = null, Color? color = null) : base(mesh,
        material, bounds)
    {
        this.color = color ?? Color.white;
    }

    public void SetColor(Color color)
    {
        this.color = color;
    }
}

[StaticConstructorOnStartup]
public class SimpleWorldLayer_ModuleResult
{
    private static readonly MaterialPropertyBlock propertyBlock = new();
    private readonly List<Vector3> elevationValues = new();

    //
    private readonly List<MeshCollider> meshCollidersInOrder = new();

    //Data
    private readonly SimpleWorldView simpleWorld;
    private readonly List<List<int>> triangleIndexToTileID = new();

    private bool meshGenerated;
    protected List<LayerSubMeshColor> subMeshes = new();

    public SimpleWorldLayer_ModuleResult(SimpleWorldView parentWorld)
    {
        simpleWorld = parentWorld;
    }

    private SimpleWorldGrid WorldGrid => simpleWorld.WorldGrid;
    private ModuleBase ModuleResult => simpleWorld.ModuleResult;
    private int Seed => simpleWorld.Seed;

    public virtual bool ShouldRegenerate => Dirty;
    protected virtual int Layer => 7;
    protected virtual Quaternion Rotation => Quaternion.identity;
    protected virtual float Alpha => 1f;
    public bool Dirty { get; private set; } = true;

    public void RegenerateNow()
    {
        Dirty = false;
        //Generate Mesh OneTime
        GenerateMesh().ExecuteEnumerable();

        Regenerate().ExecuteEnumerable();
    }

    private IEnumerable GenerateMesh()
    {
        if (meshGenerated) yield break;
        ClearSubMeshes(MeshParts.All);

        var tilesCount = WorldGrid.TilesCount;
        var tileIDToVerts_offsets = WorldGrid.tileIDToVerts_offsets;
        var verts = WorldGrid.verts;
        triangleIndexToTileID.Clear();
        foreach (var obj2 in CalculateInterpolatedVerticesParams()) yield return obj2;
        //
        var num = 0;
        for (var i = 0; i < tilesCount; i++)
        {
            var pos = WorldGrid.GetTileCenter(i);
            var val = ModuleResult.GetValue(pos);
            if (val <= 0.15) continue; //Exclude tile from being added to mesh gen

            LayerSubMesh subMesh = GetSubMesh(TeleContent.WorldTerrain, Color.cyan, out var j);
            while (j >= triangleIndexToTileID.Count) triangleIndexToTileID.Add(new List<int>());
            var count = subMesh.verts.Count;
            var num2 = 0;
            var num3 = i + 1 < tileIDToVerts_offsets.Count ? tileIDToVerts_offsets[i + 1] : verts.Count;
            for (var k = tileIDToVerts_offsets[i]; k < num3; k++)
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
        foreach (var obj3 in RegenerateMeshColliders()) yield return obj3;
        //
        elevationValues.Clear();
        elevationValues.TrimExcess();

        //
        meshGenerated = true;
    }

    public IEnumerable Regenerate()
    {
        Dirty = false;
        //
        /*
        int tilesCount = WorldGrid.TilesCount;
        int i = 0;
        while (i < tilesCount)
        {
            var pos = WorldGrid.GetTileCenter(i);
            var val = ModuleResult.GetValue(pos);

            if (val <= 0)
            {
                i++;
                continue;
            }
            var color = val > 0.1f ? Color.cyan : Color.white;
            LayerSubMesh subMesh = GetSubMesh(TeleContent.WorldTerrain, color);

            Vector3 vector = WorldGrid.GetTileCenter(i);
            WorldRendererUtility.PrintQuadTangentialToPlanet(vector, vector, WorldGrid.averageTileSize, 0.0005f, subMesh);
            i++;
        }
        */

        /*
        int tilesCount = WorldGrid.TilesCount;
        for (int i = 0; i < tilesCount; i++)
        {
            var result = ModuleResult.GetValue(WorldRendererUtility.)
        }
        for (var i = 0; i < subMeshes.Count; i++)
        {
            var mesh = subMeshes[i];
            mesh.SetColor(Color.Lerp(Color.green, Color.red, (float)(i/(float)subMeshes.Count)));
        }

        int tilesCount = WorldGrid.TilesCount;
        for (int i = 0; i < tilesCount; i++)
        {
            var pos = WorldGrid.GetTileCenter(i);
            var val = ModuleResult.GetValue(pos);

            var color = val > 0.1f ? Color.white : Color.clear;
            subMeshes[i].SetColor(color);
        }
        */
        yield break;
    }

    private IEnumerable RegenerateMeshColliders()
    {
        meshCollidersInOrder.Clear();
        var gameObject = WorldTerrainColliderManager.GameObject;
        var components = gameObject.GetComponents<MeshCollider>();
        int j;
        for (j = 0; j < components.Length; j++) Object.Destroy(components[j]);
        for (var i = 0; i < subMeshes.Count; i = j + 1)
        {
            var meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = subMeshes[i].mesh;
            meshCollidersInOrder.Add(meshCollider);
            yield return null;
            j = i;
        }
    }

    private IEnumerable CalculateInterpolatedVerticesParams()
    {
        elevationValues.Clear();
        var grid = WorldGrid;
        var tilesCount = grid.TilesCount;
        var verts = grid.verts;
        var tileIDToVerts_offsets = grid.tileIDToVerts_offsets;
        var tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
        var tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
        //List<Tile> tiles = grid.tiles;
        int num4;
        for (var i = 0; i < tilesCount; i = num4 + 1)
        {
            //Tile tile = tiles[i];
            //float elevation = tile.elevation;
            var num = i + 1 < tileIDToNeighbors_offsets.Count
                ? tileIDToNeighbors_offsets[i + 1]
                : tileIDToNeighbors_values.Count;
            var num2 = i + 1 < tilesCount ? tileIDToVerts_offsets[i + 1] : verts.Count;
            for (var j = tileIDToVerts_offsets[i]; j < num2; j++)
            {
                var vector = default(Vector3);
                vector.x = 0; // tile.elevation
                var flag = false;
                for (var k = tileIDToNeighbors_offsets[i]; k < num; k++)
                {
                    var num3 = tileIDToNeighbors_values[k] + 1 < tileIDToVerts_offsets.Count
                        ? tileIDToVerts_offsets[tileIDToNeighbors_values[k] + 1]
                        : verts.Count;
                    var l = tileIDToVerts_offsets[tileIDToNeighbors_values[k]];
                    while (l < num3)
                        if (verts[l] == verts[j])
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
                        else
                            l++;
                }

                if (flag) vector.x = 0f;
                /*
                if (tile.biome.DrawMaterial.shader != ShaderDatabase.WorldOcean && vector.x < 0f)
                {
                    vector.x = 0f;
                }
                */
                elevationValues.Add(vector);
            }

            if (i % 1000 == 0) yield return null;
            num4 = i;
        }
    }

    private IEnumerable OldRegen()
    {
        Rand.PushState();
        Rand.Seed = Seed;
        var tilesCount = WorldGrid.TilesCount;
        var i = 0;
        while (i < tilesCount)
        {
            var pos = WorldGrid.GetTileCenter(i);
            var coverage = ModuleResult.GetValue(pos);
            if (coverage <= 0)
            {
                i++;
                continue;
            }
            //TiberiumTile tibTile = Find.World.worldObjects.WorldObjectAt<TiberiumTile>(i);

            LayerSubMesh subMesh = GetSubMesh(TeleContent.WorldTerrain, new Color(1, 1, 1, coverage));
            var vector = WorldGrid.GetTileCenter(i);
            var posForTangents = vector;
            //float magnitude = vector.magnitude;
            //vector = (vector + Rand.UnitVector3 * worldGrid.averageTileSize).normalized * magnitude;
            WorldRendererUtility.PrintQuadTangentialToPlanet(vector, posForTangents, 1.35f * WorldGrid.averageTileSize,
                0.005f, subMesh, false, true, false);
            WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, 2), Rand.Range(0, 2), 2, 2, subMesh);

            i++;
        }

        Rand.PopState();
        FinalizeMesh(MeshParts.All);
        yield break;
    }

    public void Render()
    {
        if (ShouldRegenerate) RegenerateNow();

        var layer = Layer;
        var rotation = Rotation;
        for (var i = 0; i < subMeshes.Count; i++)
            if (subMeshes[i].finalized)
            {
                var color = subMeshes[i].color;
                propertyBlock.SetColor(ShaderPropertyIDs.Color, color);
                Graphics.DrawMesh(subMeshes[i].mesh, Vector3.zero, rotation, subMeshes[i].material, layer, null, 0,
                    propertyBlock);
            }
    }

    protected LayerSubMeshColor GetSubMesh(Material material, Color color)
    {
        int num;
        return GetSubMesh(material, color, out num);
    }

    protected LayerSubMeshColor GetSubMesh(Material material, Color color, out int subMeshIndex)
    {
        for (var i = 0; i < subMeshes.Count; i++)
        {
            var layerSubMesh = subMeshes[i];
            if (layerSubMesh.material == material && layerSubMesh.verts.Count < 40000)
            {
                subMeshIndex = i;
                return layerSubMesh;
            }
        }

        var mesh = new Mesh();
        if (UnityData.isEditor) mesh.name = $"SimpleWorldLayerSubMesh_{GetType().Name}_{Find.World.info.seedString}";

        var layerSubMesh2 = new LayerSubMeshColor(mesh, material, null, color);
        subMeshIndex = subMeshes.Count;
        subMeshes.Add(layerSubMesh2);
        return layerSubMesh2;
    }

    protected void FinalizeMesh(MeshParts tags)
    {
        for (var i = 0; i < subMeshes.Count; i++)
            if (subMeshes[i].verts.Count > 0)
                subMeshes[i].FinalizeMesh(tags);
    }

    public void SetDirty()
    {
        Dirty = true;
    }

    private void ClearSubMeshes(MeshParts parts)
    {
        for (var i = 0; i < subMeshes.Count; i++) subMeshes[i].Clear(parts);
    }
}