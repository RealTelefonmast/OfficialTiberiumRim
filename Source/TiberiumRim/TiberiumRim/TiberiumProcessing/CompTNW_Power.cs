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

        public override bool[] DrawBools => new bool[] { true, StructureSet.Pipes.Any(), power.GeneratesPowerNow, power.GeneratesPowerNow };
        public override bool ShouldDoEffecters => power.GeneratesPowerNow;
    }
}
