using System.Linq;
using UnityEngine;
using Verse;

namespace TR.TiberiumEnvironment;

[StaticConstructorOnStartup]
public class TiberiumFieldFogLayer
{
    private static readonly Material FogOverlayWorld = MatLoader.LoadMat("Weather/FogOverlayWorld");

    private bool fieldMeshDirty = true;
    private Mesh lastFieldMesh;
    public Color mainColor;
    public BoolGrid mainGrid;

    public TiberiumFieldFogLayer(Color color, BoolGrid grid)
    {
        mainColor = color;
        mainGrid = grid;
    }

    public Mesh FieldMesh
    {
        get
        {
            if (fieldMeshDirty)
                UpdateFieldMesh();
            return lastFieldMesh;
        }
    }

    public Mesh FieldMesh
    {
        get
        {
            if (fieldMeshDirty)
                UpdateFieldMesh();
            return lastFieldMesh;
        }
    }

    private void UpdateFieldMesh()
    {
        var cells = mainGrid.ActiveCells;
        var minX = cells.Min(i => i.x);
        var maxX = cells.Max(i => i.x);
        var minY = cells.Min(i => i.x);
        var maxY = cells.Max(i => i.x);
        var height = maxY - minY;
        var width = maxX - minX;

        var vertexCount = (width + 1) * (height + 1);

        var vertices = new Vector3[vertexCount];
        var uv = new Vector2[] { };
        var triangles = new int[cells.Count() * 6];

        for (int i = 0, y = 0; y <= height; y++)
        for (var x = 0; x <= width; x++)
        {
            vertices[i] = new Vector3(x, 0, y);
            i++;
        }

        lastFieldMesh = new Mesh();
        lastFieldMesh.name = "NewPlaneMesh()";
        lastFieldMesh.vertices = vertices;
        lastFieldMesh.uv = uv;
        lastFieldMesh.SetTriangles(triangles, 0);
        lastFieldMesh.RecalculateNormals();
        lastFieldMesh.RecalculateBounds();

        fieldMeshDirty = false;
    }

    public void DrawFieldFog(Map map)
    {
        var position = map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather);
        UnityEngine.Graphics.DrawMesh(FieldMesh, position, Quaternion.identity, FogOverlayWorld, 0);
    }
}