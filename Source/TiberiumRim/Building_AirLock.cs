using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Building_AirLock : Building_Door
    {
        public Room RoomOuter => (Position + Rotation.FacingCell).GetRoom(Map);
        public Room RoomInner => (Position - Rotation.FacingCell).GetRoom(Map);

        public bool ConnectsToOutside => RoomInner.UsesOutdoorTemperature || RoomOuter.UsesOutdoorTemperature;

        

        public override void Draw()
        {
            base.Draw();
        }
    }
}
