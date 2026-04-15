using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class Blueprint_BuildPipe : Blueprint_Build
    {
        private NetworkMode networkMode;
        private StoreMode storeMode;

        public void SetModes(NetworkMode nMode, StoreMode sMode)
        {
            networkMode = nMode;
            storeMode = sMode;
        }

        protected override Thing MakeSolidThing()
        {
            return base.MakeSolidThing();
        }
    }
}
