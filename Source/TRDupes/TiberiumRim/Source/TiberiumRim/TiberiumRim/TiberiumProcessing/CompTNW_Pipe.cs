using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class CompTNW_Pipe : CompTNW
    {
        private Rot4 leakDirection = Rot4.Invalid;
        private bool leaking = false;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref leaking, "leaking");
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
        }

        public override void StructureSetOnAdd(CompTNW tnw, IntVec3 cell)
        {
            if (!(tnw is CompTNW_Pipe))
                base.StructureSetOnAdd(tnw, cell);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetGizmosExtra())
            {
                yield return g;
            }          
        }
    }
}
