using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class PlaceWorker_ResearchCrane : PlaceWorker
    {
        private static List<ThingDef> TargetDefs;

        public PlaceWorker_ResearchCrane()
        {
            if (!TargetDefs.NullOrEmpty()) return;
            TargetDefs = new List<ThingDef>();
            foreach (var thing in DefDatabase<TRThingDef>.AllDefs)
            {
                if (thing.thingClass.GetInterface("IResearchCraneTarget") != null)
                {
                    TargetDefs.Add(thing);
                }
            }
        }

        private IEnumerable<Thing> ResearchTargets => TargetDefs.SelectMany(t => Find.CurrentMap.listerThings.ThingsOfDef(t));

        private Thing TargetAt(IntVec3 pos)
        {
            foreach (var targetDef in TargetDefs)
            {
                var first = pos.GetFirstThing(Find.CurrentMap, targetDef);
                if (first != null)
                    return first;
            }
            return null;
        }

        private CellRect CellRectAt(IntVec3 pos)
        {
            return new CellRect(pos.x - 1, pos.z - 1, 3, 3);
        }

        private bool Overlaps(Thing thing, CellRect rect)
        {
            if (thing == null) return false;
            return !thing.OccupiedRect().Except(rect).Any();
        }

        private static IntVec3 pos;

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            pos = loc;
            return Overlaps(TargetAt(loc), CellRectAt(loc)) ? (AcceptanceReport)true : "TR_OnTiberiumProducer".Translate();
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            new LookTargets(ResearchTargets).TryHighlight(true, false, true);
            GenDraw.DrawFieldEdges(new List<IntVec3>(){pos}, Color.blue);
            GenDraw.DrawFieldEdges(CellRectAt(center).ToList());
            GenDraw.DrawFieldEdges(ResearchTargets.SelectMany(p => p.OccupiedRect()).ToList(), Color.green);
        }

        public override bool ForceAllowPlaceOver(BuildableDef other)
        {
            return true;
        }
    }
}
