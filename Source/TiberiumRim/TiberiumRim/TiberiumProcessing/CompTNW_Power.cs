using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TiberiumRim
{
    public class CompTNW_Power : CompTNW
    {
        public CompPower_Tiberium power;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            power = parent.GetComp<CompPower_Tiberium>();
        }

        public override bool ShouldDoEffecters => power?.GeneratesPowerNow ?? false;
    }
}
