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
    public class Building_AirLock : Building_Door
    {
        private Rot4? actualInt;
        private Room[] roomArr;
        private RoomComponent_AirLock[] airLockArr;

        //Main Data
        public Rot4 ActualRotation => actualInt ??= DoorRotationAt(Position, Map);
        public bool ConnectsToOutside => Rooms[0].UsesOutdoorTemperature || Rooms[1].UsesOutdoorTemperature;

        private Room RoomOuter => (Position + ActualRotation.FacingCell).GetRoom(Map);
        private Room RoomInner => (Position + ActualRotation.Opposite.FacingCell).GetRoom(Map);

        public Room[] Rooms => roomArr;
        public RoomComponent_AirLock[] RoomComps => airLockArr;


        //Main Conditions
        public bool IsFunctioning => airLockArr[0] != null && airLockArr[1] != null;
        public bool ConnectsToPollutedRoom => IsFunctioning && !(RoomComps[0].IsClean && RoomComps[1].IsClean);

        public bool IsLockedDown { get; private set; }

        //
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            roomArr = new Room[2];
            airLockArr = new RoomComponent_AirLock[2];
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public Room OppositeRoom(Room room)
        {
            return room == RoomInner ? RoomOuter : RoomInner;
        }

        public RoomComponent_AirLock OppositeRoomComp(RoomComponent_AirLock roomComp)
        {
            return roomComp == RoomComps[0] ? RoomComps[1] : RoomComps[0];
        }

        public RoomComponent_AirLock OppositeRoomComp(int i)
        {
            return i == 0 ? RoomComps[1] : RoomComps[0];
        }

        public bool OtherIsClean(RoomComponent_AirLock ofRoom)
        {
            return OppositeRoomComp(ofRoom).IsClean;
        }

        private void Cleanup()
        {
            for (int i = 0; i < 2; i++)
            {
                if (RoomComps[i]?.Disbanded ?? false)
                    SetAirlock(i, null);
            }
        }

        public void SetAirlock(RoomComponent_AirLock airlock)
        {
            //
            Cleanup();

            //
            if (airlock == RoomComps[0] || airlock == RoomComps[1]) return;
            if (RoomComps[0] == null && (airlock.IsAirLock || RoomComps[1] != null))
            {
                SetAirlock(0, airlock);
                return;
            }
            if(RoomComps[1] == null)
            {
                SetAirlock(1, airlock);
            }
        }

        private void SetAirlock(int index, RoomComponent_AirLock airlock)
        {
            Rooms[index] = airlock?.Room;
            RoomComps[index] = airlock;
        }

        public void CheckLockDown(bool lockedDown)
        {
            if (lockedDown && ConnectsToPollutedRoom)
            {
                if (this.IsForbidden(Faction.OfPlayerSilentFail)) return;
                this.SetForbidden(true, false);
                IsLockedDown = true;
            }
            else
            {
                if (!this.IsForbidden(Faction.OfPlayerSilentFail)) return;
                this.SetForbidden(false, false);
                IsLockedDown = false;
            }
        }

        private bool RoomInLockDown(int index, Pawn forPawn)
        {
            if (RoomComps[index].LockedDown)
            {
                return !RoomComps[index].Parent.ContainsPawn(forPawn);
            }
            if (!RoomComps[index].IsClean)
            {
                return !OppositeRoomComp(index).CanVent;
            }
            return false;
        }

        public bool CanOpenOverride(Pawn p)
        {
            //If not working fully, not usable
            if (!IsFunctioning) return false;
            //If inbetween two airlocks, always openable
            if (RoomComps[0].IsAirLock && RoomComps[1].IsAirLock) return true;
            //Special conditions on
            if (RoomInLockDown(0, p)) return false;
            if (RoomInLockDown(1, p)) return false;
            return true;
        }

        public override bool BlocksPawn(Pawn p)
        {
            return base.BlocksPawn(p);
        }

        public override bool PawnCanOpen(Pawn p)
        {
            return base.PawnCanOpen(p);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.AppendLine();
            if (IsFunctioning)
            {
                if (RoomComps[0].LockedDown)
                {
                    sb.AppendLine($"[WARNING LOCKDOWN][0][{Rooms[0].ID}]".Colorize(Color.red));
                }

                if (RoomComps[1].LockedDown)
                {
                    sb.AppendLine($"[WARNING LOCKDOWN][1][{Rooms[1].ID}]".Colorize(Color.red));
                }

                if (DebugSettings.godMode)
                {
                    sb.AppendLine($"IsFunctioning: {IsFunctioning}");
                    sb.AppendLine($"ConnectsToPollutedRoom: {ConnectsToPollutedRoom}");
                    sb.AppendLine(
                        $"{$"Room[{Rooms[0].ID}]".Colorize(Color.cyan)} LockedDown: {RoomComps[0]?.LockedDown} CanVent: {RoomComps[0]?.CanVent} Ticking: {RoomComps[0]?.tickSinceLastFleck}");
                    sb.AppendLine(
                        $"{$"Room[{Rooms[1].ID}]".Colorize(Color.magenta)} LockedDown: {RoomComps[1]?.LockedDown} CanVent: {RoomComps[1]?.CanVent} Ticking: {RoomComps[1]?.tickSinceLastFleck}");
                }
            }

            return sb.ToString().TrimEndNewlines();
        }

        private void DrawLockDown(int index)
        {
            if (IsLockedDown && !RoomComps[index].IsClean)
            {
                Room.fields.Clear();
                Room.fields.AddRange(Rooms[index].Cells);
                Color color = Color.red;
                color.a = Pulser.PulseBrightness(1f, 0.6f);
                GenDraw.DrawFieldEdges(Room.fields, color, null);
                Room.fields.Clear();
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (!IsFunctioning) return;
            if (Find.Selector.IsSelected(this))
            {
                DrawLockDown(0);
                DrawLockDown(1);

                if (!DebugSettings.godMode) return;
                if (RoomComps[0] != null)
                {
                    GenDraw.DrawFieldEdges(RoomComps[0].Room.Cells.ToList(), Color.cyan);
                }
                if (RoomComps[1] != null)
                {
                    GenDraw.DrawFieldEdges(RoomComps[1].Room.Cells.ToList(), Color.magenta);
                }
            }
        }
    }
}
