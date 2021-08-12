using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class TiberiumMapRenderer
    {
        public Map map;
        public TiberiumFieldFogLayer[] fogLayers;

        public TiberiumMapRenderer(Map map)
        {
            this.map = map;
            MapComponent_Tiberium tiberium = map.Tiberium();
            var grids = tiberium.TiberiumInfo.TiberiumGrid;
            fogLayers = new TiberiumFieldFogLayer[3] {
            new TiberiumFieldFogLayer(MainTCD.Main.GreenColor, grids.fieldColorGrids[0]),
            new TiberiumFieldFogLayer(MainTCD.Main.BlueColor, grids.fieldColorGrids[1]),
            new TiberiumFieldFogLayer(MainTCD.Main.RedColor, grids.fieldColorGrids[2]),
            };
        }

        public void DrawAllTiberiumLayers()
        {
            foreach(var fogLayer in fogLayers)
            {
                fogLayer.DrawFieldFog(map);
            }
        }
    }

    [StaticConstructorOnStartup]
    public class TiberiumFieldFogLayer
    {
        public Color mainColor;
        public BoolGrid mainGrid;

        public TiberiumFieldFogLayer(Color color, BoolGrid grid)
        {
            mainColor = color;
            mainGrid = grid;
        }

        private static readonly Material FogOverlayWorld = MatLoader.LoadMat("Weather/FogOverlayWorld", -1);

        private bool fieldMeshDirty = true;
        private Mesh lastFieldMesh;

        private void UpdateFieldMesh()
        {
            var cells = mainGrid.ActiveCells;
            int minX = cells.Min(i => i.x);
            int maxX = cells.Max(i => i.x); 
            int minY = cells.Min(i => i.x);
            int maxY = cells.Max(i => i.x);
            int height = maxY - minY;
            int width = maxX - minX;

            int vertexCount = (width + 1) * (height + 1);

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uv = new Vector2[] { };
            int[] triangles = new int[cells.Count() * 6];

            for(int i = 0, y = 0; y <= height; y++)
            {
                for(int x = 0; x <= width; x++)
                {
                    vertices[i] = new Vector3(x, 0, y);
                    i++;
                }
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

        public Mesh FieldMesh
        {
            get
            {
                if (fieldMeshDirty)
                    UpdateFieldMesh();
                return lastFieldMesh;
            }
        }

        public void DrawFieldFog(Map map)
        {
            Vector3 position = map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather);
            Graphics.DrawMesh(FieldMesh, position, Quaternion.identity, FogOverlayWorld, 0);
        }
    }
}
