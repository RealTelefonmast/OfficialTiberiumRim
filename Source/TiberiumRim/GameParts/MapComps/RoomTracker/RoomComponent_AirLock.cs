using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiberiumRim.GameParts.MapComps.RoomTracker
{
    public class RoomComponent_AirLock : RoomComponent
    {
        public bool IsActive => this.Room.Districts.All(r => r.Room.Role == TiberiumDefOf.TR_AirLock);
    }
}
