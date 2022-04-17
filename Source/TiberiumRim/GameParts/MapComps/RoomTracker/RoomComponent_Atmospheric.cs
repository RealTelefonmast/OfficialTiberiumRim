using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class RoomComponent_Atmospheric : RoomComponent
    {
        private int markedDirty;
        //
        private bool menuActive = false;

        private RoomOverlay_Atmospheric renderer;
        private VectorField vectorField;

        private AtmosphericContainer valueContainer;
        private AtmosphericConnector connector;

        public Dictionary<RoomComponent_Atmospheric, List<AtmosphericConnector>> neighbourConnections = new Dictionary<RoomComponent_Atmospheric, List<AtmosphericConnector>>();
        public List<AtmosphericConnector> connections = new List<AtmosphericConnector>();

        //
        public AtmosphericMapInfo AtmosphericInfo => Map.Tiberium().AtmosphericInfo;

        public List<AtmosphericConnector> Connections => IsOutdoors ? AtmosphericInfo.ConnectionsToOutside : connections;

        public AtmosphericContainer Outside => AtmosphericInfo.OutsideContainer;
        public AtmosphericContainer ActualContainer => valueContainer;
        public AtmosphericContainer UsedContainer => IsOutdoors ? Outside : ActualContainer;

        public int UsedValue => UsedContainer.Value;
        public float UsedSaturation => UsedContainer.Saturation;

        public int ActualValue => ActualContainer.Value;
        public float ActualSaturation => ActualContainer.Saturation;

        public Dictionary<NetworkValueDef, int> Values
        {
            get
            {
                var returnDict = new Dictionary<NetworkValueDef, int>();
                foreach (var values in ActualContainer.Container.StoredValuesByType)
                {
                    returnDict.Add(values.Key, Mathf.RoundToInt(values.Value));
                }
                return returnDict;
            }
        } 

        public int TotalValue
        {
            get => UsedContainer.Value;
            //set => UsedContainer.Pollution = value;
        }

        public float Saturation => UsedContainer.Saturation;

        public bool IsOutdoors => Parent.IsOutside;
        public bool IsConnector => connector != null;
        public bool IsDirty => markedDirty > 0;
        public bool CanHaveTangibleGas => IsOutdoors || !ActualContainer.FullySaturated;

        //CREATION 
        public override void Create(RoomTracker parent)
        {
            base.Create(parent);
            //Notify Parent Manager
            AtmosphericInfo.Notify_NewComp(this);

            //Create new comps
            valueContainer = new AtmosphericContainer(this);
            renderer = new RoomOverlay_Atmospheric();
            vectorField = new VectorField(Map);
        }

        public override void Disband(RoomTracker parent, Map map)
        {
            var atmosInfo = map.Tiberium().AtmosphericInfo;
            atmosInfo.Notify_DisbandedComp(this);
            if (connections.Count > 0)
            {
                for (var i = connections.Count - 1; i >= 0; i--)
                {
                    var connector = connections[i];
                    atmosInfo.Notify_RemoveConnection(connector);
                }
            }

            if (IsOutdoors && atmosInfo.ConnectionsToOutside.Count > 0)
            {
                for (var i = atmosInfo.ConnectionsToOutside.Count - 1; i >= 0; i--)
                {
                    var pollutionConnector = atmosInfo.ConnectionsToOutside[i];
                    pollutionConnector.Other(this).RegenerateData(true, true, true);
                }
            }
        }

        public override void Notify_Reused()
        {
            ActualContainer.Container.Clear();
        }

        public override void Notify_RoofClosed()
        {
            AtmosphericInfo.RegenerateOutside();
            ActualContainer.RegenerateData(this, Room.CellCount);
        }

        public override void Notify_RoofOpened()
        {
            if (ActualValue > 0)
            {
                ActualContainer.TransferAllTo(Outside);
            }
        }

        public override void Notify_RoofChanged()
        {
            RegenerateData(false, true, true);
        }

        public override void PreApply()
        {
            MarkDirty();
        }

        public override void FinalizeApply()
        {
            RegenerateData();
        }

        private void Notify_IsConnector(AtmosphericConnector connection)
        {
            this.connector = connection;
        }

        private void MarkDirty()
        {
            if (markedDirty < 0)
                markedDirty = 0;
            markedDirty++;
        }

        public bool TryAddValue(NetworkValueDef value, int amount, out float actualAmount)
        {
            //Log.Message($"Adding {value} ({amount}) to RoomComp {Room.ID} | Outside: {Outside}");
            return UsedContainer.Container.TryAddValue(value, amount, out actualAmount);
        }

        public bool TryRemoveValue(NetworkValueDef value, int amount, out float actualAmount)
        {
            return UsedContainer.Container.TryRemoveValue(value, amount, out actualAmount);
        }

        //Equalization Logic
        public void Equalize()
        {
            //
            if (ActualValue <= 0) return;

            //EqualizeWith
            if (Parent.OpenRoofCount <= 0) return;
            if (Outside.FullySaturated) return;

            if (ActualSaturation > Outside.Saturation)
            {
                ActualContainer.TryEqualize(Outside, 1f, out _);
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

            if (Parent.Size.x <= 0 && Parent.Size.z <= 0)
            {
                Log.Message($"Trying to regen 0-sized room! {this.Room.ID}");
                return;
            }
        }

        //DATA
        public void RegenerateData(bool ignoreOthers = false, bool onlyConnections = false, bool force = false)
        {
            //Log.Message($"Regenerating {Room.ID} with ignOth: {ignoreOthers} onlyConn: {onlyConnections} force: {force} | IsDirty: {IsDirty}");
            //Log.Message($"Regenerating {this.Room.ID}...");
            if (!force && !IsDirty) return;
            if (Parent.IsDisbanded)
            {
                Log.Warning("Tried to regenerate disbanded room comp!");
                return;
            }

            //Special Outdoor Case
            if (IsOutdoors)
            {
                AtmosphericInfo.RegenerateOutside();
                ActualContainer.RegenerateData(this, Room.CellCount);

                markedDirty--;
                return;
            }

            if (!onlyConnections)
            {
                ActualContainer.RegenerateData(this, Parent.CellCount);
                renderer.UpdateMesh(Room.Cells, Parent.MinVec, Parent.Size.x, Parent.Size.z);
            }

            foreach (var connector in connections)
            {
                AtmosphericInfo.Notify_RemoveConnection(connector);
            }

            neighbourConnections.Clear();
            connections.Clear();

            foreach (var cell in Parent.BorderCellsNoCorners)
            {
                //Get All Connectors
                if (!cell.InBounds(Map)) continue;
                var building = cell.GetThingList(Map).Select(t => t as Building).FirstOrFallback(IsPassBuilding, null);
                if (building == null) continue;
                var otherRoom = OppositeRoomFrom(building.Position);
                if (otherRoom == null) continue;
                var otherPollution = otherRoom.AtmosphericRoomComp();
                if (otherPollution == null) continue;

                var newConn = SetNewConnection(otherPollution, building);
                AtmosphericInfo.Notify_AddConnection(newConn);
                if (!ignoreOthers)
                {
                    otherPollution.MarkDirty();
                    otherPollution.RegenerateData(true, true);
                }
            }

            //Assign starting pollution based on position
            if (AtmosphericInfo.Cache.TryGetAtmosphericValuesForRoom(Room, out var info))
            {
                //We know we are not outside, use actual
                ActualContainer.Data_SetInfo(info);
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

        public AtmosphericConnector SetNewConnection(RoomComponent_Atmospheric toOther, Building connection)
        {
            if (!neighbourConnections.ContainsKey(toOther))
            {
                neighbourConnections.Add(toOther, new List<AtmosphericConnector>());
            }

            var newConn = new AtmosphericConnector(connection, this, toOther);
            connections.Add(newConn);
            neighbourConnections[toOther].Add(newConn);

            //Notify roomcomp as connector
            connection.GetRoom()?.AtmosphericRoomComp()?.Notify_IsConnector(newConn);

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
                Building b => b.def.IsEdifice() && b.def.fillPercent < 1f,
                _ => false
            };
        }

        //
        public void ToggleOverlay()
        {
            menuActive = !menuActive;
            if (!menuActive)
            {
                DrawDebug = false;
            }
        }

        //RENDERING
        public override void OnGUI()
        {
            /*
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                if (Room.CellCount <= 0) return;
                IntVec3 first = Room.Cells.First();
                Vector3 v = (GenMapUI.LabelDrawPosFor(first)) + new Vector2(0, -0.75f);
                GenMapUI.DrawThingLabel(v,  $"{Room.ID} [{ActualValue}]({UsedContainer.Container.Capacity})", Color.red);
            }
            */
            if (menuActive && Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && !IsOutdoors && !IsConnector)
            {
                if (Room.CellCount <= 0) return;
                DrawMenu(Room.Cells.First());

                /*
                Vector3 v = (GenMapUI.LabelDrawPosFor(first));
                UsedContainer.Container.ContainerGizmo.GizmoOnGUI(v, 500, new GizmoRenderParms()
                {
                    highLight = false,
                    lowLight = false,
                    shrunk = false,
                });
                */
            }
            //vectorField.OnGUI();
        }

        private void DrawMenu(IntVec3 pos)
        {
            var v = DrawPosFor(pos) - new Vector2(0, 69);

            /*
            if (minimized)
            {
                var smolRect = new Rect(v.x, v.y, 25, 15);
                TRWidgets.DrawColoredBox(smolRect, new Color(1, 1, 1, 0.125f), Color.white, 1);
                if (Widgets.ButtonInvisible(smolRect))
                {
                    minimized = false;
                }
                return;
            }
            */

            //46 - approx cell size when zoomed in

            var rect = new Rect(v.x, v.y, 115, 69);
            TRWidgets.DrawColoredBox(rect, new Color(1, 1, 1, 0.125f), Color.white, 1);

            rect = rect.ContractedBy(5);
            Widgets.BeginGroup(rect);

            var rect1 = new Rect(0, 0, 10, rect.height);
            var rect2 = new Rect(12.5f, 0, 10, rect.height);
            var rect3 = new Rect(50, 0, 20,20);
            Widgets.DrawHighlight(rect1);
            Widgets.DrawHighlight(rect2);

            Text.Font = GameFont.Tiny;
            //Rect labelRect = new Rect(0, 0, 0, 0);

            Text.Font = default;

            DrawPctBar(rect1, Outside);
            DrawPctBar(rect2, ActualContainer);

            if (Widgets.ButtonText(rect3, "X", false, true, true))
            {
                ActualContainer.Container.Clear();
            }

            Rect textRect = new Rect(22.5f, 0, rect.width - 22.5f, rect.height);
            GUI.color = Color.red;
            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(textRect, $"ID: {Room.ID}");

            GUI.color = Color.green;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(textRect, $"Val: {TotalValue}");

            GUI.color = Color.green;
            Text.Anchor = TextAnchor.LowerRight;
            Widgets.Label(textRect, $"Pct: {Saturation.ToStringPercent()}");

            GUI.color = Color.blue;
            Text.Anchor = TextAnchor.LowerLeft;
            Widgets.Label(textRect, $"[{containedPawns.Count}]");

            Text.Anchor = default;
            GUI.color = Color.white;

            Event curEvent = Event.current;
            if (Widgets.ButtonInvisible(rect.AtZero()))
            {
                if(curEvent.button == 1)
                    DrawDebug = !DrawDebug;
            }

            Widgets.EndGroup();

            /*
            Find.WindowStack.ImmediateWindow(Room.GetHashCode(), rect, WindowLayer.GameUI, delegate
            {
                rect = rect.ContractedBy(5);
                Widgets.DrawHighlight(rect);

                Widgets.BeginGroup(rect);

                var rect1 = new Rect(0,0, 20, rect.height);
                var rect2 = new Rect(25, 0, 20, rect.height);
                Widgets.DrawHighlight(rect1);
                Widgets.DrawHighlight(rect2);

                DrawPctBar(rect1, Outside);
                DrawPctBar(rect2, ActualContainer);
                Widgets.EndGroup();
            }, true, false, 0);
            */
        }

        private void DrawPctBar(Rect rect, AtmosphericContainer container)
        {
            Widgets.BeginGroup(rect);
            var actualContainer = container.Container;
            float yPos = rect.height;
            foreach (var type in actualContainer.AllStoredTypes)
            {
                float percent = (actualContainer.ValueForType(type) / actualContainer.Capacity);
                var height = rect.height * percent;
                Rect typeRect = new Rect(0, yPos - height, rect.width, height);
                yPos -= height;
                Widgets.DrawBoxSolid(typeRect, type.valueColor);
            }
            Widgets.EndGroup();
        }

        private Vector2 DrawPosFor(IntVec3 pos)
        {
            Vector3 position = new Vector3((float)pos.x, (float)pos.y + AltitudeLayer.MetaOverlays.AltitudeFor(), (float)pos.z);
            Vector2 vector = Find.Camera.WorldToScreenPoint(position) / Prefs.UIScale;
            vector.y = (float)UI.screenHeight - vector.y;
            return vector;
        }

        private static bool DrawDebug = false;

        public override void Draw()
        {
            if (!IsOutdoors)
            {
                renderer.Draw(Parent.DrawPos, Saturation);
                //vectorField.DrawVectors();
            }
            if (DrawDebug && Room.Cells.Contains(UI.MouseCell()))
            {
                GenDraw.DrawFieldEdges(Room.Cells.ToList(), Color.cyan);
                GenDraw.DrawFieldEdges(Parent.BorderCellsNoCorners.ToList(), Color.blue);

                if (Connections.Any())
                {
                    GenDraw.DrawFieldEdges(Connections.Select(t => t.Building.Position).ToList(), Color.green);
                    GenDraw.DrawFieldEdges(Connections.SelectMany(t => t.Other(this).Room.Cells).ToList(), Color.red);
                }
            }

            /*
            if (Room.CellCount <= 0) return;
            var vec = Room.Cells.First().ToVector3();
            GenDraw.FillableBarRequest r = default;
            r.center = vec + new Vector3(0.25f, 0, 0.5f);
            r.size = new Vector2(1f, 0.5f);
            r.rotation = Rot4.East;
            r.fillPercent = AtmosphericInfo.OutsideContainer.Saturation;
            r.filledMat = TiberiumContent.GreenMaterial;
            r.unfilledMat = TiberiumContent.ClearMaterial;
            r.margin = 0.125f;
            GenDraw.DrawFillableBar(r);

            if (ActualContainer.Value > 0)
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
            */
        }
    }

    /*
    public class FlowRenderer
    {
        private Material cachedMat;
        private Mesh cachedMesh;

        public static float MainAlpha = 0.8f;

        [TweakValue("DrawPollution_Tiling", 0.01f, 20f)]
        public static float Tiling = 0.15f;

        [TweakValue("DrawPollution_FlowSpeed", 0f, 2f)]
        public static float FlowSpeed = 0.38f;

        [TweakValue("DrawPollution_BlendSpeed", 0f, 2f)]
        public static float BlendSpeed = 0.4f;

        [TweakValue("DrawPollution_BlendValue", 0f, 1f)]
        public static float BlendValue = 0.48f;

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
                    cachedMat = new Material(TRContentDatabase.TextureBlend);
                    cachedMat.SetTexture("_MainTex1", TiberiumContent.Nebula1);
                    cachedMat.SetTexture("_MainTex2", TiberiumContent.Nebula2);
                    cachedMat.SetColor("_Color", new ColorInt(0, 255, 97, 255).ToColor);
                }

                cachedMat.SetFloat("_Tiling", Tiling);
                cachedMat.SetFloat("_BlendValue", BlendValue);
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
    }
    */
}
