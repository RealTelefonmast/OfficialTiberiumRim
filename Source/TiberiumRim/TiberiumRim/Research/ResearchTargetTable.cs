using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class ResearchTargetTable : IExposable
    {
        public Dictionary<TResearchTaskDef, ScribeList<Thing>> targets = new Dictionary<TResearchTaskDef, ScribeList<Thing>>();
        private readonly Dictionary<ThingDef, List<TResearchTaskDef>> tasksForThings = new Dictionary<ThingDef, List<TResearchTaskDef>>();
        
        public ResearchTargetTable()
        {
            foreach (var task in DefDatabase<TResearchTaskDef>.AllDefs)
            {
                targets.Add(task, new ScribeList<Thing>(new List<Thing>(), LookMode.Reference));
                if(task.PossibleMainTargets.NullOrEmpty())continue;
                foreach (var thingDef in task.PossibleMainTargets)
                {
                    if(tasksForThings.ContainsKey(thingDef))
                        tasksForThings[thingDef].Add(task);
                    else 
                        tasksForThings.Add(thingDef, new List<TResearchTaskDef>(){task});
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref targets, "researchTargets", LookMode.Def, LookMode.Deep);
        }

        public List<Thing> GetTargetsFor(TResearchTaskDef task)
        {
            return targets[task].Where(Available).ToList();
        }

        public void RegisterNewTarget(Thing thing)
        {
            if (!tasksForThings.ContainsKey(thing.def)) return;
            foreach (var task in tasksForThings[thing.def])
            {
                if (targets[task].Contains(thing)) return;
                Log.Message("Registering Task target " + thing + " for " + task.LabelCap);
                targets[task].Add(thing);
                if(task.RelevantTargetStat != null)
                    targets[task].SortBy(t => t.GetStatValue(task.RelevantTargetStat));
            }
        }

        public void DeregisterTarget(Thing thing)
        {
            if (!tasksForThings.ContainsKey(thing.def)) return;
            foreach (var task in tasksForThings[thing.def])
            {
                Log.Message("Removing Task target " + thing + " for " + task.LabelCap);
                targets[task].Remove(thing);
            }
        }

        private static bool Available(Thing thing)
        {
            var compThing = thing as ThingWithComps;
            if (compThing == null) return true;
            return compThing.IsPowered(out bool usesPower) || !usesPower;
        }
    }
}
