using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim;

[StaticConstructorOnStartup]
public class TiberiumMapRenderer
{
    public TiberiumFieldFogLayer[] fogLayers;
    public Map map;

    public TiberiumMapRenderer(Map map)
    {
        this.map = map;
        var tiberium = map.Tiberium();
        var grids = tiberium.TiberiumInfo.TiberiumGrid;
        fogLayers = new TiberiumFieldFogLayer[3]
        {
            new(MainTCD.Main.GreenColor, grids.fieldColorGrids[0]),
            new(MainTCD.Main.BlueColor, grids.fieldColorGrids[1]),
            new(MainTCD.Main.RedColor, grids.fieldColorGrids[2])
        };
    }

    public void DrawAllTiberiumLayers()
    {
        foreach (var fogLayer in fogLayers) fogLayer.DrawFieldFog(map);
    }
}

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
        Graphics.DrawMesh(FieldMesh, position, Quaternion.identity, FogOverlayWorld, 0);
    }
}