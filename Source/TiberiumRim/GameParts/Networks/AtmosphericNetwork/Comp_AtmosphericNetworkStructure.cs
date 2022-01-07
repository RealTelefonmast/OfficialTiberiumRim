using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class Comp_AtmosphericNetworkStructure : Comp_NetworkStructure
    {
        private RoomComponent_Atmospheric atmosphericInt;

        public NetworkComponent AtmosphericComp => this[TiberiumDefOf.AtmosphericNetwork];

        public RoomComponent_Atmospheric Atmospheric
        {
            get
            {
                if (atmosphericInt == null || atmosphericInt.Parent.IsDisbanded)
                {
                    atmosphericInt = parent.GetRoom().AtmosphericRoomComp();
                }
                return atmosphericInt;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
        }
    }

    public class CompProperties_ANS : CompProperties_NetworkStructure
    {
        public CompProperties_ANS()
        {
            this.compClass = typeof(Comp_AtmosphericNetworkStructure);
        }
    }
}
