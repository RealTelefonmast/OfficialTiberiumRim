using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class RoomComponent_AirLock : RoomComponent
    {
        public bool IsActive => this.Room.Districts.All(r => r.Room.Role == TiberiumDefOf.TR_AirLock);

        private List<Thing> AirVents = new List<Thing>();

        public override void Create(RoomTracker parent)
        {
            base.Create(parent);
            
        }
    }
}