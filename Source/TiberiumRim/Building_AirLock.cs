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


        public Rot4 ActualRotation => actualInt ??= DoorRotationAt(Position, Map);
        public bool ConnectsToOutside => Rooms[0].UsesOutdoorTemperature || Rooms[1].UsesOutdoorTemperature;

        private Room RoomOuter => (Position + ActualRotation.FacingCell).GetRoom(Map);
        private Room RoomInner => (Position + ActualRotation.Opposite.FacingCell).GetRoom(Map);

        public Room[] Rooms => roomArr;
        public RoomComponent_AirLock[] RoomComps => airLockArr;

        public bool IsFunctioning => airLockArr[0] != null && airLockArr[1] != null;

        private bool IsClean
        {
            get
            {
                bool flag1 = !RoomComps[0].IsActive || RoomComps[0].IsClean;
                bool flag2 = !RoomComps[0].IsActive || RoomComps[0].IsClean;
                return flag1 && flag2;
            }
        }
        /*
        public bool IsReady
        {
            get
            {
                bool flag1 = RoomComps[0].IsActive ? RoomComps[0].IsClean : ;
                bool flag2 = !RoomComps[0].IsActive || RoomComps[0].IsClean;
                return flag1 && flag2;
            }
        }

        public bool IsReady => ((AirlockOne?.IsClean ?? true) || AirlockOne.AllDoorsClosed) ||
                               ((AirlockTwo?.IsClean ?? true) || AirlockTwo.AllDoorsClosed);
        */
        private bool CanBeUsed => IsFunctioning && IsClean;

        //Deciding 
        private bool NeedsToClose => !(RoomComps[0].IsClean && RoomComps[1].IsClean);

        //Deciding
        public bool CannotOpen
        {
            get
            {
                bool oneClean = RoomComps[0].IsClean;
                bool twoClean = RoomComps[1].IsClean;
                if (oneClean && twoClean) return false;

                if (!oneClean)
                {
                    return !RoomComps[1].AllDoorsClosed;
                }
                if (!twoClean)
                {
                    return !RoomComps[0].AllDoorsClosed;
                }

                return false;
            }
        }

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

        /*
        public RoomComponent_AirLock OppositeOf(Room room)
        {
            return room == RoomOne ? AirlockTwo : AirlockOne;
        }
        */
        public void SetAirlock(RoomComponent_AirLock airlock)
        {
            TLog.Debug($"Setting airlock for {this}: {airlock.Room?.ID}...");
            if (airlock.Room == RoomInner)
            {
                Rooms[0] = airlock.Room;
                RoomComps[0] = airlock;
            }
            if (airlock.Room == RoomOuter)
            {
                Rooms[1] = airlock.Room;
                RoomComps[1] = airlock;
            }
        }

        //Might be redundant
        public void RemoveAirlock(Room forRoom)
        {
            if (RoomComps[0]?.Room.ID == forRoom.ID)
            {
                RoomComps[0] = null;
                Rooms[0] = null;
            }
        }

        public override bool PawnCanOpen(Pawn p)
        {
            return base.PawnCanOpen(p) && CanBeUsed;
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());
            sb.AppendLine();
            sb.AppendLine($"Can Be Used: {CanBeUsed}");
            sb.AppendLine($"Can Not Open: {CannotOpen}");
            sb.AppendLine($"Is Clean: {IsClean}");
            //sb.AppendLine($"Is Ready: {IsReady}");
            sb.AppendLine($"Room1[{ Rooms[0].ID}][{Rooms[0].UsesOutdoorTemperature}] IsReady:{RoomComps[0].IsReadyForUsage} Clean:{RoomComps[0].IsClean}");
            sb.AppendLine($"Room2[{ Rooms[1].ID}][{Rooms[1].UsesOutdoorTemperature}] IsReady:{RoomComps[1].IsReadyForUsage} Clean:{RoomComps[1].IsClean}");
            return sb.ToString().TrimEndNewlines();
        }

        public override void Draw()
        {
            base.Draw();
            if (Find.Selector.IsSelected(this))
            {
                if (RoomComps[0] != null)
                {
                    GenDraw.DrawFieldEdges(RoomComps[0].Room.Cells.ToList(), Color.magenta);
                }
                if (RoomComps[1] != null)
                {
                    GenDraw.DrawFieldEdges(RoomComps[1].Room.Cells.ToList(), Color.cyan);
                }
            }
        }
    }
}
