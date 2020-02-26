using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TiberiumRim
{
    public class TResearchManager : WorldComponent, IExposable
    {

        public Dictionary<TResearchTaskDef, float> TaskProgress = new Dictionary<TResearchTaskDef, float>();
        public Dictionary<TResearchTaskDef, bool> TasksCompleted = new Dictionary<TResearchTaskDef, bool>();
        public Dictionary<TResearchDef, bool> ResearchCompleted = new Dictionary<TResearchDef, bool>();
        public static float researchFactor = 0.01f;

        //Date for the existing groups - [open in tab] [finished]

        //Research Window
        private readonly Dictionary<TResearchGroupDef, bool[]> researchGroupData = new Dictionary<TResearchGroupDef, bool[]>();
        public static bool hideGroups, hideMissions;

        public TResearchDef currentProject;

        public TResearchManager(World world) : base(world)
        {
            foreach (var group in DefDatabase<TResearchGroupDef>.AllDefs)
            {
                researchGroupData.Add(group, new bool[2] {false, false});
            }
        }

        public List<TResearchGroupDef> Groups => researchGroupData.Keys.ToList();
        public List<TResearchDef> AllProjects => DefDatabase<TResearchDef>.AllDefsListForReading;
        public List<TResearchTaskDef> AllTasks => DefDatabase<TResearchTaskDef>.AllDefsListForReading;

        public override void ExposeData()
        {
            Scribe_Defs.Look(ref currentProject, "currentProj");
            Scribe_Collections.Look(ref TaskProgress, "TaskProgress", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref TasksCompleted, "TasksCompleted", LookMode.Def, LookMode.Value);
            Scribe_Collections.Look(ref ResearchCompleted, "ResearchCompleted", LookMode.Def, LookMode.Value);
        }

        private static int checkTick = 2000;
        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (currentProject == null)
                return;

            if (checkTick <= 0)
            {
                CheckGroup(currentProject.ParentGroup);
                checkTick = 2000;
            }
            checkTick--;
        }

        public void StartResearch(TResearchDef project)
        {
            currentProject = project.Equals(currentProject) ? null : project;
        }

        public void DoCompletionDialog(TResearchDef proj)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("TiberiumRimResearchCompletion".Translate(proj.LabelCap, proj.description));
            DiaNode diaNode = new DiaNode(stringBuilder.ToString());
            diaNode.options.Add(DiaOption.DefaultOK);
            DiaOption diaOption = new DiaOption("TR_OpenTab".Translate());
            diaOption.action = delegate ()
            {
                Find.MainTabsRoot.SetCurrentTab(TiberiumDefOf.TiberiumTab);
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, "ResearchComplete".Translate()));
        }

        private void CheckGroup(TResearchGroupDef group)
        {
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
            if (research.IsFinished)
                return false;
            foreach (var task in research.tasks)
            {
                if (!CheckTask(task))
                    return false;
            }
            Complete(research);
            currentProject = null;
            DoCompletionDialog(research);
            CheckGroup(research.ParentGroup);
            return true;
        }

        private bool CheckTask(TResearchTaskDef task)
        {
            if (IsCompleted(task))
                return true;
            if (task.workAmount > 0 && GetProgress(task) < task.workAmount)
                return false;
            if (!task.PlayerTaskCompleted())
                return false;
            SetCompleted(task, true);
            task.FinishAction();
            CheckResearch(task.ParentProject);
            return true;
        }

        public bool TaskActive(TResearchTaskDef task)
        {
            return currentProject != null && currentProject.CurrentTask == task;
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

        public void Complete(TResearchGroupDef group)
        {
            researchGroupData[group][1] = true;
        }

        public bool IsCompleted(TResearchGroupDef group)
        {
            return researchGroupData[group][1];
        }

        //Research Projects
        public void Complete(TResearchDef def)
        {
            if (!ResearchCompleted.ContainsKey(def))
                ResearchCompleted.Add(def, true);
        }

        public bool IsCompleted(TResearchDef research)
        {
            return ResearchCompleted.TryGetValue(research, out bool value) && value;
            //return progress.TryGetValue(def, out float value) && value >= def.baseCost;
        }

        //Research Tasks
        public void SetCompleted(TResearchTaskDef task, bool completed)
        {
            if (!TasksCompleted.ContainsKey(task))
            {
                TasksCompleted.Add(task, completed);
                return;
            }
            TasksCompleted[task] = completed;
        }

        public bool IsCompleted(TResearchTaskDef task)
        {
            return TasksCompleted.TryGetValue(task, out bool value) && value;
            //return progress.TryGetValue(def, out float value) && value >= def.baseCost;
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
                float progress = GetProgress(task);
                SetProgress(task, Mathf.Min(progress + value, task.workAmount));
            }
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
