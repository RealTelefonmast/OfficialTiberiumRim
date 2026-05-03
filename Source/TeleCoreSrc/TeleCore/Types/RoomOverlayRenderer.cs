using System;
using System.Collections.Generic;
using LudeonTK;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

/// <summary>
/// </summary>
public class RoomOverlayRenderer
{
    public static float MainAlpha = 0.8f;

    [TweakValue("RoomOverlay_Tiling", 0.01f, 20f)]
    public static float Tiling = 0.15f;

    protected Material cachedMat;
    protected Mesh cachedMesh;

    public virtual Shader Shader => null;

    public Material Material
    {
        get
        {
            if (cachedMat == null)
            {
                var shader = Shader ?? ShaderDatabase.Transparent;
                cachedMat = new Material(shader); //TRContentDatabase.TextureBlend
                InitShaderProps(cachedMat);
            }

            UpdateShaderProps(cachedMat);
            return cachedMat;
        }
    }

    protected virtual void InitShaderProps(Material material)
    {
    }

    protected virtual void UpdateShaderProps(Material material)
    {
        material.SetFloat("_Tiling", Tiling);
    }

    public void UpdateMesh(IEnumerable<IntVec3> cells, IntVec3 reference, int width, int height)
    {
        Action action = delegate { cachedMesh = GetMesh(cells, reference, width, height); };
        action.EnqueueActionForMainThread();
    }

    private Mesh GetMesh(IEnumerable<IntVec3> cells, IntVec3 reference, int width, int height)
    {
        var xSize = width;
        var zSize = height;
        var offsetCells = CellGen.OffsetIntvecs(cells, reference);
        offsetCells.Sort((a, b) => a.Compare(b, xSize));

        var triangles = new int[xSize * zSize * 6];
        var verts = new Vector3[(xSize + 1) * (zSize + 1)];
        var uvs = new Vector2[verts.Length];

        foreach (var cell in offsetCells)
        {
            var x = cell.x;
            var z = cell.z;

            var vert = x + z * xSize;
            var tris = vert * 6;

            var vecs = cell.CornerVec3s();
            int BL = vert + z,
                BR = vert + z + 1,
                TL = vert + z + xSize + 1,
                TR = vert + z + xSize + 2;

            verts[BL] = vecs[0]; //00
            verts[BR] = vecs[1]; //10
            verts[TL] = vecs[2]; //01
            verts[TR] = vecs[3]; //11

            uvs[BL] = new Vector2(verts[BL].x / xSize, verts[BL].z / zSize);
            uvs[BR] = new Vector2(verts[BR].x / xSize, verts[BR].z / zSize);
            uvs[TL] = new Vector2(verts[TL].x / xSize, verts[TL].z / zSize);
            uvs[TR] = new Vector2(verts[TR].x / xSize, verts[TR].z / zSize);

            triangles[tris + 0] = BL;
            triangles[tris + 1] = TL;
            triangles[tris + 2] = BR;
            triangles[tris + 3] = BR;
            triangles[tris + 4] = TL;
            triangles[tris + 5] = TR;
        }

        var mesh = new Mesh();
        mesh.name = "CustomRoomMesh";
        mesh.vertices = verts;
        mesh.uv = uvs;

        mesh.SetTriangles(triangles, 0);
        return mesh;
    }

    //
    public void Draw(Vector3 drawPos, float sat)
    {
        if (cachedMesh == null) return;
        MainAlpha = sat;

        Matrix4x4 matrix = default;
        matrix.SetTRS(drawPos, Quaternion.identity, Vector3.one);
        Graphics.DrawMesh(cachedMesh, matrix, Material, 0);
    }
}