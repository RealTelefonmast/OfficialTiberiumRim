using System;
using System.Collections.Generic;
using System.Linq;
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
            workType = DefDatabase<WorkTypeDef>.GetNamed("Research");
            relevantPawnStat = StatDefOf.ResearchSpeed;
            relevantTargetStat = StatDefOf.ResearchSpeedFactor;
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
        [Unsaved]
        private List<Texture2D> cachedImages;

        //Local Requisites
        public CreationProperties creationTasks;
        public TargetProperties taskTarget;
        public DiscoveryList discoveries;
        public WorkTypeDef workType;
        public List<SkillRequirement> skillRequirements;
        public StatDef relevantPawnStat;
        public StatDef relevantTargetStat;

        public List<EventDef> events;

        public List<string> images;
        //TODO: Look at base.Map.listerBuildings.ColonistsHaveBuilding for building requisites
        public List<ThingDef> requiredFacilities;
        public List<ThingDef> requiredThings = new List<ThingDef>();
        public List<TRThingDef> unlocks = new List<TRThingDef>();
        public List<RecipeDef> recipeUnlocks = new List<RecipeDef>();

        //settings
        public float distanceFromTarget = -1;
        public FloatRange distanceRange = FloatRange.One;
        public PawnPosture posture = PawnPosture.Standing;

        public float workAmount = 750;
        public bool showTargetList = true;
        //
        public string descriptionShort;

        public TResearchTaskDef()
        {
        }

        public TResearchDef ParentProject
        {
            get
            {
                return parentProject ??= DefDatabase<TResearchDef>.AllDefsListForReading.FirstOrDefault((r) => !r.tasks.NullOrEmpty() && r.tasks.Contains(this));
            }
        }

        public bool CanCheckTargets => creationTasks != null;

        public bool HasAnyTarget => !PossibleMainTargets.NullOrEmpty();
        public bool HasSingleTarget => PossibleMainTargets.Count == 1;
        public ThingDef MainTarget => PossibleMainTargets.FirstOrDefault();
        public TargetProperties TargetProperties => taskTarget ?? ParentProject.mainTarget;
        public List<ThingDef> PossibleMainTargets
        {
            get
            {
                if (TargetProperties == null) return null;
                if (mainTargets == null)
                {
                    if (TargetProperties.targetType == null)
                        return TargetProperties.targetDefs;
                    mainTargets = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t =>
                        t.thingClass == TargetProperties.targetType);
                }
                return mainTargets;
            }
        }

        public List<Texture2D> Images
        {
            get
            {
                if (images.NullOrEmpty()) return null;
                if (cachedImages == null)
                {
                    cachedImages = new List<Texture2D>(images.Count);
                    foreach(var image in images)
                        cachedImages.Add(ContentFinder<Texture2D>.Get(image));
                }
                return cachedImages;
            }
        }

        public WorkTypeDef WorkType => workType ?? ParentProject.workType;
        public List<SkillRequirement> SkillRequirements => skillRequirements ?? ParentProject.skillRequirements;
        public StatDef RelevantPawnStat => relevantPawnStat ?? ParentProject.relevantPawnStat;
        public StatDef RelevantTargetStat => relevantTargetStat ?? ParentProject.relevantTargetStat;

        //The task check acts allows the task to have a goal for the player
        public virtual bool PlayerTaskCompleted()
        {
            return true;
        }

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

        public virtual void FinishAction()
        {
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
                //Targets
                if (TargetProperties != null)
                {
                    cachedTaskInfo += "TR_TaskTargets".Translate() + "\n";
                    if (!TargetProperties.groupLabel.NullOrEmpty())
                        cachedTaskInfo += TargetProperties.groupLabel + "\n";
                    if(showTargetList)
                    {
                        foreach (var target in PossibleMainTargets)
                        {
                            cachedTaskInfo += "   -" + target.LabelCap;
                            if (RelevantTargetStat != null)
                                cachedTaskInfo += " (" + RelevantTargetStat.LabelCap + ": " + target.GetStatValueAbstract(RelevantTargetStat) + ")";
                            cachedTaskInfo += "\n";
                        }
                    }
                    cachedTaskInfo += "\n";
                }
                //Crafting & Construction
                if (creationTasks != null)
                {
                    cachedTaskInfo += creationTasks.TargetLabel() + "\n";
                    foreach (var option in creationTasks.thingsToCreate)
                    {
                        cachedTaskInfo += "    -";
                        if (option.amount > 1)
                            cachedTaskInfo += option.amount + "x ";
                        cachedTaskInfo += option.def.LabelCap;
                        if(option.stuffDef != null)
                            cachedTaskInfo += "(" + option.stuffDef.LabelAsStuff + ")";
                        cachedTaskInfo += "\n";
                    }

                    cachedTaskInfo += "\n";
                }

                //WorkType
                cachedTaskInfo += "TR_TaskWorkType".Translate(WorkType.labelShort) + "\n\n";

                //Skills
                if (!SkillRequirements.NullOrEmpty())
                {
                    cachedTaskInfo += "TR_TaskSkillReq".Translate() + "\n";
                    foreach (var skill in SkillRequirements)
                    {
                        cachedTaskInfo += "    -" + skill.skill.LabelCap + " (" + skill.minLevel + ")\n";
                    }
                }
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
}
