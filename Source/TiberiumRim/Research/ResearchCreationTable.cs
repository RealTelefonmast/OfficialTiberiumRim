using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TR
{
    /*  This Table Keeps track of Research Tasks and their "creation" goal
     *  For example creating a certain amount of a thing
     *
     */
    public class ResearchCreationTable : IExposable
    {
        //public static Dictionary<TResearchTaskDef, List<ThingDef>> taskCreationThingDefs = new Dictionary<TResearchTaskDef, List<ThingDef>>();
        public Dictionary<TResearchTaskDef, CreationGroupTracker> taskCreations = new ();
        private readonly Dictionary<ThingDef, List<TResearchTaskDef>> tasksForThings = new ();

        public ResearchCreationTable()
        {
            foreach (var task in DefDatabase<TResearchTaskDef>.AllDefs)
            {
                //taskCreationThingDefs.Add(task, new List<ThingDef>());
                if (task.creationTasks == null) continue;
                taskCreations.Add(task, new CreationGroupTracker(task));
                foreach (var creationOption in task.creationTasks.thingsToCreate)
                {
                    if(creationOption.def == null) continue;
                    //taskCreationThingDefs[task].Add(creationOption.props);
                    if(tasksForThings.ContainsKey(creationOption.def))
                        tasksForThings[creationOption.def].Add(task);
                    else
                        tasksForThings.Add(creationOption.def, new List<TResearchTaskDef>() {task});
                }
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref taskCreations, "TrackedCreations", LookMode.Def, LookMode.Deep);
        }

        public int TaskCompletion(TResearchTaskDef task)
        {
            return taskCreations[task].DoneCount;
        }

        public bool TaskComplete(TResearchTaskDef task)
        {
            return taskCreations[task].Completed;
        }

        public void TryTrackCreated(Thing thing)
        {
            if (!tasksForThings.TryGetValue(thing.def, out List<TResearchTaskDef> outList)) return;
            foreach (var task in outList)
            {
                taskCreations[task].AddCreation(thing);
                TRUtils.ResearchManager().CheckTask(task);
            }
        }

        public void TryTrackConstructedOrClaimedBuilding(ThingDef thingDef)
        {
            if (!tasksForThings.TryGetValue(thingDef, out List<TResearchTaskDef> outList)) return;
            foreach (var task in outList)
            {
                taskCreations[task].AddSingleCreation(thingDef);
                TRUtils.ResearchManager().CheckTask(task);
            }
        }
    }

    /*
     * Each Task has a creation group tracker, to track the groups of things it should create
     */

    public class CreationGroupTracker : IExposable
    {
        private TResearchTaskDef taskDef;
        private readonly Dictionary<ThingDef, List<CreationOptionProperties>> creationOptionMap = new ();
        private readonly Dictionary<CreationOptionProperties, int> thingsToCreate = new ();

        private List<int> intValues;

        public CreationGroupTracker()
        {
        }

        public CreationGroupTracker(TResearchTaskDef task)
        {
            taskDef = task;
            Setup(taskDef);
        }

        private void Setup(TResearchTaskDef task, List<int> values = null)
        {
            int i = 0;
            foreach (var creationOption in task.creationTasks.thingsToCreate)
            { 
                if (creationOption.def == null) continue;
                thingsToCreate.Add(creationOption, values?[i] ?? creationOption.amount);
                if (creationOptionMap.ContainsKey(creationOption.def))
                    creationOptionMap[creationOption.def].Add(creationOption);
                else
                    creationOptionMap.Add(creationOption.def, new List<CreationOptionProperties>() { creationOption });

                i++;
            }
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref taskDef, "taskDef");
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                intValues = thingsToCreate.Values.ToList();
            }
            Scribe_Collections.Look(ref intValues, "values");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Setup(taskDef, intValues);
            }
        }

        public int DoneCount => TotalCount - TotalCountLeft;
        public int TotalCount => thingsToCreate.Sum(t => t.Key.amount);
        public int TotalCountLeft => thingsToCreate.Sum(t => t.Value);
        public bool Completed => TotalCountLeft <= 0;

        public void AddSingleCreation(ThingDef thingDef)
        {
            if (Completed || !creationOptionMap.TryGetValue(thingDef, out List<CreationOptionProperties> options)) return;
            foreach (var option in options)
            {
                AddProgress(option, 1);
            }
        }

        public void AddCreation(Thing thing)
        {
            if (Completed || !creationOptionMap.TryGetValue(thing.def, out List<CreationOptionProperties> options)) return;
            foreach (var option in options)
            {
                if (option.Accepts(thing))
                    AddProgress(option, thing.stackCount);
            }
        }

        public void AddProgress(CreationOptionProperties optionProperties, int progress)
        {
            thingsToCreate[optionProperties] = Math.Min(Math.Max(thingsToCreate[optionProperties] - progress, 0), optionProperties.amount);
        }
    }
}
