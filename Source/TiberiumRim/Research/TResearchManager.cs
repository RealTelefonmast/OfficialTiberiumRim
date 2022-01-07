using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TResearchManager : WorldComponent, IExposable //, ISynchronizable
    {
        //Research Progress Data
        public Dictionary<TResearchTaskDef, float> TaskProgress = new Dictionary<TResearchTaskDef, float>();
        public Dictionary<TResearchTaskDef, bool> TasksCompleted = new Dictionary<TResearchTaskDef, bool>();
        public Dictionary<TResearchDef, bool> ResearchCompleted = new Dictionary<TResearchDef, bool>();

        public ResearchCreationTable creationTable;
        public ResearchTargetTable researchTargets;

        //Research Menu
        private readonly Dictionary<TResearchGroupDef, bool[]> researchGroupData = new Dictionary<TResearchGroupDef, bool[]>();

        private TResearchTaskDef taskOverride;
        private TResearchDef currentProject;

        public TResearchTaskDef TaskOverride
        {
            get => taskOverride;
            [SyncMethod]
            set => taskOverride = value;
        }

        public TResearchDef CurrentProject
        {
            get => currentProject;
            [SyncMethod]
            set => currentProject = value;
        }

        public List<TResearchGroupDef> Groups => researchGroupData.Keys.ToList();

        //Static data
        public static float researchFactor = 0.01f;
        public static bool hideGroups, hideMissions;

        public TResearchManager(World world) : base(world)
        {
            var sorted = DefDatabase<TResearchGroupDef>.AllDefs.ToList();
            sorted.SortBy(t => t.priority);
            foreach (var group in sorted)
            {
                researchGroupData.Add(group, new bool[2] {false, false});
            }
            researchTargets = new ResearchTargetTable();
            creationTable = new ResearchCreationTable();
        }

        /*
        public void Sync(SyncWorker sync)
        {
            sync.Bind(ref this.currentProject);
            sync.Bind(ref this.TaskProgress);
            sync.Bind(ref this.TasksCompleted);
            sync.Bind(ref this.ResearchCompleted);
        }
        */

        [SyncWorker]
        static void SyncTResearchManager(SyncWorker sync, ref TResearchManager type)
        {
            type = Find.World.GetComponent<TResearchManager>();
        }

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref currentProject, "currentProj");
            Scribe_Collections.Look(ref TaskProgress, "TaskProgress", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref TasksCompleted, "TasksCompleted", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref ResearchCompleted, "ResearchCompleted", LookMode.Def, LookMode.Value);
            Scribe_Deep.Look(ref researchTargets, "researchTargets");
            Scribe_Deep.Look(ref creationTable, "creationTable");
        }

        private static int checkTick = 2000;
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (CurrentProject == null)
                return;

            if (checkTick <= 0)
            {
                CheckGroup(CurrentProject.ParentGroup);
                checkTick = 2000;
            }
            checkTick--;
        }

        [SyncMethod]
        public void StartResearch(TResearchDef project, bool sameFlag)
        {
            if (!sameFlag)
            {
                Messages.Message("TR_StartedProject".Translate(project.LabelCap), MessageTypeDefOf.NeutralEvent, false);
                CurrentProject = project;
            }
            else
            {
                CurrentProject = null;
            }
        }

        public void DoCompletionDialog(TResearchDef proj)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(proj.projectDescription);
            DiaNode diaNode = new DiaNode(stringBuilder.ToString());
            diaNode.options.Add(DiaOption.DefaultOK);
            DiaOption diaOption = new DiaOption("TR_OpenTab".Translate());
            diaOption.action = delegate ()
            {
                Find.MainTabsRoot.SetCurrentTab(TiberiumDefOf.TiberiumTab);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, "TR_ResearchProjectDone".Translate(proj.LabelCap)));
        }

        private void CheckGroup(TResearchGroupDef group)
        {
            TLog.Debug($"Checking Research Group {group}");
            if (group.IsFinished)
                return;
            foreach (var research in group.researchProjects)
            {
                if (!CheckResearch(research))
                    return;
            }
            Complete(group);
        }

        private bool CheckResearch(TResearchDef research)
        {
            TLog.Debug($"Checking Research Project {research}");
            if (research.IsFinished)
                return false;

            foreach (var task in research.tasks)
            {
                if (!CheckTask(task))
                    return false;
            }
            Complete(research);
            return true;
        }

        public bool CheckTask(TResearchTaskDef task)
        {
            TLog.Debug($"Checking Research Task '{task}'");
            if (IsCompleted(task))
                return true;
            if (task.ProgressToDo > 0 && task.ProgressReal < task.ProgressToDo)
                return false;
            if (!task.Worker.PlayerTaskCompleted())
                return false;
            SetCompleted(task, true);
            return true;
        }

        public bool TaskActive(TResearchTaskDef task)
        {
            return CurrentProject != null && CurrentProject.CurrentTask == task;
        }

        //Research Groups
        public bool IsOpen(TResearchGroupDef group)
        {
            return researchGroupData[group][0];
        }

        public void OpenClose(TResearchGroupDef group)
        {
            researchGroupData[group][0] = !researchGroupData[group][0];
        }

        [SyncMethod(SyncContext.None)]
        public void Complete(TResearchGroupDef group)
        {
            TLog.Debug($"Completing Research Group {group}");
            researchGroupData[group][1] = true;
        }

        public bool IsCompleted(TResearchGroupDef group)
        {
            return researchGroupData[group][1];
        }

        //Research Projects
        [SyncMethod(SyncContext.None)]
        public void Complete(TResearchDef researchDef)
        {
            TLog.Debug($"Completing Research Project {researchDef}");
            if (!ResearchCompleted.ContainsKey(researchDef))
                ResearchCompleted.Add(researchDef, true);

            researchDef.TriggerEvents();
            researchDef.FinishAction();
            CurrentProject = null;
            DoCompletionDialog(researchDef);
            CheckGroup(researchDef.ParentGroup);
        }

        public bool IsCompleted(TResearchDef research)
        {
            return ResearchCompleted.TryGetValue(research, out bool value) && value;
            //return progress.TryGetValue(props, out float weight) && weight >= props.baseCost;
        }

        //Research Tasks
        [SyncMethod(SyncContext.None)]
        public void SetCompleted(TResearchTaskDef task, bool completed)
        {
            TLog.Debug($"Completing Research Task {task} -> {completed}");
            if (!TasksCompleted.ContainsKey(task))
            {
                TasksCompleted.Add(task, completed);
            }
            TasksCompleted[task] = completed;
            if (completed)
            {
                TLog.Debug($"Doing completed actions...");
                task.DoDiscoveries();
                task.TriggerEvents();
                task.Worker.FinishAction();
                TaskOverride = null;
                Messages.Message("TR_ResearchTaskDone".Translate(task.LabelCap), MessageTypeDefOf.TaskCompletion, false);
                CheckResearch(task.ParentProject);
            }

        }

        public bool IsCompleted(TResearchTaskDef task)
        {
            return TasksCompleted.TryGetValue(task, out bool value) && value;
            //return progress.TryGetValue(props, out float weight) && weight >= props.baseCost;
        }

        public float GetProgress(TResearchTaskDef task)
        {
            if (TaskProgress.TryGetValue(task, out var result))
                return result;

            TaskProgress.Add(task, 0f);
            return 0f;
        }

        public void PerformResearch(TResearchTaskDef task, Pawn researcher, float value)
        {
            value *= researchFactor;
            if (DebugSettings.fastResearch)
            {
                value *= 1000;
            }
            researcher?.records.AddTo(RecordDefOf.ResearchPointsResearched, value);
            if (task != null)
            {
                AddProgress(task, value);
            }
        }

        public void AddProgress(TResearchTaskDef task, float value)
        {
            float progress = GetProgress(task);
            SetProgress(task, Mathf.Min(progress + value, task.ProgressToDo));
        }

        public void SetProgress(TResearchTaskDef task, float f)
        {
            if (TaskProgress.ContainsKey(task))
                TaskProgress[task] = f;
            else
                TaskProgress.Add(task, f);

            CheckTask(task);
        }
    }
}
