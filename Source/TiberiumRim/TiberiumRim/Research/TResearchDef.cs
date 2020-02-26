using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

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
        public List<TResearchDef> researchProjects;

        public List<TResearchDef> ActiveProjects => researchProjects.NullOrEmpty() ? null : researchProjects.Where(t => t.RequisitesComplete).ToList();

        public bool IsFinished => researchProjects.NullOrEmpty() || researchProjects.All(r => r.IsFinished);
    }

    public class ResearchTarget
    {
        public Type targetType;
        public List<ThingDef> targetDefs;
        public string groupLabel;

        public Thing FindStation(Map map)
        {
            Thing thing = null;

            void Action(IntVec3 c)
            {
                var list = c.GetThingList(map);
                thing = list.First(t => t.GetType() == targetType || (targetDefs != null && targetDefs.Contains(t.def)));
            }

            bool Predicate(IntVec3 c) => c.IsValid && thing == null;
            var pawns = map.mapPawns.FreeColonistsSpawned;
            map.floodFiller.FloodFill(pawns.FirstOrDefault().Position, Predicate, Action, default, false,
                pawns.Select(p => p.Position));
            return thing;
        }
    }

    public class TResearchDef : Def
    {
        [Unsaved()]
        private TResearchGroupDef parentGroup;

        public Requisites requisites;

        public ResearchTarget mainTarget;
        public WorkTypeDef workType;
        public List<SkillRequirement> skillRequirements;
        public StatDef relevantPawnStat;
        public StatDef relevantTargetStat;

        public List<TResearchTaskDef> tasks;
        public List<EventDef> events;
        public string researchType = "missing";

        public virtual void FinishAction()
        {
        }

        public virtual bool RequisitesComplete => requisites?.Completed ?? true;
        public virtual bool CanStartNow => !IsFinished && RequisitesComplete;
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
                if (IsFinished)
                    return ResearchState.Finished;
                if (CanStartNow)
                    return ResearchState.Available;
                if (Equals(TRUtils.ResearchManager().currentProject))
                    return ResearchState.InProgress;
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

        //Local Requisites
        public ResearchTarget taskTarget;
        public WorkTypeDef workType;
        public List<SkillRequirement> skillRequirements;
        public StatDef relevantPawnStat;
        public StatDef relevantTargetStat;

        public List<string> images;
        public List<ThingDef> requiredFacilities;
        public List<ThingDef> requiredThings = new List<ThingDef>();
        public List<TRThingDef> unlocks = new List<TRThingDef>();
        public List<RecipeDef> recipeUnlocks = new List<RecipeDef>();

        //settings
        public float distanceFromTarget = -1;
        public PawnPosture posture = PawnPosture.Standing;

        public float workAmount = 750;
        public string descriptionShort;

        public TResearchTaskDef()
        {
        }

        public TResearchDef ParentProject
        {
            get
            {
                return parentProject ??= DefDatabase<TResearchDef>.AllDefsListForReading.FirstOrDefault((r) => r.tasks.Contains(this));
            }
        }


        public bool HasSingleTarget => PossibleMainTargets.Count == 1;
        public ThingDef MainTarget => PossibleMainTargets.FirstOrDefault();
        public ResearchTarget ResearchTarget => taskTarget ?? ParentProject.mainTarget;
        public List<ThingDef> PossibleMainTargets
        {
            get
            {
                if (mainTargets == null)
                {
                    if (ResearchTarget.targetType == null)
                        return ResearchTarget.targetDefs;
                    mainTargets = DefDatabase<ThingDef>.AllDefsListForReading.FindAll(t =>
                        t.thingClass == ResearchTarget.targetType);
                }
                return mainTargets;
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

        public virtual void FinishAction()
        {
        }

        public float ProgressPct => ProgressReal / workAmount;
        public float ProgressReal => TRUtils.ResearchManager().GetProgress(this);
        public bool IsFinished => TRUtils.ResearchManager().IsCompleted(this);

        public string WorkLabel => (int)ProgressReal + "/" + workAmount;

        public string TaskInfo
        {
            get
            {
                if (cachedTaskInfo == null)
                {
                    //Targets
                    cachedTaskInfo = "TR_TaskTargets".Translate();
                    if (ResearchTarget.groupLabel != null)
                        cachedTaskInfo += "\n " + ResearchTarget.groupLabel;
                    else if(PossibleMainTargets != null)
                    {
                        foreach (var target in PossibleMainTargets)
                        {
                            cachedTaskInfo += "\n   -" + target.LabelCap;
                            if(RelevantTargetStat != null)
                                cachedTaskInfo += " (" + target.GetStatValueAbstract(RelevantTargetStat) + ")";
                        }
                    }

                    //WorkType
                    cachedTaskInfo += "\n\n" + "TR_TaskWorkType".Translate(WorkType.labelShort);

                    //Skills
                    if (!SkillRequirements.NullOrEmpty())
                    {
                        cachedTaskInfo += "\n\n" + "TR_TaskSkillReq".Translate();
                        foreach (var skill in SkillRequirements)
                        {
                            cachedTaskInfo += "\n   -" + skill.skill.LabelCap + " (" + skill.minLevel + ")";
                        }
                    }
                }
                return cachedTaskInfo;
            }
        }

        public ResearchState State
        {
            get
            {
                if (IsFinished)
                    return ResearchState.Finished;
                if (Equals(ParentProject.CurrentTask))
                    return ResearchState.InProgress;
                return ResearchState.Available;
            }
        }
    }
}
