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
        private RoomComponent_Pollution pollutionInt;

        public NetworkComponent AtmosphericComp => this[TiberiumDefOf.AtmosphericNetwork];

        public RoomComponent_Pollution Pollution
        {
            get
            {
                if (pollutionInt == null || pollutionInt.Parent.IsDisbanded)
                {
                    pollutionInt = parent.GetRoom().Pollution();
                }
                return pollutionInt;
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

        }
    }
}
