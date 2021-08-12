using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_BeamHub : TRBuilding
    {
        private List<Building_BeamHub> connectedHubs = new List<Building_BeamHub>(4) { null, null, null, null };
        private List<ScribeList<Building_BeamHubSegmentPart>> connectedSegments = new List<ScribeList<Building_BeamHubSegmentPart>>(4)
        {
            new ScribeList<Building_BeamHubSegmentPart>(LookMode.Reference),
            new ScribeList<Building_BeamHubSegmentPart>(LookMode.Reference),
            new ScribeList<Building_BeamHubSegmentPart>(LookMode.Reference),
            new ScribeList<Building_BeamHubSegmentPart>(LookMode.Reference)
        };

        private bool[] allowedDirections = new[] {true, true, true, true};

        private Graphic toggleGraphic;
        public Graphic ToggleGraphic => toggleGraphic ??= def.beamHub.toggleGraphic.Graphic;

        public bool IsPowered => GetComp<CompPowerTrader>().PowerOn;

        public bool IsConnectedAndPoweredIn(int direction)
        {
            return allowedDirections[direction] && IsPowered && (connectedHubs[direction]?.IsPowered ?? false);
        }

        public bool AllowsDirection(int direction)
        {
            return allowedDirections[direction];
        }

        private int Opposite(int i)
        {
            switch (i)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 0;
                case 3:
                    return 1;
            }
            return 0;
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref connectedHubs, "connectedHubs", LookMode.Reference);
            Scribe_Collections.Look(ref connectedSegments, "segments");
            DataExposeUtility.BoolArray(ref allowedDirections, 4, "allowedDirections");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (respawningAfterLoad) return;
            TryFindHubs();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (var i = 0; i < connectedHubs.Count; i++)
            {
                var hub = connectedHubs[i];
                if (hub == null) continue;
                LoseConnectionWith(hub, i);
                hub.LoseConnectionWith(this, Opposite(i));
                for (var k = connectedSegments[i].Count - 1; k >= 0; k--)
                {
                    var segmentPart = connectedSegments[i][k];
                    RemoveSegment(segmentPart, i);
                }
            }
            base.Destroy(mode);
        }

        public bool HasConnectionInDirection(Rot4 direction)
        {
            return connectedHubs[direction.AsInt] != null;
        }

        private void TryFindHubs()
        {
            //Checking all cardinal directions
            for (int i = 0; i < 4; i++)
            {
                LookForHub(i);
            }
        }

        private void LookForHub(int direction)
        {
            //Looking for hub in a specified direction
            List<IntVec3> cells = new List<IntVec3>();
            for (int i = 0; i <= def.beamHub.range; i++)
            {
                var curCell = Position + new IntVec3(0, 0, i).RotatedBy(new Rot4(direction));
                if (!curCell.InBounds(Map)) break;
                var thing = curCell.GetFirstBuilding(Map);
                if (thing != null && thing != this && !(thing is Building_BeamHub || thing is Building_BeamHubSegmentPart) ) break;
                cells.Add(curCell);
                if (thing is Building_BeamHub hub && hub != this)
                {
                    //Hub found, making connection
                    ConnectWith(hub, direction);
                    hub.ConnectWith(this, Opposite(direction));

                    MakeSegments(hub, cells, direction);
                    break;
                }
            }
        }

        public void ConnectWith(Building_BeamHub other, int direction)
        {
            if (connectedHubs[direction] != null)
            {
                //Other hub exists in same direction
                connectedSegments[direction].Clear();
            }
            connectedHubs[direction] = other;
            switch (direction)
            {
                case 0:
                    allowedDirections[0] = other.allowedDirections[2];
                    break;
                case 1:
                    allowedDirections[1] = other.allowedDirections[3];
                    break;
                case 2:
                    allowedDirections[2] = other.allowedDirections[0];
                    break;
                case 3:
                    allowedDirections[3] = other.allowedDirections[1];
                    break;
            }
        }

        public void LoseConnectionWith(Building_BeamHub other, int direction)
        {
            connectedHubs[direction] = null;
        }

        private void MakeSegments(Building_BeamHub toOther, List<IntVec3> cells, int direction)
        {
            foreach (var cell in cells)
            {
                //Making segments for new connection
                Building_BeamHubSegmentPart segmentPart;
                if (cell.GetFirstThing(Map, this.def.beamHub.segmentDef) is Building_BeamHubSegmentPart part)
                {
                    //Existing segment found, update new parents
                    segmentPart = part;
                }
                else
                {
                    //Make new segment, add connection
                    segmentPart = (Building_BeamHubSegmentPart)GenSpawn.Spawn(this.def.beamHub.segmentDef, cell, Map);
                }
                segmentPart.RegisterConnection(this, toOther, Opposite(direction));

                AddSegment(segmentPart, direction);
                toOther.AddSegment(segmentPart, Opposite(direction));
            }
        }

        public void AddSegment(Building_BeamHubSegmentPart segment, int directionTo)
        {
            //segment.RegisterHub(this, Opposite(directionTo));
            connectedSegments[directionTo].Add(segment);
        }

        public void RemoveSegment(Building_BeamHubSegmentPart segment, int directionTo)
        {
            segment?.DeregisterConnection(Opposite(directionTo));
            connectedSegments[directionTo].Remove(segment);
        }

        public void UpdateSegmentToExistingConnections(Building_BeamHubSegmentPart segment)
        {
            Log.Message("Updating segment " + segment);
            for (var i = 0; i < connectedHubs.Count; i++)
            {
                var hub = connectedHubs[i];
                if(hub==null)continue;
                segment.RegisterConnection(this, hub, i);
            }
        }

        private void CheckSegmentStatusChange(int direction)
        {
            connectedSegments[direction].ForEach(b => b.CheckBeamStatus());
        }

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if (signal == "PowerTurnedOn")
            {
                for (int i = 0; i < 4; i++)
                {
                    CheckSegmentStatusChange(i);
                    //TryToggle(i, false);
                }
            }
            else if (signal == "PowerTurnedOff")
            {
                for (int i = 0; i < 4; i++)
                {
                    CheckSegmentStatusChange(i);
                    //TryToggle(i, false);
                }
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.AppendLine("\nAllowed: " + (allowedDirections[0]) + ", " + (allowedDirections[1]) + ", " + (allowedDirections[2]) + ", " + (allowedDirections[3]) + "\n");
            sb.AppendLine("Hubs: " + (connectedHubs[0] != null) + ", " + (connectedHubs[1] != null) + ", " + (connectedHubs[2] != null) + ", " + (connectedHubs[3] != null) + "\n");
            sb.AppendLine("Segments: \n" + connectedSegments[0].Count + "\n"
                          + connectedSegments[1].Count + "\n"
                          + connectedSegments[2].Count + "\n"
                          + connectedSegments[3].Count + "\n");
            return sb.ToString().TrimStart().TrimEndNewlines();
        }

        public override void Draw()
        {
            base.Draw();
            if (!Find.Selector.IsSelected(this)) return;
            foreach (var hub in connectedHubs)
            {
                if (hub == null) continue;
                GenDraw.DrawLineBetween(DrawPos, hub.DrawPos);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            yield return new Command_Action
            {
                defaultLabel = "Toggle North",
                icon = (Texture2D)ToggleGraphic.MatNorth.mainTexture,
                action = delegate
                {
                    allowedDirections[0] = !allowedDirections[0];
                    if (connectedHubs[0] != null)
                        connectedHubs[0].allowedDirections[2] = allowedDirections[0];
                    Log.Message("Toggling North to: " + allowedDirections[0]);
                    CheckSegmentStatusChange(0);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Toggle East",
                icon = (Texture2D)ToggleGraphic.MatEast.mainTexture,
                action = delegate
                {
                    allowedDirections[1] = !allowedDirections[1];
                    if (connectedHubs[1] != null)
                        connectedHubs[1].allowedDirections[3] = allowedDirections[1];
                    Log.Message("Toggling East to: " + allowedDirections[1]);
                    CheckSegmentStatusChange(1);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Toggle South",
                icon = (Texture2D)ToggleGraphic.MatSouth.mainTexture,
                action = delegate
                {
                    allowedDirections[2] = !allowedDirections[2];
                    if (connectedHubs[2] != null)
                        connectedHubs[2].allowedDirections[0] = allowedDirections[2];
                    Log.Message("Toggling South to: " + allowedDirections[2]);
                    CheckSegmentStatusChange(2);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Toggle West",
                icon = (Texture2D)ToggleGraphic.MatWest.mainTexture,
                action = delegate
                {
                    allowedDirections[3] = !allowedDirections[3];
                    if (connectedHubs[3] != null)
                        connectedHubs[3].allowedDirections[1] = allowedDirections[3];
                    Log.Message("Toggling West to: " + allowedDirections[3]);
                    CheckSegmentStatusChange(3);
                    //TryToggle(3, !allowedDirections[3]);
                }
            };
        }
    }
}
