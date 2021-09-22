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
        public bool ConnectsToOutside => (RoomInner?.UsesOutdoorTemperature ?? false) || (RoomOuter?.UsesOutdoorTemperature ?? false);

        public Room RoomOuter => (Position + ActualRotation.FacingCell).GetRoom(Map);
        public Room RoomInner => (Position + ActualRotation.FacingCell).GetRoom(Map);

        public RoomComponent_AirLock AirLock { get; set; }
    
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public void SetAirlock(RoomComponent_AirLock airlock)
        {
            AirLock = airlock;
        }

        public override bool PawnCanOpen(Pawn p)
        {
            return base.PawnCanOpen(p) && AirLock.IsClean;
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}
