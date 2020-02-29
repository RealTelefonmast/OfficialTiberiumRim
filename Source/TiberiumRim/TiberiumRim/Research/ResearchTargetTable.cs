using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public static class ResearchTargetTable
    {
        public static Dictionary<TResearchTaskDef, List<Thing>> targets = new Dictionary<TResearchTaskDef, List<Thing>>();
        public static Dictionary<TResearchTaskDef, Thing> bestTarget = new Dictionary<TResearchTaskDef, Thing>();
        private static readonly Dictionary<ThingDef, List<TResearchTaskDef>> tasksForThings = new Dictionary<ThingDef, List<TResearchTaskDef>>();
        
        static ResearchTargetTable()
        {
            Log.Message("Setting up Task Look-Up table");
            Log.Message("For " + DefDatabase<TResearchTaskDef>.AllDefs.Count() + " tasks");
            foreach (var task in DefDatabase<TResearchTaskDef>.AllDefs)
            {
                targets.Add(task, new List<Thing>());
                bestTarget.Add(task, null);
                if(task.PossibleMainTargets.NullOrEmpty())continue;
                foreach (var thingDef in task.PossibleMainTargets)
                {
                    if(tasksForThings.ContainsKey(thingDef))
                        tasksForThings[thingDef].Add(task);
                    else 
                        tasksForThings.Add(thingDef, new List<TResearchTaskDef>(){task});
                }
            }
            Log.Message("Created Task Look-Up table with " + tasksForThings.Count + " things");
        }

        public static List<Thing> GetTargetsFor(TResearchTaskDef task)
        {
            return targets[task].Where(Available).ToList();
        }

        public static void RegisterNewTarget(Thing thing)
        {
            if (!tasksForThings.ContainsKey(thing.def)) return;
            foreach (var task in tasksForThings[thing.def])
            {
                Log.Message("Registering Task target " + thing + " for " + task.LabelCap);
                targets[task].Add(thing);
                if(task.RelevantTargetStat != null)
                    targets[task].SortBy(t => t.GetStatValue(task.RelevantTargetStat));
            }
        }

        public static void DeregisterTarget(Thing thing)
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
