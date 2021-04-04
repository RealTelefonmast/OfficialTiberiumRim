using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using HarmonyLib;
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
        [Unsaved] private TResearchDef parentProject;
        [Unsaved] private List<ThingDef> mainTargets;
        [Unsaved] private Dictionary<string, string[]> cachedTaskInfo;
        [Unsaved()]
        private ResearchWorker workerInt;
        [Unsaved()] 
        private List<ThingDef> unlocksThings = new List<ThingDef>();

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
        //public List<ThingDef> requiredFacilities;
        //public List<ThingDef> requiredThings = new List<ThingDef>();
        //public List<TRThingDef> unlocks = new List<TRThingDef>();
        //public List<RecipeDef> recipeUnlocks = new List<RecipeDef>();

        //settings
        public float workAmount = 750;
        public float distanceFromTarget = -1;
        public float distanceRange = 1;
        public PawnPosture posture = PawnPosture.Standing;

        public bool showTargetList = true;
        //

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            foreach (var trDef in DefDatabase<TRThingDef>.AllDefs)
            {
                if (trDef.requisites == null) continue;
                if (IsLast && trDef.requisites.tiberiumResearch.Contains(ParentProject))
                    unlocksThings.Add(trDef);
                if (trDef.requisites.tiberiumResearchTasks.Contains(this))
                    unlocksThings.Add(trDef);
            }
        }

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

        public List<ThingDef> UnlocksThings => unlocksThings;

        public bool CanCheckTargets => creationTasks != null;
        //public bool TargetIsBenchOrStation => PossibleMainTargets.Any(t => t);

        public bool IsLast => ParentProject.tasks.LastOrDefault() == this;
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


        public void WriteTaskInfo(Rect rect, out float height)
        {
            GUI.BeginGroup(rect);
            float curY = 0;
            float Width = rect.width;
            //Targets
            if (CachedInfo.TryGetValue("TargetProps_Label", out string[] targetLabel))
            {
                //Target Label
                var labelHeight = Text.CalcHeight(targetLabel[0], Width);
                Rect labelRect = new Rect(0, curY, Width, labelHeight);
                Widgets.Label(labelRect, targetLabel[0]);
                curY += labelHeight;

                //Targets
                if (CachedInfo.TryGetValue("TargetProps_Targets", out string[] values))
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        float targetHeight = Text.CalcHeight(values[i], Width);
                        Rect targetRect = new Rect(0, curY, Width, targetHeight);
                        if (Mouse.IsOver(targetRect))
                            Widgets.DrawHighlight(targetRect);
                        Widgets.Label(targetRect, values[i]);
                        DoMenuOptions(targetRect, PossibleMainTargets[i], true);
                        if (Widgets.ButtonInvisible(targetRect))
                        {
                            new Dialog_InfoCard.Hyperlink(PossibleMainTargets[i]).OpenDialog();
                        }
                        curY += targetHeight;
                    }
                }
                curY += 6;
            }

            //Crafting & Construction
            if (CachedInfo.TryGetValue("CreationTasks_Label", out string[] taskLabel))
            {
                var labelHeight = Text.CalcHeight(taskLabel[0], Width);
                Rect labelRect = new Rect(0, curY, Width, labelHeight);
                Widgets.Label(labelRect, taskLabel[0]);
                curY += labelHeight;

                //Tasks
                if (CachedInfo.TryGetValue("CreationTasks_Targets", out string[] values))
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        float targetHeight = Text.CalcHeight(values[i], Width);
                        Rect targetRect = new Rect(0, curY, Width, targetHeight);
                        if(Mouse.IsOver(targetRect))
                            Widgets.DrawHighlight(targetRect);
                        Widgets.Label(targetRect, values[i]);
                        DoMenuOptions(targetRect, creationTasks.thingsToCreate[i].def, false);
                        if (Widgets.ButtonInvisible(targetRect))
                        {
                            new Dialog_InfoCard.Hyperlink(creationTasks.thingsToCreate[i].def).OpenDialog();
                        }
                        curY += targetHeight;
                    }
                }
                curY += 6;
            }

            //WorkType
            if (CachedInfo.TryGetValue("WorkType_Label", out string[] workTypeLabel))
            {
                //Target Label
                var labelHeight = Text.CalcHeight(workTypeLabel[0], Width);
                Rect labelRect = new Rect(0, curY, Width, labelHeight);
                Widgets.Label(labelRect, workTypeLabel[0]);
                curY += labelHeight;

                curY += 6;
            }

            //Skills
            if (CachedInfo.TryGetValue("SkillReq_Label", out string[] skillLabel))
            {
                var labelHeight = Text.CalcHeight(skillLabel[0], Width);
                Rect labelRect = new Rect(0, curY, Width, labelHeight);
                Widgets.Label(labelRect, skillLabel[0]);
                curY += labelHeight;

                //Skills
                if (CachedInfo.TryGetValue("SkillReq_Skills", out string[] values))
                {
                    for (var i = 0; i < values.Length; i++)
                    {
                        float targetHeight = Text.CalcHeight(values[i], Width);
                        Rect targetRect = new Rect(0, curY, Width, targetHeight);
                        Widgets.Label(targetRect, values[i]);
                        float targetWidth = Text.CalcSize(values[i]).x;
                        Rect capableRect = new Rect(targetWidth, curY, Width - targetWidth, targetHeight);
                        bool mouseOver = Mouse.IsOver(capableRect);
                        Color textCol = mouseOver ? Color.white : new Color(0.45f, 0.45f, 0.45f);
                        var pawns = CapablePawnsFor(skillRequirements[i]);
                        Widgets.Label(capableRect, (" | " + "TR_Capable".Translate().RawText + " " + pawns.Count).Colorize(textCol));
                        if (mouseOver)
                        {
                            pawns.ForEach(p => TargetHighlighter.Highlight(p, false, true, false));
                        }
                        if (!pawns.NullOrEmpty() && Widgets.ButtonInvisible(capableRect))
                        {
                            CameraJumper.TryJumpAndSelect(pawns.RandomElement());
                            Find.MainTabsRoot.EscapeCurrentTab();
                        }
                        curY += targetHeight;
                    }
                }
                curY += 6;
            }
            height = curY;
            GUI.EndGroup();
        }

        private void DoMenuOptions(Rect rect, ThingDef def, bool allowJump)
        {
            //rect = rect.ContractedBy(1);
            WidgetRow row = new WidgetRow(rect.xMax, rect.y, UIDirection.LeftThenDown, 9999f, 0);

            //Select To Construct
            if (def.IsConstructible() && row.ButtonIcon(TiberiumContent.Construct, ""))
            {
                //Find.MainTabsRoot.OpenTab.TabWindow.Close();
                //Find.MainTabsRoot.EscapeCurrentTab();
                Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Architect);
                var Architect = Find.MainTabsRoot.OpenTab.TabWindow as MainTabWindow_Architect;

                var trThingDef = def as TRThingDef;
                if (trThingDef != null && trThingDef.IsActive(out _))
                {
                    Architect.selectedDesPanel = Traverse.Create(Architect).Field("desPanelsCached").GetValue<List<ArchitectCategoryTab>>().Find(a => a.def == TiberiumDefOf.Tiberium);
                    Designator_TRMenu menu = (TiberiumDefOf.Tiberium.AllResolvedDesignators.FirstOrDefault() as Designator_TRMenu);
                    menu.Select(trThingDef);

                    new Designator_BuildFixed(trThingDef).ProcessInput(null);
                    return;
                }

                if (def.IsResearchFinished)
                {
                    Architect.selectedDesPanel = Traverse.Create(Architect).Field("desPanelsCached").GetValue<List<ArchitectCategoryTab>>().Find(a => a.def == def.designationCategory);
                    new Designator_BuildFixed(def).ProcessInput(null);
                    return;
                }
            }

            //Jump To Existing
            if (!allowJump || !TargetThings().Any()) return;
            var thing = TargetThings().Where(d => d.def == def);
            if (thing.Any() && row.ButtonIcon(TiberiumContent.SelectThing, ""))
            {
                //if (!TargetThings().Any()) return;
                //Thing thing = TargetThings().RandomElement(); //FirstOrDefault(t => t.def == def); //Find.Maps.SelectMany(m => m.listerThings.ThingsOfDef(def)).RandomElement();
                Find.MainTabsRoot.EscapeCurrentTab();
                CameraJumper.TryJumpAndSelect(thing.RandomElement());
            }
        }

        public Dictionary<string, string[]> CachedInfo
        {
            get
            {
                if (cachedTaskInfo == null)
                {
                    cachedTaskInfo = new Dictionary<string, string[]>();
                    CacheTaskInfo();
                }
                return cachedTaskInfo;
            }
        }

        public void CacheTaskInfo()
        {
            //Targets
            if (TargetProperties != null)
            {
                var singleArr = new []{RelevantTargetStat != null
                    ? "TR_TaskBenches".Translate(RelevantTargetStat.LabelCap.RawText.Colorize("#FFE164")).RawText
                    : "TR_TaskTargets".Translate().RawText};
                cachedTaskInfo.Add("TargetProps_Label", singleArr);


                if (!TargetProperties.groupLabel.NullOrEmpty())
                    cachedTaskInfo.Add("TargetProps_GroupLabel", new []{TargetProperties.groupLabel});

                if (showTargetList)
                {
                    var targetArr = new string[PossibleMainTargets.Count];
                    for (var i = 0; i < PossibleMainTargets.Count; i++)
                    {
                        var target = PossibleMainTargets[i];
                        var targetText = "  -" + target.LabelCap.RawText;
                        if (RelevantTargetStat != null)
                            targetText += "  (" + (target.GetStatValueAbstract(RelevantTargetStat) + "x").Colorize("#FFE164") + ")";

                        targetArr[i] = targetText;
                    }
                    cachedTaskInfo.Add("TargetProps_Targets", targetArr);
                }
            }

            //Crafting & Construction
            if (creationTasks != null)
            {
                cachedTaskInfo.Add("CreationTasks_Label", new []{creationTasks.TargetLabel()});
                var taskArr = new string[creationTasks.thingsToCreate.Count];
                for (var i = 0; i < creationTasks.thingsToCreate.Count; i++)
                {
                    var option = creationTasks.thingsToCreate[i];
                    var targetText = "    -";
                    if (option.amount > 1)
                        targetText += option.amount + "x ";
                    targetText += option.def.LabelCap.RawText;
                    if (option.stuffDef != null)
                        targetText += "(" + option.stuffDef.LabelAsStuff + ")";

                    taskArr[i] = targetText;
                }
                cachedTaskInfo.Add("CreationTasks_Targets", taskArr);
            }

            //WorkType
            cachedTaskInfo.Add("WorkType_Label", new []{"TR_TaskWorkType".Translate(WorkType.labelShort.Colorize("#00C8CC")).RawText});

            //Skills
            if (!SkillRequirements.NullOrEmpty())
            {
                cachedTaskInfo.Add("SkillReq_Label", new []{"TR_TaskSkillReq".Translate().RawText});
                var skillArr = new string[SkillRequirements.Count];
                for (var i = 0; i < SkillRequirements.Count; i++)
                {
                    var skill = SkillRequirements[i];
                    skillArr[i] = "    -" + skill.skill.LabelCap.RawText + " (" + skill.minLevel + ")";
                }
                cachedTaskInfo.Add("SkillReq_Skills", skillArr);
            }
        }

        public List<Pawn> CapablePawns()
        {
            var pawns = new List<Pawn>();
            if (skillRequirements == null) return null;
            foreach (var skillRequirement in skillRequirements)
            {
                pawns.AddRange(CapablePawnsFor(skillRequirement));
            }
            return pawns;
        }

        public List<Pawn> CapablePawnsFor(SkillRequirement skillReq)
        {
            var pawns = new List<Pawn>();
            foreach (var map in Find.Maps)
            {
                pawns.AddRange(map.mapPawns.AllPawns.Where(p => p.IsColonist && skillReq.PawnSatisfies(p)));   
            }
            return pawns;
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
