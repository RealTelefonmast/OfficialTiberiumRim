using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class IonCannonCore : ThingWithComps
    {
        private List<PowerBeam> currentBeams = new List<PowerBeam>();
        private float radius = 25f;
        private int maxBeams = 8;

        private bool Finalized = false;

        // Stage One


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            base.Tick();
        }

    }
}
