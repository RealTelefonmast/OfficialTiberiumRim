using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class ResearchCreationTable
    {
        //public static Dictionary<TResearchTaskDef, List<ThingDef>> taskCreationThingDefs = new Dictionary<TResearchTaskDef, List<ThingDef>>();
        public Dictionary<TResearchTaskDef, CreationGroupTracker> taskCreations = new Dictionary<TResearchTaskDef, CreationGroupTracker>();
        private readonly Dictionary<ThingDef, List<TResearchTaskDef>> tasksForThings = new Dictionary<ThingDef, List<TResearchTaskDef>>();

        public ResearchCreationTable()
        {
            foreach (var task in DefDatabase<TResearchTaskDef>.AllDefs)
            {
                //taskCreationThingDefs.Add(task, new List<ThingDef>());
                if (task.creationTasks == null) continue;
                taskCreations.Add(task, new CreationGroupTracker(task.creationTasks));
                foreach (var creationOption in task.creationTasks.thingsToCreate)
                {
                    //taskCreationThingDefs[task].Add(creationOption.def);
                    if(tasksForThings.ContainsKey(creationOption.def))
                        tasksForThings[creationOption.def].Add(task);
                    else
                        tasksForThings.Add(creationOption.def, new List<TResearchTaskDef>() {task});
                }
            }
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
            foreach (var task in tasksForThings[thing.def])
            {
                taskCreations[task].AddCreation(thing);
                TRUtils.ResearchManager().CheckTask(task);
            }
        }

        public void TryTrackCreated(ThingDef thingDef)
        {
            if (!tasksForThings.TryGetValue(thingDef, out List<TResearchTaskDef> outList)) return;
            foreach (var task in outList)
            {
                taskCreations[task].AddSingleCreation(thingDef);
                TRUtils.ResearchManager().CheckTask(task);
            }
        }
    }

    public class CreationGroupTracker
    {
        private readonly Dictionary<ThingDef, List<CreationOption>> creationOptionMap = new Dictionary<ThingDef, List<CreationOption>>();
        private readonly Dictionary<CreationOption, int> thingsToCreate = new Dictionary<CreationOption, int>();

        public CreationGroupTracker(CreationProperties properties)
        {
            foreach (var creationOption in properties.thingsToCreate)
            {
                thingsToCreate.Add(creationOption, creationOption.amount);

                if (creationOptionMap.ContainsKey(creationOption.def))
                    creationOptionMap[creationOption.def].Add(creationOption);
                else
                    creationOptionMap.Add(creationOption.def, new List<CreationOption>() {creationOption});
            }
        }

        public int DoneCount => TotalCount - TotalCountLeft;
        public int TotalCount => thingsToCreate.Sum(t => t.Key.amount);
        public int TotalCountLeft => thingsToCreate.Sum(t => t.Value);
        public bool Completed => TotalCountLeft <= 0;

        public void AddSingleCreation(ThingDef thingDef)
        {
            if (Completed || !creationOptionMap.TryGetValue(thingDef, out List<CreationOption> options)) return;
            foreach (var option in options)
            {
                AddProgress(option, 1);
            }
        }

        public void AddCreation(Thing thing)
        {
            if (Completed || !creationOptionMap.TryGetValue(thing.def, out List<CreationOption> options)) return;
            foreach (var option in options)
            {
                if (option.quality == null || thing.TryGetQuality(out QualityCategory qc) && qc == option.quality)
                    AddProgress(option, thing.stackCount);
            }
        }

        public void AddProgress(CreationOption option, int progress)
        {
            thingsToCreate[option] = Math.Min(Math.Max(thingsToCreate[option] - progress, 0), option.amount);
        }
    }
}
