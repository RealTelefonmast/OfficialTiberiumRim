using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public class TRWorldObject : WorldObject
    {
        public override void SpawnSetup()
        {
            //
            TRUtils.Tiberium().Notify_RegisterNewObject(this);
            base.SpawnSetup();
        }
    }
}
