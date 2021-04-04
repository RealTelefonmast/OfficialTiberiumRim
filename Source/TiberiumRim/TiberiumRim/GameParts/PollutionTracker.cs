using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PollutionTracker
    {
        private Map map;
        private RoomGroup roomGroup;
        private int pollutionInt;

        private int markedDirty = 0;
        private int width = 0;
        private int height = 0;
        private float diagonal = 0;

        private readonly HashSet<IntVec3> borderCells = new HashSet<IntVec3>();
        private readonly Dictionary<PollutionTracker, List<PollutionPasser>> ConnectedTrackers = new Dictionary<PollutionTracker, List<PollutionPasser>>();

        public RoomGroup Group => roomGroup;

        private TiberiumPollutionMapInfo PollutionInfo => roomGroup.Map.Tiberium().PollutionInfo;

        public int Pollution
        {
            get => PolluteOutDoors ? PollutionInfo.OutsidePollution : pollutionInt;
            set
            {
                if (PolluteOutDoors)
                {
                    PollutionInfo.OutsidePollution = value;
                    return;
                }
                pollutionInt = value;
                //roomGroup.Rooms[0].Notify_RoomShapeOrContainedBedsChanged();
            }
        }

        public float Saturation => (float)Pollution / (roomGroup.CellCount * 100f);
        public bool PolluteOutDoors => roomGroup.UsesOutdoorTemperature;
        public bool IsDirty => markedDirty > 0;

        public HashSet<IntVec3> BorderCellsWithoutCorners => borderCells ?? RegenerateBorderCells();

        public PollutionTracker(Map map, RoomGroup group, int value)
        {
            this.map = map;
            roomGroup = group;
            pollutionInt = value;
        }

        public void Pollute(int value)
        {
            Pollution += value;
        }

        public int PushAmountToOther(PollutionTracker other, int throughPutCap)
        {
            return Mathf.RoundToInt(throughPutCap * (Saturation - other.Saturation));
        }

        public int ThroughPut(float pressureA, float pressureB, int viscosity = 1, float length = 1, int crossSection = 100)
        {
            return Mathf.RoundToInt((10000f * (pressureA - pressureB)) / (8f * (float)Math.PI * length));
        }

        public bool ShouldPushToOther(PollutionTracker other)
        {
            return (Saturation - other.Saturation) >= 0.01f;
        }

        public void TryPushToOther(PollutionTracker other, int value)
        {
            int actualValue = value;
            if (value > Pollution)
                actualValue = Pollution;
            Pollution -= actualValue;
            other.Pollution += actualValue;
        }

        public void MarkDirty()
        {
            markedDirty++;
        }

        private Room OppositeRoomFrom(IntVec3 cell)
        {
            for (int i = 0; i < 4; i++)
            {
                Room room = (cell + GenAdj.CardinalDirections[i]).GetRoom(map);
                if(room == null || room.Group == Group) continue;
                return room;
            }
            return null;
        }

        private HashSet<IntVec3> RegenerateBorderCells()
        {
            borderCells.Clear();
            foreach (IntVec3 c in Group.Cells)
            {
                for (int i = 0; i < 4; i++)
                {
                    IntVec3 intVec = c + GenAdj.CardinalDirections[i];
                    Region region = (intVec).GetRegion(roomGroup.Map, RegionType.Set_Passable);
                    if ((region == null || region.Room.Group != Group))
                    {
                        borderCells.Add(intVec);
                    }
                }
            }
            return borderCells;
        }

        public void RegenerateData(bool ignoreTracker = false)
        {
            if (!IsDirty) return;
            ConnectedTrackers.Clear();
            RegenerateBorderCells();

            //int minX = roomGroup.Cells.MinBy(t => t.x).x;
            //int maxX = roomGroup.Cells.MaxBy(t => t.x).x;
            //int minZ = roomGroup.Cells.MinBy(t => t.z).z;
            //int maxZ = roomGroup.Cells.MaxBy(t => t.z).z;
            //this.width = maxX - minX;
            //this.height = maxZ - minZ;
            //this.diagonal = Mathf.Sqrt(Mathf.Pow(width, 2) + Mathf.Pow(height, 2));
            foreach (var cell in BorderCellsWithoutCorners)
            {
                if (!cell.InBounds(map)) continue;
                var building = cell.GetFirstBuilding(roomGroup.Map);
                if (building == null) continue;
                if (!(building is Building_Door || building is Building_Vent || building is Building_Cooler)) continue;
                var actualRoom = OppositeRoomFrom(building.Position);
                if (actualRoom == null) continue;
                var tracker = PollutionInfo.TrackerFor(actualRoom);
                if (tracker == null) continue;

                if (!ignoreTracker)
                {
                    tracker.MarkDirty();
                    tracker.RegenerateData(true);
                }

                if (!ConnectedTrackers.ContainsKey(tracker))
                {
                    ConnectedTrackers.Add(tracker, new List<PollutionPasser>());
                }
                ConnectedTrackers[tracker].Add(new PollutionPasser(building));
            }
            markedDirty--;
        }

        public void Equalize()
        {
            foreach (var tracker in ConnectedTrackers)
            {
                foreach (var passer in tracker.Value)
                {
                    if (!passer.CanPass) continue;
                    if (!ShouldPushToOther(tracker.Key)) continue;
                    TryPushToOther(tracker.Key, PushAmountToOther(tracker.Key, 100));
                }
            }
        }

        public void OnGUI()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                IntVec3 first = Group.Cells.First();
                Vector3 v = GenMapUI.LabelDrawPosFor(first);
                GenMapUI.DrawThingLabel(v, Group.ID.ToString() + "[" + Pollution + "]", Color.red);
            }
        }

        public void DrawData()
        {
            if (Group.Cells.Contains(UI.MouseCell()))
            {
                GenDraw.DrawFieldEdges(BorderCellsWithoutCorners.ToList(), Color.cyan);
                GenDraw.DrawFieldEdges(ConnectedTrackers.SelectMany(t => t.Value.Select(t => t.Building.Position)).ToList(), Color.green);
                GenDraw.DrawFieldEdges(ConnectedTrackers.SelectMany(t => t.Key.Group.Cells).Distinct().ToList(), Color.red);
            }

            var vec = roomGroup.Cells.First().ToVector3();
            GenDraw.FillableBarRequest r = default;
            r.center = vec + new Vector3(0f, 0, 0.5f);
            r.size = new Vector2(1f, 0.5f);
            r.rotation = Rot4.East;
            r.fillPercent = Saturation;
            r.filledMat = TiberiumContent.GreenMaterial;
            r.unfilledMat = TiberiumContent.ClearMaterial;
            r.margin = 0.12f;
            GenDraw.DrawFillableBar(r);
        }
    }
}
