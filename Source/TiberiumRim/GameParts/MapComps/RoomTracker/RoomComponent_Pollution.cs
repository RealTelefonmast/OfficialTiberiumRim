using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class RoomComponent_Pollution : RoomComponent
    {
        private int markedDirty, width, height;
        private IntVec2 size;
        private IntVec3 minVec;
        private Vector3 actualCenter;
        private Vector3 drawPos;

        private readonly HashSet<IntVec3> borderCells = new HashSet<IntVec3>();
        private readonly HashSet<IntVec3> thinRoofCells = new HashSet<IntVec3>();

        private FlowRenderer renderer;
        private VectorField vectorField;
        private PollutionContainer pollutionContainer;

        public Dictionary<RoomComponent_Pollution, List<PollutionConnector>> neighbourConnections = new Dictionary<RoomComponent_Pollution, List<PollutionConnector>>();
        public List<PollutionConnector> connections = new List<PollutionConnector>();

        //
        public TiberiumPollutionMapInfo PollutionInfo => Map.Tiberium().PollutionInfo;

        public HashSet<IntVec3> BorderCellsNoCorners => borderCells;
        public List<PollutionConnector> Connections => UsesOutDoorPollution ? PollutionInfo.ConnectionsToOutside : connections;


        public PollutionContainer Outside => PollutionInfo.OutsideContainer;
        public PollutionContainer ActualContainer => pollutionContainer;
        public PollutionContainer UsedContainer => UsesOutDoorPollution ? Outside : ActualContainer;

        public int ActualPollution => ActualContainer.Pollution;
        public float ActualSaturation => ActualContainer.Saturation;

        public IntVec3 MinVec => minVec;
        public IntVec2 Size => size;

        public int Pollution
        {
            get => UsedContainer.Pollution;
            set => UsedContainer.Pollution = value;
        }

        public float Saturation => UsedContainer.Saturation;

        public bool UsesOutDoorPollution => Parent.IsOutside;
        public bool IsDirty => markedDirty > 0;
        //

        //CREATION 
        public override void Create(RoomTracker parent)
        {
            base.Create(parent);
            //Notify Parent Manager
            PollutionInfo.Notify_NewComp(this);
            Log.Message($"Creating Pollution RoomComp for {Room.ID} with {Room.CellCount} cells. (AllCompCount: {PollutionInfo.AllComps.Count})");

            //Create new comps
            pollutionContainer = new PollutionContainer();
            renderer = new FlowRenderer();
            vectorField = new VectorField(Map);

            if (UsesOutDoorPollution) return;
            //Assign starting pollution based on position
            if (PollutionInfo.Cache.TryGetAverageRoomPollution(Room, out int pollution))
            {
                //We know we are not outside, use actual
                ActualContainer.Pollution = pollution;
            }
        }

        public override void Disband(RoomTracker parent, Map map)
        {
            Log.Message($"Disbanding {Room.ID}");
            var pollInfo = map.Tiberium().PollutionInfo;
            pollInfo.Notify_DisbandedComp(this);
            foreach (var connector in connections)
            {
                pollInfo.Notify_RemoveConnection(connector);
            }

            if (UsesOutDoorPollution)
            {
                for (var i = pollInfo.ConnectionsToOutside.Count - 1; i >= 0; i--)
                {
                    var pollutionConnector = pollInfo.ConnectionsToOutside[i];
                    pollutionConnector.Other(this).RegenerateData(true, true, true);
                }
            }
        }

        public override void Notify_Reused() { }

        public override void Notify_RoofClosed()
        {
            PollutionInfo.RegenerateOutside();
            pollutionContainer.RegenerateData(Room.CellCount);
        }

        public override void Notify_RoofOpened()
        {
            if (ActualPollution > 0)
            {
                Outside.Pollution += ActualPollution;
                ActualContainer.Pollution -= ActualPollution;
            }
        }

        public override void Notify_RoofChanged()
        {
            RegenerateData(false, true, true);
        }

        public override void PreApply()
        {
            Log.Message($"Pre-appyling for {Room.ID}");
            MarkDirty();
        }

        public override void FinalizeApply()
        {
            Log.Message($"Finalizing Apply for {Room.ID}");
            RegenerateData();
        }

        private void MarkDirty()
        {
            if (markedDirty < 0)
                markedDirty = 0;
            markedDirty++;
        }

        public bool TryAddPollution(int amount, out int actuallyAdded)
        {
            return UsedContainer.TryAddValue(amount, out actuallyAdded);
        }

        public bool TryRemovePollution(int amount, out int actuallyRemoved)
        {
            return UsedContainer.TryRemoveValue(amount, out actuallyRemoved);
        }

        //Equalization Logic
        public void Equalize()
        {
            //
            if (ActualPollution <= 0) return;

            //Equalize
            if (Parent.OpenRoofCount <= 0) return;
            if (Outside.FullySaturated) return;

            if (ShouldPushToOther(ActualSaturation, Outside.Saturation))
            {
                int from = ActualPollution, to = Outside.Pollution;
                TryPushToOther(ref @from, ref to, PushAmountToOther(ActualSaturation, Outside.Saturation, TiberiumPollutionMapInfo.CELL_CAPACITY * Parent.OpenRoofCount));

                //Moving from actual container to outside or vice versa
                ActualContainer.Pollution = @from;
                Outside.Pollution = to;
            }
            
        }

        public int PushAmountToOther(float saturation, float otherSaturation, int throughPutCap, float factor = 1)
        {
            return Mathf.RoundToInt((throughPutCap * (saturation - otherSaturation)) * factor);
        }

        public int ThroughPut(float pressureA, float pressureB, int viscosity = 1, float length = 1, int crossSection = 100)
        {
            return Mathf.RoundToInt((10000f * (pressureA - pressureB)) / (8f * (float)Math.PI * length));
        }

        public bool ShouldPushToOther(float saturation, float otherSaturation)
        {
            return (saturation - otherSaturation) > 0f;
        }

        public void TryPushToOther(ref int from, ref int to, int value)
        {
            int actualValue = value;
            if (value > from)
                actualValue = from;
            from -= actualValue;
            to += actualValue;
        }

        public void Notify_FlowChanged()
        {
            //No More Flow For Now
            return;
            if (Parent.IsDisbanded)
            {
                Log.Message("Trying to regen disbanded room!");
                return;
            }

            if (size.x <= 0 && size.z <= 0)
            {
                Log.Message($"Trying to regen 0-sized room! {this.Room.ID}");
                return;
            }
        }

        //DATA
        public void RegenerateData(bool ignoreOthers = false, bool onlyConnections = false, bool force = false)
        {
            Log.Message($"Regenerating {Room.ID} with ignOth: {ignoreOthers} onlyConn: {onlyConnections} force: {force} | IsDirty: {IsDirty}");
            if (!force && !IsDirty) return;
            if (Parent.IsDisbanded)
            {
                Log.Warning("Tried to regenerate disbanded room comp!");
                return;
            }

            //Special Outdoor Case
            if (UsesOutDoorPollution)
            {
                PollutionInfo.RegenerateOutside();
                ActualContainer.RegenerateData(Room.CellCount);

                markedDirty--;
                return;
            }

            if (!onlyConnections || !borderCells.Any())
            {
                int minX = Room.Cells.MinBy(t => t.x).x;
                int maxX = Room.Cells.MaxBy(t => t.x).x;
                int minZ = Room.Cells.MinBy(t => t.z).z;
                int maxZ = Room.Cells.MaxBy(t => t.z).z;
                this.width = maxX - minX + 1;
                this.height = maxZ - minZ + 1;

                Log.Message(
                    $"Setting size for {Room.ID} - minX: {minX}|maxX: {maxX}|minZ: {minZ}|maxZ: {maxZ} - {width}/{height}");
                size = new IntVec2(width, height);
                minVec = new IntVec3(minX, 0, minZ);
                actualCenter = new Vector3(minX + (width / 2f), 0, minZ + (height / 2f));
                drawPos = new Vector3(minX, AltitudeLayer.FogOfWar.AltitudeFor(), minZ);

                //Get Cell Data
                GenerateCellData();
                ActualContainer.RegenerateData(Parent.CellCount);
                renderer.UpdateMesh(Room.Cells, minVec, width, height);
            }

            foreach (var connector in connections)
            {
                PollutionInfo.Notify_RemoveConnection(connector);
            }

            neighbourConnections.Clear();
            connections.Clear();

            foreach (var cell in BorderCellsNoCorners)
            {
                //Get All Connectors
                if (!cell.InBounds(Map)) continue;
                var building = cell.GetFirstBuilding(Map);
                if (building == null) continue;
                if (!IsPassBuilding(building)) continue;
                var otherRoom = OppositeRoomFrom(building.Position);
                if (otherRoom == null) continue;
                var otherPollution = otherRoom.Pollution();
                if (otherPollution == null) continue;

                var newConn = SetNewConnection(otherPollution, building);
                PollutionInfo.Notify_AddConnection(newConn);
                if (!ignoreOthers)
                {
                    otherPollution.MarkDirty();
                    otherPollution.RegenerateData(true, true);
                }
            }

            /*
                Action action = delegate
                {
                    var tex = vectorField.GetTextureFor(this);
                    renderer.SetFlowMap(tex);
                };
                action.EnqueueActionForMainThread();
                */

            markedDirty--;
        }

        public PollutionConnector SetNewConnection(RoomComponent_Pollution toOther, Building connection)
        {
            if (!neighbourConnections.ContainsKey(toOther))
            {
                neighbourConnections.Add(toOther, new List<PollutionConnector>());
            }

            var cells = GenAdj.CellsAdjacentCardinal(connection);

            var newConn = new PollutionConnector(connection, this, toOther);
            connections.Add(newConn);
            neighbourConnections[toOther].Add(newConn);
            return newConn;
            /*
            if (connection is Building_Door)
                ConnectedDoors.Add(newPasser);
            ConnectedTrackers[toTracker].Add(newPasser);
            */
        }

        private Room OppositeRoomFrom(IntVec3 cell)
        {
            for (int i = 0; i < 4; i++)
            {
                Room room = (cell + GenAdj.CardinalDirections[i]).GetRoom(Map);
                if (room == null || room == Room) continue;
                return room;
            }
            return null;
        }

        private bool IsPassBuilding(Building building)
        {
            return building switch
            {
                Building_Door _ => true,
                Building_Vent _ => true,
                Building_Cooler _ => true,
                Building b => b.def.fillPercent < 1f,
                _ => false
            };
        }

        private void GenerateCellData()
        {
            borderCells.Clear();
            thinRoofCells.Clear();

            foreach (IntVec3 c in Room.Cells)
            {
                if (!Map.roofGrid.RoofAt(c)?.isThickRoof ?? false)
                    thinRoofCells.Add(c);

                for (int i = 0; i < 4; i++)
                {
                    IntVec3 cardinal = c + GenAdj.CardinalDirections[i];

                    var region = cardinal.GetRegion(Map);
                    if (region == null || region.Room != Room)
                    {
                        borderCells.Add(cardinal);
                    }
                }
            }
        }

        //RENDERING
        public override void OnGUI()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                if (Room.CellCount <= 0) return;
                IntVec3 first = Room.Cells.First();
                Vector3 v = (GenMapUI.LabelDrawPosFor(first)) + new Vector2(0, -0.75f);
                GenMapUI.DrawThingLabel(v, Room.ID + "[" + Pollution + "]{" + UsedContainer.TotalCapacity + "}", Color.red);
            }
            //vectorField.OnGUI();
        }

        public override void Draw()
        {
            if (!UsesOutDoorPollution)
            {
                renderer.Draw(drawPos, Saturation);
                //vectorField.DrawVectors();
            }
            if (Room.Cells.Contains(UI.MouseCell()))
            {
                GenDraw.DrawFieldEdges(Room.Cells.ToList(), Color.cyan);
                GenDraw.DrawFieldEdges(BorderCellsNoCorners.ToList(), Color.blue);

                if (Connections.Any())
                {
                    GenDraw.DrawFieldEdges(Connections.Select(t => t.Building.Position).ToList(), Color.green);
                    GenDraw.DrawFieldEdges(Connections.SelectMany(t => t.Other(this).Room.Cells).ToList(), Color.red);
                }
            }

            if (Room.CellCount <= 0) return;
            var vec = Room.Cells.First().ToVector3();
            GenDraw.FillableBarRequest r = default;
            r.center = vec + new Vector3(0.25f, 0, 0.5f);
            r.size = new Vector2(1f, 0.5f);
            r.rotation = Rot4.East;
            r.fillPercent = PollutionInfo.OutsideContainer.Saturation;
            r.filledMat = TiberiumContent.GreenMaterial;
            r.unfilledMat = TiberiumContent.ClearMaterial;
            r.margin = 0.125f;
            GenDraw.DrawFillableBar(r);

            if (ActualContainer.Pollution > 0)
            {
                GenDraw.FillableBarRequest r2 = default;
                r2.center = vec + new Vector3(0.75f, 0, 0.5f);
                r2.size = new Vector2(1f, 0.5f);
                r2.rotation = Rot4.East;
                r2.fillPercent = ActualContainer.Saturation;
                r2.filledMat = TiberiumContent.RedMaterial;
                r2.unfilledMat = TiberiumContent.ClearMaterial;
                r2.margin = 0.125f;
                GenDraw.DrawFillableBar(r2);
            }
        }
    }


    public class FlowRenderer
    {
        private Material cachedMat;
        private Mesh cachedMesh;

        public static float MainAlpha = 0.8f;


        [TweakValue("DrawPollution_Tiling", 0.01f, 20f)]
        public static float Tiling = 0.15f;

        [TweakValue("DrawPollution_FlowSpeed", 0f, 2f)]
        public static float FlowSpeed = 0.5f;

        [TweakValue("DrawPollution_BlendSpeed", 0f, 2f)]
        public static float BlendSpeed = 1f;

        [TweakValue("DrawPollution_BlendValue", 0f, 1f)]
        public static float BlendValue = 0.45f;

        [TweakValue("DrawPollution_Alpha", 0f, 1f)]
        public static float Alpha = 1f;

        [TweakValue("DrawPollution_Override", 0, 1)]
        public static int Override = 0;

        public Material Material
        {
            get
            {
                if (cachedMat == null)
                {
                    cachedMat = new Material(TiberiumContent.TextureBlend);
                    cachedMat.SetTexture("_MainTex1", TiberiumContent.Nebula1);
                    cachedMat.SetTexture("_MainTex2", TiberiumContent.Nebula2);
                    cachedMat.SetColor("_Color", new ColorInt(0, 255, 97, 255).ToColor);
                }

                cachedMat.SetFloat("_Tiling", Tiling);
                cachedMat.SetFloat("_BlendValue", BlendValue);
                cachedMat.SetFloat("_TimeSpeed", (int)Find.TickManager.CurTimeSpeed);
                cachedMat.SetFloat("_BlendSpeed", BlendSpeed);
                cachedMat.SetFloat("_Opacity", MainAlpha * Alpha);

                return cachedMat;
            }
        }


        private List<IntVec3> OffsetIntvecs(IEnumerable<IntVec3> cells, IntVec3 reference)
        {
            List<IntVec3> offsetVecs = new List<IntVec3>();
            foreach (IntVec3 c in cells)
            {
                offsetVecs.Add(c - reference);
            }

            return offsetVecs;
        }

        //Mesh Creation
        private int Compare(IntVec3 a, IntVec3 b, int gridWidth)
        {
            var aValue = a.x + (a.z * gridWidth);
            var bValue = b.x + (b.z * gridWidth);
            return aValue - bValue;
        }

        public void UpdateMesh(IEnumerable<IntVec3> cells, IntVec3 reference, int width, int height)
        {
            Action action = delegate { cachedMesh = GetMesh(cells, reference, width, height); };
            action.EnqueueActionForMainThread();
        }

        private Mesh GetBorderMesh(IEnumerable<IntVec3> borderCells, IntVec3 reference, int width, int height)
        {

            Mesh mesh = new Mesh();
            return mesh;
        }

        private Mesh GetMesh(IEnumerable<IntVec3> cells, IntVec3 reference, int width, int height)
        {
            int xSize = width;
            int zSize = height;
            var offsetCells = OffsetIntvecs(cells, reference);
            offsetCells.Sort((a, b) => Compare(a, b, xSize));

            int[] triangles = new int[xSize * zSize * 6];
            Vector3[] verts = new Vector3[(xSize + 1) * (zSize + 1)];
            Vector2[] uvs = new Vector2[verts.Length];

            foreach (var cell in offsetCells)
            {
                int x = cell.x;
                int z = cell.z;

                var vert = x + (z * xSize);
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

            Mesh mesh = new Mesh();
            mesh.name = "CustomRoomMesh";
            mesh.vertices = verts;
            mesh.uv = uvs;

            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        public void Draw(Vector3 drawPos, float sat)
        {
            if (cachedMesh == null) return;
            MainAlpha = sat;

            Matrix4x4 matrix = default;
            matrix.SetTRS(drawPos, Quaternion.identity, Vector3.one);
            Graphics.DrawMesh(cachedMesh, matrix, Material, 0);
        }
    }

}
