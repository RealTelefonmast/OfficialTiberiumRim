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
     
        public Rot4 ActualRotation => actualInt ??= DoorRotationAt(Position, Map);
        public bool ConnectsToOutside => (RoomOne?.UsesOutdoorTemperature ?? false) || (RoomTwo?.UsesOutdoorTemperature ?? false);

        private Room RoomOuter => (Position + ActualRotation.FacingCell).GetRoom(Map);
        private Room RoomInner => (Position + ActualRotation.Opposite.FacingCell).GetRoom(Map);

        public Room RoomOne { get; set; }
        public Room RoomTwo { get; set; }
        public RoomComponent_AirLock AirlockOne { get; set; }
        public RoomComponent_AirLock AirlockTwo { get; set; }

        public bool IsClean => (AirlockOne?.IsClean ?? true) || (AirlockTwo?.IsClean ?? true);
        public bool IsReady => (AirlockOne?.AllDoorsClosed ?? true) || (AirlockTwo?.AllDoorsClosed ?? true);

        public bool CanBeUsed => IsClean && IsReady;
        public bool NeedsToClose => !(AirlockOne.IsClean && AirlockTwo.IsClean);

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public Room OppositeRoom(Room room)
        {
            return room == RoomOne ? RoomTwo : RoomOne;
        }

        public RoomComponent_AirLock OppositeOf(Room room)
        {
            return room == RoomOne ? AirlockTwo : AirlockOne;
        }

        public void SetAirlock(RoomComponent_AirLock airlock)
        {
            if (airlock.Room == RoomOuter)
            {
                RoomOne = airlock.Room;
                AirlockOne = airlock;
            }

            if (airlock.Room == RoomInner)
            {
                RoomTwo = airlock.Room;
                AirlockTwo = airlock;
            }
        }

        //Might be redundant
        public void RemoveAirlock(Room forRoom)
        {
            if (AirlockOne?.Room.ID == forRoom.ID)
            {
                AirlockOne = null;
                RoomOne = null;
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
            sb.AppendLine($"Room1[{RoomOne.ID}][{RoomOne.UsesOutdoorTemperature}] Clean:{AirlockOne.IsClean}");
            sb.AppendLine($"Room2[{RoomTwo.ID}][{RoomTwo.UsesOutdoorTemperature}] Clean:{AirlockTwo.IsClean}");
            return sb.ToString().TrimEndNewlines();
        }

        public override void Draw()
        {
            base.Draw();
            if (Find.Selector.IsSelected(this))
            {
                if (AirlockOne != null)
                {
                    GenDraw.DrawFieldEdges(AirlockOne.Room.Cells.ToList(), Color.magenta);
                }
                if (AirlockTwo != null)
                {
                    GenDraw.DrawFieldEdges(AirlockTwo.Room.Cells.ToList(), Color.cyan);
                }
            }
        }
    }
}
