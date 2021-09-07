using System;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PollutionConnector
    {
        private Building building;
        private RoomComponent_Pollution[] connections;
        private Rot4[] connDirections;
        private Rot4 lastFlowDirection;
        private Rot4 flowDirection;

        public Building Building => building;

        public bool IsFlowing { get; private set; }

        public Rot4 FlowDirection => flowDirection;

        public PollutionConnector(Building building, RoomComponent_Pollution roomA, RoomComponent_Pollution roomB)
        {
            this.building = building;
            connections = new[] { roomA, roomB };
            connDirections = new[] { RotationFrom(building.Position - roomA.MinVec), RotationFrom(building.Position - roomB.MinVec) };
            //Log.Message("Setting Connection: " + building.Position + " - " + connDirections[0].ToStringWord() + " <=> " + connDirections[1].ToStringWord());
        }

        /*
        public Rot4 RoomDirectionAt(IntVec3 offsetPos)
        {
            var group = offsetPos.GetRoom(Building.Map).Group;
            if (connections[0].Group == group)
            {
                var mat = Building.Graphic.MatSingle;
                var tex = 

                return;
            }
            return;
        }
        */

        public Rot4 RotationFrom(IntVec3 diff)
        {
            var connectsHorizontally = !Building.Rotation.IsHorizontal; //(GenAdj.CardinalDirections[0] + Building.Position).GetFirstBuilding(building.Map) != null;
            if (connectsHorizontally)
            {
                return diff.x > 0 ? Rot4.East : Rot4.West;
            }
            return diff.z > 0 ? Rot4.North : Rot4.South;
        }

        public bool CanPass => PassPercent > 0;
        private bool FullFillage => building.def.Fillage == FillCategory.Full;
        private float Fillage => building.def.fillPercent;

        public float PassPercent
        {
            get
            {
                return building switch
                {
                    Building_Door door => door.Open ? 1 : (FullFillage ? 0 : 1f - Fillage),
                    Building_Vent vent => FlickUtility.WantsToBeOn(vent) ? 1 : 0,
                    Building_Cooler cooler => cooler.IsPoweredOn() ? 1 : 0,
                    { } b => FullFillage ? 0 : 1f - Fillage,
                    _ => 0
                };
            }
        }

        public void TryEqualize()
        {
            IsFlowing = false;
            if (!CanPass) return;
            if (!ShouldEqualize(connections[0].Saturation, connections[1].Saturation)) return;
            var flowAmount = PushAmountToOther(connections[0].Saturation, connections[1].Saturation, (TiberiumPollutionMapInfo.CELL_CAPACITY), PassPercent);
            TryEqualizeBetween(connections[0].UsedContainer, connections[1].UsedContainer, flowAmount);
            IsFlowing = true;
            flowDirection = flowAmount > 0 ? connDirections[1].Opposite : connDirections[0].Opposite;

            if (lastFlowDirection != flowDirection)
            {
                connections[0].Notify_FlowChanged();
                connections[1].Notify_FlowChanged();
            }
            lastFlowDirection = flowDirection;

        }

        public int PushAmountToOther(float saturation, float otherSaturation, int throughPutCap, float factor = 1)
        {
            return Mathf.RoundToInt((throughPutCap * (saturation - otherSaturation)) * factor);
        }

        public bool ShouldEqualize(float saturation, float otherSaturation)
        {
            return Math.Abs((saturation - otherSaturation)) > 0.01f;
        }

        public void TryEqualizeBetween(PollutionContainer containerA, PollutionContainer containerB, int amount)
        {
            containerA.Pollution -= amount;
            containerB.Pollution += amount;
        }

        public RoomComponent_Pollution Other(RoomComponent_Pollution from)
        {
            return from == connections[0] ? connections[1] : connections[0];
        }

        public bool Connects(RoomComponent_Pollution toThis)
        {
            return toThis == connections[0] || toThis ==connections[1];
        }

        public bool IsSameBuilding(PollutionConnector other)
        {
            return Building == other.Building;
        }

        public bool ConnectsSame(PollutionConnector other)
        {
            return other.connections.All(connections.Contains);
        }

        public bool ConnectsOutside()
        {
            return connections[0].UsesOutDoorPollution || connections[1].UsesOutDoorPollution;
        }

        public bool IsOutside()
        {
            return connections[0].UsesOutDoorPollution && connections[1].UsesOutDoorPollution;
        }

        public override string ToString()
        {
            return connections[0].Room.ID + " -[" + Building + "]-> " + connections[1].Room.ID;
        }
    }
}
