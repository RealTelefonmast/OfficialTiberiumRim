using Verse;

namespace TiberiumRim
{
    public class SectionLayer_TiberiumGas : SectionLayer
    {
        public SectionLayer_TiberiumGas(Section section) : base(section)
        {
        }

        public override bool Visible => true;

        public override void DrawLayer()
        {
            base.DrawLayer();
        }

        public override void Regenerate()
        {
            /*
            float y = AltitudeLayer.FogOfWar.AltitudeFor();

            TiberiumPollutionMapInfo pollution = Map.Tiberium().PollutionInfo;
            CellRect cellRect = new CellRect(this.section.botLeft.x, this.section.botLeft.z, 17, 17);
            cellRect.ClipInsideMap(base.Map);
            base.ClearSubMeshes(MeshParts.All);
            foreach (IntVec3 cell in cellRect)
            {
                int i = cell.x;
                int j = cell.z;

                PollutionTracker trackerAt = pollution.PollutionFor(cell.GetRoom(Map));
                if(trackerAt == null) continue;

                LayerSubMesh subMesh = base.GetSubMesh(trackerAt.renderer.AssetFlowMapMat);
                int count = subMesh.verts.Count;
                subMesh.verts.Add(new Vector3((float)i, y, (float)j));
                subMesh.verts.Add(new Vector3((float)i, y, (float)(j + 1)));
                subMesh.verts.Add(new Vector3((float)(i + 1), y, (float)(j + 1)));
                subMesh.verts.Add(new Vector3((float)(i + 1), y, (float)j));
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 1);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count);
                subMesh.tris.Add(count + 2);
                subMesh.tris.Add(count + 3);
            }
            base.FinalizeMesh(MeshParts.Verts | MeshParts.Tris);
            */
        }
    }
}
