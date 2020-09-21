using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public enum ResearchState
    {
        Finished,
        InProgress,
        Available,
        Hidden
    }

    public class TResearchGroupDef : Def
    {
        public int priority = 0;
        public List<TResearchDef> researchProjects;

        public List<TResearchDef> ActiveProjects => researchProjects.NullOrEmpty() ? null : researchProjects.Where(t => t.RequisitesComplete).ToList();

        public bool IsVisible => !IsFinished && !ActiveProjects.NullOrEmpty() || (IsFinished && !TResearchManager.hideGroups);
        public bool IsFinished => researchProjects.NullOrEmpty() || researchProjects.All(r => r.IsFinished);

        public bool HasUnseenProjects => ActiveProjects.Any(t => !t.HasBeenSeen);
    }

    public class TResearchDef : Def
    {
        [Unsaved()]
        private TResearchGroupDef parentGroup;

        public Requisites requisites;

        public TargetProperties mainTarget;
        public WorkTypeDef workType;
        public List<SkillRequirement> skillRequirements;
        public StatDef relevantPawnStat;
        public StatDef relevantTargetStat;

        public List<TResearchTaskDef> tasks;
        public List<EventDef> events;
        public string researchType = "missing";
        public string projectDescription;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            //workType = DefDatabase<WorkTypeDef>.GetNamed("Research");
            //relevantPawnStat = StatDefOf.ResearchSpeed;
            //relevantTargetStat = StatDefOf.ResearchSpeedFactor;
        }

        public void TriggerEvents()
        {
            if (events.NullOrEmpty()) return;
            foreach (var @event in events)
            {
                TRUtils.EventManager().StartEvent(@event);
            }
        }

        public virtual void FinishAction()
        {
        }

        public virtual bool RequisitesComplete => requisites?.FulFilled() ?? true;
        public virtual bool CanStartNow => RequisitesComplete;
        public bool IsFinished => TRUtils.ResearchManager().IsCompleted(this);
        public bool HasBeenSeen => TRUtils.DiscoveryTable().ResearchHasBeenSeen(this);

        public TResearchGroupDef ParentGroup
        {
            get
            {
                return parentGroup ??= DefDatabase<TResearchGroupDef>.AllDefsListForReading.FirstOrDefault((r) => r.researchProjects.Contains(this));
            }
        }

        public virtual TResearchTaskDef CurrentTask
        {
            get
            {
                return tasks.FirstOrDefault((task) => !task.IsFinished);
            }
        }

        public ResearchState State
        {
            get
            {
                if (Equals(TRUtils.ResearchManager().currentProject))
                    return ResearchState.InProgress;
                if (IsFinished)
                    return ResearchState.Finished;
                if (CanStartNow)
                    return ResearchState.Available;
                return ResearchState.Hidden;
            }
        }
    }

    public class TResearchTaskDef : Def
    {
        [Unsaved]
        private TResearchDef parentProject;
        [Unsaved]
        private List<ThingDef> mainTargets;
        [Unsaved]
        private string cachedTaskInfo;
        [Unsaved()]
        private ResearchWorker workerInt;

        public Type researchWorker = typeof(ResearchWorker);
        //Local Requisites
        public CreationProperties creationTasks;
        public TargetProperties taskTarget;
        public DiscoveryList discoveries;
        public WorkTypeDef workType;
        public List<SkillRequirement> skillRequirements;
        public StatDef relevantPawnStat;
        public StatDef relevantTargetStat;

        public List<EventDef> events;

        public string descriptionShort;
        public List<string> images;

        //TODO: Look at base.Map.listerBuildings.ColonistsHaveBuilding for building requisites
        public List<ThingDef> requiredFacilities;
        public List<ThingDef> requiredThings = new List<ThingDef>();
        public List<TRThingDef> unlocks = new List<TRThingDef>();
        public List<RecipeDef> recipeUnlocks = new List<RecipeDef>();

        //settings
        public float workAmount = 750;
        public float distanceFromTarget = -1;
        public float distanceRange = 1;
        public PawnPosture posture = PawnPosture.Standing;

        public bool showTargetList = true;
        //

        public ResearchWorker Worker
        {
            get
            {
                return workerInt ??= (ResearchWorker)System.Activator.CreateInstance(researchWorker);
            }
        }

        public TResearchDef ParentProject
        {
            get
            {
                return parentProject ??= DefDatabase<TResearchDef>.AllDefsListForReading.FirstOrDefault((r) => !r.tasks.NullOrEmpty() && r.tasks.Contains(this));
            }
        }

        public bool CanCheckTargets => creationTasks != null;
        //public bool TargetIsBenchOrStation => PossibleMainTargets.Any(t => t);

        public bool HasAnyTarget => !PossibleMainTargets.NullOrEmpty();
        public bool HasSingleTarget => HasAnyTarget && PossibleMainTargets.Count == 1;
        public ThingDef MainTarget => PossibleMainTargets?.FirstOrDefault();
        public TargetProperties TargetProperties => taskTarget ?? ParentProject.mainTarget;

        //Targets
        public List<ThingDef> PossibleMainTargets
        {
            get
            {
                if (TargetProperties == null) return null;
                if (mainTargets == null)
                {
                    if (TargetProperties.targetType == null)
                        return TargetProperties.targetDefs;
                    mainTargets = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t => t.thingClass == TargetProperties.targetType);
                }
                return mainTargets;
            }
        }

        public IEnumerable<Thing> TargetThings()
        {
           return TRUtils.ResearchTargetTable().GetTargetsFor(this);
        }

        public IEnumerable<IntVec3> TargetInteractionPositions()
        {
            return TargetThings().Select(t => t.InteractionCell);
        }

        public IEnumerable<IntVec3> TargetPositions(IntVec3 origin)
        {
            return GenRadial.RadialCellsAround(origin, distanceFromTarget - distanceRange/2, distanceFromTarget + (distanceRange/2));
        }

        public IEnumerable<IntVec3> TargetDistancePositions()
        {
            return TargetThings().SelectMany(t => TargetPositions(t.Position));
        }

        public WorkTypeDef WorkType => workType ?? ParentProject.workType;
        public List<SkillRequirement> SkillRequirements => skillRequirements ?? ParentProject.skillRequirements;
        public StatDef RelevantPawnStat => relevantPawnStat ?? ParentProject.relevantPawnStat;
        public StatDef RelevantTargetStat => relevantTargetStat ?? ParentProject.relevantTargetStat;

        public void TriggerEvents()
        {
            if (events.NullOrEmpty()) return;
            foreach (var @event in events)
            {
                TRUtils.EventManager().StartEvent(@event);
            }
        }

        public void DoDiscoveries()
        {
            discoveries?.Discover();
        }

        public void Debug_Finish()
        {
            if (this.creationTasks != null)
            {
                foreach (var option in this.creationTasks.thingsToCreate)
                {
                    TRUtils.ResearchCreationTable().taskCreations[this].AddProgress(option, option.amount);
                }
            }
            TRUtils.ResearchManager().SetProgress(this, this.ProgressToDo);
        }

        public void Debug_Reset()
        {
            TRUtils.ResearchManager().SetProgress(this, 0);
            TRUtils.ResearchManager().SetCompleted(this, false);
            TRUtils.ResearchManager().ResearchCompleted.Remove(this.ParentProject);
            if (this.creationTasks != null)
            {
                foreach (var option in this.creationTasks.thingsToCreate)
                {
                    TRUtils.ResearchCreationTable().taskCreations[this].AddProgress(option, -option.amount);
                }
            }
        }

        public virtual float ProgressReal => creationTasks != null ? TRUtils.ResearchCreationTable().TaskCompletion(this) : TRUtils.ResearchManager().GetProgress(this);
        public virtual float ProgressToDo => creationTasks?.TotalCountToMake ?? workAmount;

        public float ProgressPct => ProgressReal / ProgressToDo;
        public bool IsFinished => TRUtils.ResearchManager().IsCompleted(this);

        public string WorkLabel => (int)ProgressReal + "/" + ProgressToDo;

        public string TaskInfo
        {
            get
            {
                if (cachedTaskInfo != null) return cachedTaskInfo;
                StringBuilder sb = new StringBuilder();
                //Targets
                if (TargetProperties != null)
                {
                    //Log.Message("<color=#FFC800>" + "TR_TaskBenches".Translate((RelevantTargetStat.LabelCap)).RawText + "</color>");
                    //Log.Message("TR_TaskBenches".Translate(("<color=#FFC800>" + RelevantTargetStat.LabelCap.RawText + "</color>")).RawText);

                    sb.AppendLine(RelevantTargetStat != null
                        ? "TR_TaskBenches".Translate(("<color=#FFE164>" + RelevantTargetStat.LabelCap.RawText + "</color>")).RawText
                        : "TR_TaskTargets".Translate().RawText);

                    if (!TargetProperties.groupLabel.NullOrEmpty())
                        sb.AppendLine(TargetProperties.groupLabel);
                    
                    if (showTargetList)
                    {
                        foreach (var target in PossibleMainTargets)
                        {
                            sb.Append("  -" + target.LabelCap.RawText);
                            if (RelevantTargetStat != null)
                                sb.Append(("  (<color=#FFE164>" + target.GetStatValueAbstract(RelevantTargetStat) + "x</color>)"));
                            //cachedTaskInfo += " (" + RelevantTargetStat.LabelCap + ": " + target.GetStatValueAbstract(RelevantTargetStat) + ")";
                            sb.AppendLine();
                        }
                    }
                    sb.AppendLine();
                }

                //Crafting & Construction
                if (creationTasks != null)
                {
                    sb.AppendLine(creationTasks.TargetLabel());
                    foreach (var option in creationTasks.thingsToCreate)
                    {
                        sb.Append("    -");
                        if (option.amount > 1)
                            sb.Append(option.amount + "x ");
                        sb.Append(option.def.LabelCap.RawText);
                        if(option.stuffDef != null)
                            sb.Append("(" + option.stuffDef.LabelAsStuff + ")");
                        sb.AppendLine();
                    }
                    sb.AppendLine();
                }

                //WorkType
                sb.AppendLine("TR_TaskWorkType".Translate("<color=#00C8CC>" + WorkType.labelShort + "</color>").RawText);
                sb.AppendLine();

                //Skills
                if (!SkillRequirements.NullOrEmpty())
                {
                    sb.AppendLine("TR_TaskSkillReq".Translate().RawText);
                    foreach (var skill in SkillRequirements)
                    {
                        sb.Append("    -" + skill.skill.LabelCap.RawText + " (" + skill.minLevel + ")");
                    }
                }
                cachedTaskInfo = sb.ToString();
                return cachedTaskInfo;
            }
        }

        public ResearchState State
        {
            get
            {
                if (Equals(ParentProject.CurrentTask))
                    return ResearchState.InProgress;
                if (IsFinished)
                    return ResearchState.Finished;
                return ResearchState.Available;
            }
        }
    }

    /*  ResearchWorker is going to contain valuable data such as culprits for events
     *  ResearchWorkers can vary in their completion task
     *
     */

    public class ResearchWorker
    {
        public TResearchTaskDef def;
        public TargetInfo Culprit;

        public ResearchWorker(){}

        public ResearchWorker(TResearchTaskDef def)
        {
            this.def = def;
        }

        public void RegisterCulprit(Thing thing)
        {
            Culprit = new TargetInfo(thing);
        }

        //The task check acts allows the task to have a goal for the player
        public virtual bool PlayerTaskCompleted()
        {
            return true;
        }

        public virtual void FinishAction()
        {
        }
    }
}
