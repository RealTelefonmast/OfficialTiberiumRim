namespace TiberiumRim
{
    /*
    public class PollutionTracker
    {
        private static int CELL_CAPACITY = 100;

        private Map map;
        private RoomGroup roomGroup;
        public FlowRenderer renderer;
        private int pollutionInt;
        private int totalCapacity;

        private Vector3 actualCenter;

        private int markedDirty = 0;

        private bool hasDoor = false;
        private int width = 0;
        private int height = 0;
        //private float diagonal = 0;

        private readonly HashSet<IntVec3> borderCells = new HashSet<IntVec3>();
        private readonly HashSet<IntVec3> thinRoofCells = new HashSet<IntVec3>();
        private readonly Dictionary<PollutionTracker, List<PollutionConnector>> ConnectedTrackers = new Dictionary<PollutionTracker, List<PollutionConnector>>();
        public readonly HashSet<PollutionConnector> AllPassers = new HashSet<PollutionConnector>();
        
        private PollutionTracker[] ConnectingRooms;

        public RoomGroup Group => roomGroup;

        private TiberiumPollutionMapInfo PollutionInfo => Group.Map.Tiberium().PollutionInfo;
        private OutsidePollutionData Outside => PollutionInfo.OutsideData;

        public int Pollution
        {
            get => UsesOutDoorPollution ? Outside.Pollution : ActualPollution;
            set
            {
                if (FullySaturated) return;
                if (UsesOutDoorPollution)
                {
                    Outside.Pollution = value;
                    return;
                }
                ActualPollution = value;
            }
        }

        public int ActualPollution
        {
            get => pollutionInt;
            set => pollutionInt = value;
        }

        public int OpenRoofCount => Group.OpenRoofCount;
        public int Capacity => totalCapacity;

        public float ActualSaturation => (float)ActualPollution / Capacity;
        public float Saturation => IsDoorWay ? MixSaturation : (UsesOutDoorPollution ? Outside.Saturation : ActualSaturation);
        private float MixSaturation => ((ConnectingRooms[0]?.Saturation ?? 0) + (ConnectingRooms[1]?.Saturation ?? 0) / 2);

        public bool FullySaturated => UsesOutDoorPollution ? Outside.FullySaturated : Saturation >= CriticalPressure;
        public bool UsesOutDoorPollution => roomGroup.UsesOutdoorTemperature;
        public bool IsDoorWay => hasDoor;
        public bool IsDirty => markedDirty > 0;

        private float CriticalPressure => 1.5f;

        public Vector3 ActualCenter => actualCenter;
        public Vector3 Size => new Vector3(width, 0, height);

        public HashSet<IntVec3> BorderCellsWithoutCorners => borderCells;

        public PollutionTracker(Map map, RoomGroup group, int value)
        {
            this.map = map;
            roomGroup = group;
            pollutionInt = value;
            renderer = new FlowRenderer(this);
        }

        public bool TryPollute(int value)
        {
            if (FullySaturated) return false;
            Pollution += value;
            return true;
        }

        public void Equalize()
        {
            //Check Pressure
            //if (Saturation > CriticalPressure)

            //Equalize
            if (OpenRoofCount > 0 && ActualPollution > 0)
            {
                if (!Outside.FullySaturated)
                {
                    if (ShouldPushToOther(ActualSaturation, Outside.Saturation))
                    {

                        int from = ActualPollution, to = Outside.Pollution;
                        TryPushToOther(ref from, ref to, PushAmountToOther(ActualSaturation, Outside.Saturation, CELL_CAPACITY * OpenRoofCount));
                        ActualPollution = from;
                        Outside.Pollution = to;
                        return;
                    }
                }
            }
            
            foreach (var tracker in ConnectedTrackers)
            {
                if (tracker.Key.FullySaturated) continue;
                foreach (var passer in tracker.Value)
                {
                    if (!passer.CanPass) continue;
                    if (!ShouldPushToOther(Saturation, tracker.Key.Saturation)) continue;
                    int from = Pollution, 
                        to = tracker.Key.Pollution;
                    TryPushToOther(ref from, ref to, PushAmountToOther(Saturation, tracker.Key.Saturation, CELL_CAPACITY, 1f - passer.Building.props.fillPercent));
                    Pollution = from;
                    tracker.Key.Pollution = to;
                }
            }
        }

        private void TryOverpressure()
        {
            //Prefer Thin Roof
            if (thinRoofCells.Any())
            {
                map.roofGrid.SetRoof(thinRoofCells.RandomElement(), null);
                return;
            }
            //Choose Passers
            var passers = ConnectedTrackers.Values.SelectMany(t => t).ToList();
            if (passers.Any())
            {
                var randomPasser = passers.RandomElement();
                IntVec3 pos = randomPasser.Building.Position;
                randomPasser.Building.Destroy();

            }
            //Resolve to Walls?

            //Keep increasing pressure..
        }

        private void DoPressureEvacuation(IntVec3 puller, PollutionTracker otherRoom)
        {
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
            return (saturation - otherSaturation) >= 0.01f;
        }

        public void TryPushToOther(ref int from, ref int to, int value)
        {
            int actualValue = value;
            if (value > from)
                actualValue = from;
            from -= actualValue;
            to += actualValue;
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

        private void RegenerateCellInformation()
        {
            borderCells.Clear();
            foreach (IntVec3 c in Group.Cells)
            {
                if (!map.roofGrid.RoofAt(c)?.isThickRoof ?? false)
                    thinRoofCells.Add(c);

                for (int i = 0; i < 4; i++)
                {
                    IntVec3 cardinal = c + GenAdj.CardinalDirections[i];
                    //IntVec3 diagonal = c + GenAdj.DiagonalDirections[i];

                    //bool isBorder = !Group.Regions.Contains(cardinal.GetRegion(map, RegionType.Set_All));
                    Region region = (cardinal).GetRegion(map);
                    if (region == null || region.Room != Group.Rooms[0])
                    {
                        borderCells.Add(cardinal);
                    }
                }
            }
        }

        public void SetNewPasser(PollutionTracker toTracker, Building passerBuilding)
        {
            if (!ConnectedTrackers.ContainsKey(toTracker))
            {
                ConnectedTrackers.Add(toTracker, new List<PollutionConnector>());
            }
            var newPasser = new PollutionConnector(passerBuilding, this, toTracker);
            AllPassers.Add(newPasser);
            if (passerBuilding is Building_Door)
                ConnectedDoors.Add(newPasser);
            ConnectedTrackers[toTracker].Add(newPasser);
        }

        public void RegenerateData(bool ignoreTracker = false)
        {
            if (!IsDirty) return;
            //Generic Data For All
            ConnectedTrackers.Clear();
            AllPassers.Clear();
         
            totalCapacity = Group.CellCount * CELL_CAPACITY;
            hasDoor = Group.Rooms[0].IsDoorway;

            renderer.CachedTiling = Group.CellCount / 100f;

            if (hasDoor)
            {
                ConnectingRooms = new PollutionTracker[2];
                int k = 0;
                for (int i = 0; i < 4; i++)
                {
                    Room room = (Group.Cells.First() + GenAdj.CardinalDirections[i]).GetRoom(map);
                    if (room == null || room.Group == Group) continue;
                    ConnectingRooms[k] = PollutionInfo.PollutionFor(room);
                    if (k >= 1) break;
                    k++;
                }
                ConnectingRooms[0].SetNewPasser(ConnectingRooms[1], this.Group.Rooms[0].Regions[0].door);
                if(ConnectingRooms[1] != ConnectingRooms[0])
                    ConnectingRooms[1].SetNewPasser(ConnectingRooms[0], this.Group.Rooms[0].Regions[0].door);
                Log.Message("Made Door Connector For |" + ConnectingRooms[0]?.Group.ID + " - " + ConnectingRooms[1]?.Group.ID + "|");
            }

            if (UsesOutDoorPollution)
            {
                Log.Message("Updating outdoors - existing passers: " + Outside.);
                //Update Passers
                foreach (var passer in ConnectedDoors)
                {
                    PollutionInfo.PollutionFor(passer.Building.GetRoom()).RegenerateData(true);
                }
                markedDirty--;
                return;
            }

            //Special Data for INDOOR rooms only!
            RegenerateCellInformation();
            foreach (var cell in BorderCellsWithoutCorners)
            {
                if (!cell.InBounds(map)) continue;
                var building = cell.GetFirstBuilding(roomGroup.Map);
                if (building == null) continue;
                if (!(building is Building_Door || building is Building_Vent || building is Building_Cooler || building.props.Fillage != FillCategory.Full)) continue;
                var actualRoom = OppositeRoomFrom(building.Position);
                if (actualRoom == null) continue;
                var tracker = PollutionInfo.PollutionFor(actualRoom);
                if (tracker == null) continue;

                SetNewPasser(tracker, building);

                if (!ignoreTracker)
                {
                    if (tracker.UsesOutDoorPollution)
                    {
                        tracker.SetNewPasser(this, building);
                    }
                    else
                    {
                        tracker.MarkDirty();
                        tracker.RegenerateData(true);
                    }
                    if (building is Building_Door door)
                    {
                        var tracker2 = PollutionInfo.PollutionFor(door.GetRoomGroup());
                        tracker2.MarkDirty();
                        tracker2.RegenerateData(true);
                    }
                }
            }
            markedDirty--;
        }

        public void OnGUI()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                IntVec3 first = Group.Cells.First();
                Vector3 v = (GenMapUI.LabelDrawPosFor(first)) + new Vector2(0,-0.75f);
                GenMapUI.DrawThingLabel(v, Group.ID + "[" + Pollution + "][" + ActualPollution + "]" + (IsDoorWay ? "[Door]" : ""), Color.red);
            }
        }

        public void DrawData()
        {
            if (!UsesOutDoorPollution)
                renderer.Draw();
            
            if (!TRUtils.Tiberium().GameSettings.RadiationOverlay) return;
            if (Group.Cells.Contains(UI.MouseCell()))
            {
                GenDraw.DrawFieldEdges(BorderCellsWithoutCorners.ToList(), Color.cyan);
                GenDraw.DrawFieldEdges(ConnectedTrackers.SelectMany(t => t.Value.Select(t => t.Building.Position)).ToList(), Color.green);
                GenDraw.DrawFieldEdges(ConnectedTrackers.SelectMany(t => t.Key.Group.Cells).Distinct().ToList(), Color.red);
                if(IsDoorWay)
                    GenDraw.DrawFieldEdges(ConnectingRooms.SelectMany(t => t.Group.Cells).ToList(), Color.red);
            }

            var vec = roomGroup.Cells.First().ToVector3();
            GenDraw.FillableBarRequest r = default;
            r.center = vec + new Vector3(0f, 0, 0.5f);
            r.size = new Vector2(1f, 0.5f);
            r.rotation = Rot4.East;
            r.fillPercent = Saturation;
            r.filledMat = TiberiumContent.GreenMaterial;
            r.unfilledMat = TiberiumContent.ClearMaterial;
            r.margin = 0.125f;
            GenDraw.DrawFillableBar(r);

            if (ActualPollution > 0)
            {
                GenDraw.FillableBarRequest r2 = default;
                r2.center = vec + new Vector3(0.5f, 0, 0.5f);
                r2.size = new Vector2(1f, 0.5f);
                r2.rotation = Rot4.East;
                r2.fillPercent = ActualSaturation;
                r2.filledMat = TiberiumContent.RedMaterial;
                r2.unfilledMat = TiberiumContent.ClearMaterial;
                r2.margin = 0.125f;
                GenDraw.DrawFillableBar(r2);
            }
        }
    }
    */
}
