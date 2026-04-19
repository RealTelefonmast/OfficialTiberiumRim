using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public enum ResearchTabOption
{
    Projects,
    Events
}

public class MainTabWindow_TibResearch : MainTabWindow
{
    //

    //Dimensions
    private static readonly float leftWidth = 250; //Original: 200
    private static readonly float tabHeight = 32;
    private static float bannerHeight = 50;

    private static float tabMargin = 10f;

    private static readonly float mainRectLeftPct = 0.35f;

    //Sizes
    private static readonly Vector2 researchGroupSize = new(220, 20);
    private static readonly Vector2 researchOptionSize = new(200, 30);
    private static readonly Vector2 startButtonSize = new(120, 40);
    private static readonly Vector2 iconSize = new(20, 20);
    private static readonly float taskCurrentHeight = 50;

    private static float taskOtherHeight = 30;
    //private static Vector2 tabOptionSize = new Vector2(80, 20);

    //Colors
    private static readonly Color taskBG = new(0, 0, 0, 0.1f);
    private static readonly Color ColorWhite50P = new(1, 1, 1, 0.5f);
    private static readonly Color ColorWhite05P = new(1, 1, 1, 0.05f);
    private static readonly Color taskInfoBG = new ColorInt(33, 33, 33).ToColor;

    private static readonly Color TaskAvailable = new(1, 1, 1, 0.1f);
    private static readonly Color TaskInProgress = new(1, 1, 1, 0.5f);
    private static readonly Color TaskFinished = new(0, 0, 0, 0.75f);

    private static readonly string startProjLabel = "TR_StartResearch".Translate();

    private static readonly string stopProjLabel = "TR_StopResearch".Translate();

    private Vector2 projectScrollPos = Vector2.zero;
    private Vector2 taskScrollPos = Vector2.zero;

    public MainTabWindow_TibResearch()
    {
        //Calc Banner height
        var diff = TiberiumContent.Banner.width - (leftWidth - 2);
        var pct = 1 - diff / TiberiumContent.Banner.width;
        bannerHeight = TiberiumContent.Banner.height * pct;
    }

    protected override float Margin => 0f;

    public override Vector2 RequestedTabSize => new(1280f, 720f); //new Vector2(UI.screenWidth, UI.screenHeight * 0.6f);

    public TResearchManager Manager => Find.World.GetComponent<TResearchManager>();

    public static ResearchTabOption SelTab { get; set; } = ResearchTabOption.Projects;
    public static TResearchDef SelProject { get; set; }

    public TResearchTaskDef CurTask => SelProject.CurrentTask;

    public TResearchDef MainProject => Manager.currentProject;

    public override void PreOpen()
    {
        base.PreOpen();
    }

    public override void PostOpen()
    {
        base.PostOpen();
        //esearchRoots.AddRange(DefDatabase<TResearchDef>.AllDefs.Where(t => t.requisites?.tiberiumResearch.NullOrEmpty() ?? false));
    }

    //
    public override void DoWindowContents(Rect inRect)
    {
        //Draw BackGround Image Here

        var rect = inRect.ContractedBy(5f);
        GUI.BeginGroup(rect);

        var LeftRect = new Rect(0, 0, leftWidth, rect.height);
        var RightRect = new Rect(LeftRect.xMax, 0, rect.width - LeftRect.width, rect.height);

        DrawLeftPart(LeftRect.ContractedBy(10f));
        if (SelProject != null)
            DrawRightPart(RightRect.ContractedBy(10f));

        GUI.EndGroup();

        //TODO: REMOVE FROM
        if (!DebugSettings.godMode) return;
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.DrawHighlightIfMouseover(inRect);
        GUI.color = Color.red;
        Widgets.Label(inRect, inRect.size.ToString());
        Text.Anchor = default;
        GUI.color = Color.white;
        //TODO: REMOVE TO
    }

    private void DrawLeftPart(Rect rect)
    {
        //
        GUI.BeginGroup(rect);

        var tabRect = new Rect(0, tabHeight, rect.width, tabHeight);
        var menuRect = new Rect(0, tabHeight, rect.width, rect.height - tabHeight);
        Widgets.DrawMenuSection(menuRect);

        //Draw Tabs
        var tabs = new List<TabRecord>();
        tabs.Add(new TabRecord("TR_MainTabResearch".Translate(), delegate { SelTab = ResearchTabOption.Projects; },
            SelTab == ResearchTabOption.Projects));
        tabs.Add(new TabRecord("TR_MainTabEvents".Translate(), delegate { SelTab = ResearchTabOption.Events; },
            SelTab == ResearchTabOption.Events));
        TabDrawer.DrawTabs(tabRect, tabs);

        switch (SelTab)
        {
            case ResearchTabOption.Projects:
                DrawProjects(menuRect.ContractedBy(5f));
                break;
            case ResearchTabOption.Events:
                DrawEvents(menuRect.ContractedBy(5f));
                break;
        }

        GUI.EndGroup();

        //TODO: REMOVE FROM
        if (!DebugSettings.godMode) return;
        Text.Anchor = TextAnchor.LowerLeft;
        Widgets.DrawHighlightIfMouseover(rect);
        GUI.color = Color.red;
        Widgets.Label(rect, rect.size.ToString());
        Text.Anchor = default;
        GUI.color = Color.white;
        //TODO: REMOVE TO
    }

    /*
    private void DrawTabsWindow(Rect rect)
    {
        Find.WindowStack.ImmediateWindow(873459, rect, WindowLayer.GameUI, delegate
        {
            DrawRightPart(rect.ContractedBy(10f));
        }, true, true, 1f);
    }
    */

    private void DrawProjects(Rect rect)
    {
        var bannerRect = new Rect(rect.x - 4, rect.y - 5, rect.width + 8, bannerHeight);
        Widgets.DrawTextureFitted(bannerRect, TiberiumContent.Banner, 1f);

        GUI.BeginGroup(rect);
        var outRect = new Rect(0, 0, rect.width, rect.height - bannerHeight);
        var viewRect = new Rect(0, 0, outRect.width, outRect.height);
        Widgets.BeginScrollView(outRect, ref projectScrollPos, viewRect);
        var curY = bannerRect.height + 5; //new Vector2(rect.width, 0); //Width and yPos
        foreach (var researchGroup in TRUtils.ResearchManager().Groups)
            if (ShouldShow(researchGroup))
                DrawResearchGroup(ref curY, researchGroup);
        Widgets.EndScrollView();
        GUI.EndGroup();

        //TODO: REMOVE FROM
        if (!DebugSettings.godMode) return;
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.DrawHighlightIfMouseover(rect);
        GUI.color = Color.red;
        Widgets.Label(rect, rect.size.ToString());
        Text.Anchor = default;
        GUI.color = Color.white;
        //TODO: REMOVE TO
    }

    private bool ShouldShow(TResearchGroupDef group)
    {
        return (!group.IsFinished && !group.ActiveProjects.NullOrEmpty()) ||
               (group.IsFinished && !TResearchManager.hideGroups);
    }

    private void DrawResearchGroup(ref float curY, TResearchGroupDef group)
    {
        if (group.ActiveProjects.NullOrEmpty())
            return;
        var height = group.ActiveProjects.Count() * researchOptionSize.y;
        var textHeight = Text.CalcHeight(group.LabelCap, researchGroupSize.x);
        var groupOptionRect = new Rect(0, curY, researchGroupSize.x, researchGroupSize.y + textHeight);
        Widgets.DrawMenuSection(groupOptionRect);

        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(groupOptionRect, group.LabelCap);

        curY += groupOptionRect.height;
        if (Widgets.ButtonInvisible(groupOptionRect)) Manager.OpenClose(group);
        if (Manager.IsOpen(group))
        {
            var xOff = (researchGroupSize.x - researchOptionSize.x) / 2;
            var groupOptionSelection = new Rect(xOff, curY, researchOptionSize.x, height);
            Widgets.DrawMenuSection(groupOptionSelection);
            foreach (var project in group.ActiveProjects)
            {
                var ProjectOptionRect = new Rect(xOff, curY, researchOptionSize.x, researchOptionSize.y);
                var IconRect = ProjectOptionRect.LeftPartPixels(iconSize.x);
                var TextRect = ProjectOptionRect.RightPartPixels(ProjectOptionRect.width - 30);

                Widgets.DrawTextureFitted(IconRect, ProjectStatusTexture(project.State), 1);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(TextRect, project.LabelCap);

                if (Mouse.IsOver(ProjectOptionRect) || project == SelProject)
                    Widgets.DrawHighlight(ProjectOptionRect);

                if (Widgets.ButtonInvisible(ProjectOptionRect))
                    SelProject = project;

                curY += ProjectOptionRect.height;
            }
        }

        curY += 5f;
        Text.Anchor = default;
        //curY += height;
    }

    private void DrawEvents(Rect rect)
    {
        GUI.BeginGroup(rect);
        var outRect = new Rect(0, 0, rect.width, rect.height - bannerHeight);
        var viewRect = new Rect(0, 0, outRect.width, outRect.height);
        Widgets.BeginScrollView(outRect, ref projectScrollPos, viewRect);
        float curY = 0; //new Vector2(rect.width, 0); //Width and yPos
        foreach (var TRevent in TRUtils.EventManager().allEvents)
        {
            DrawEvent(TRevent, new Rect(0, curY, researchGroupSize.x, researchGroupSize.y));
            curY += researchGroupSize.y;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();
    }

    public void DrawEvent(BaseEvent baseEvent, Rect rect)
    {
        //BaseEvent activeEvent = TRUtils.EventManager().activeEvents.First(e => e != null && e.def == def);
        Widgets.DrawMenuSection(rect);
        Widgets.Label(rect, baseEvent.def.LabelCap + " " + baseEvent.TimeReadOut + " " + baseEvent.def.IsFinished);
    }

    private Texture2D ProjectStatusTexture(ResearchState state)
    {
        switch (state)
        {
            case ResearchState.Finished:
                return Widgets.CheckboxOnTex;
            case ResearchState.InProgress:
                return TiberiumContent.Research_Active;
            case ResearchState.Available:
                return TiberiumContent.Research_Available;
            default:
                return BaseContent.BadTex;
        }
    }

    // Desc / Image / Steps-Tasks
    private void DrawRightPart(Rect rect)
    {
        var menuRect = new Rect(rect.x, rect.y + tabHeight, rect.width, rect.height - tabHeight);
        //TODO: remove back
        var menuBack = new Rect(rect.x, rect.y + tabHeight, rect.width, rect.height - tabHeight);
        Widgets.DrawMenuSection(menuRect);

        menuRect = menuRect.ContractedBy(5f);
        GUI.BeginGroup(menuRect);
        menuRect = new Rect(0, 0, menuRect.width, menuRect.height);

        var LeftPart = menuRect.LeftPart(mainRectLeftPct);
        var RightPart =
            menuRect.RightPart(1f -
                               mainRectLeftPct); //(new Rect(LeftThird.width, 0, menuRect.width - LeftThird.width, menuRect.height));

        //LeftPart
        //Desc

        var TopHalfRect = LeftPart.TopHalf().ContractedBy(5);
        var BottomHalfRect = LeftPart.BottomHalf().ContractedBy(5);

        //Title
        Text.Font = GameFont.Medium;
        var mainTitleHeight = Text.CalcHeight(SelProject.LabelCap, LeftPart.width);
        var TitleRect = new Rect(0, 0, TopHalfRect.width, mainTitleHeight);
        Widgets.Label(TitleRect, SelProject.LabelCap);
        Text.Font = GameFont.Tiny;
        var subTitleHeight = Text.CalcHeight(SelProject.researchType, LeftPart.width);
        var SubTitleRect = new Rect(0, mainTitleHeight, TopHalfRect.width, subTitleHeight);
        Widgets.Label(SubTitleRect, SelProject.researchType);
        Text.Font = GameFont.Small;
        var fullTitleHeight = mainTitleHeight + subTitleHeight;

        var DescRect = new Rect(0, fullTitleHeight, TopHalfRect.width,
            TopHalfRect.height - fullTitleHeight - startButtonSize.y);
        var StartButtonRect = new Rect(TopHalfRect.xMax - (startButtonSize.x + 10), DescRect.yMax, startButtonSize.x,
            startButtonSize.y);


        Widgets.TextArea(DescRect, SelProject.description, true);

        if (SelProject.IsFinished)
        {
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.DrawHighlight(StartButtonRect);
            Widgets.Label(StartButtonRect.ContractedBy(5f), "Finished".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
        }
        else
        {
            var sameFlag = SelProject.Equals(MainProject);
            if (Widgets.ButtonText(StartButtonRect, sameFlag ? stopProjLabel : startProjLabel))
            {
                if (!sameFlag)
                    Messages.Message("TR_StartedProject".Translate(SelProject.LabelCap),
                        MessageTypeDefOf.NeutralEvent, false);
                Manager.StartResearch(SelProject);
            }
        }

        //
        DrawTaskInfo(BottomHalfRect);

        //RightPart
        //Image 
        var ImageRect = RightPart.TopHalf().ContractedBy(5f);
        DrawImage(ImageRect);

        //Tasks
        var TaskRect = RightPart.BottomHalf().ContractedBy(5f);
        if (SelProject != null && !SelProject.tasks.NullOrEmpty())
            DrawTasks(TaskRect);
        GUI.EndGroup();

        //TODO: REMOVE FROM
        if (!DebugSettings.godMode) return;
        Text.Anchor = TextAnchor.UpperCenter;
        Widgets.DrawHighlightIfMouseover(menuBack);
        GUI.color = Color.red;
        Widgets.Label(menuBack, menuBack.size.ToString());
        Text.Anchor = default;
        GUI.color = Color.white;
        //TODO: REMOVE TO
    }

    private void AddGapLine(Rect rect, float gapSize, out float newY)
    {
        GUI.color = TRMats.GapLineColor;
        Widgets.DrawLineHorizontal(rect.x, rect.y + gapSize / 2, rect.width);
        newY = rect.y + gapSize;
        GUI.color = Color.white;
    }

    private void DrawImage(Rect rect)
    {
        Widgets.DrawShadowAround(rect);
        Text.Anchor = TextAnchor.MiddleCenter;
        Text.Font = GameFont.Medium;
        Widgets.Label(rect, rect.height + " x " + rect.width);
        Text.Font = GameFont.Small;
        Text.Anchor = default;
    }

    private void DrawTaskInfo(Rect rect)
    {
        if (CurTask == null) return;
        Widgets.DrawMenuSection(rect);

        GUI.BeginGroup(rect.ContractedBy(5f));

        var newRect = rect.AtZero();
        string taskInfoTitle = "TR_CurTask".Translate(CurTask.LabelCap);
        var titleSize = Text.CalcSize(taskInfoTitle);
        var TitlePart = newRect.TopPartPixels(titleSize.y);
        Widgets.Label(TitlePart, taskInfoTitle);
        newRect = newRect.BottomPartPixels(newRect.height - titleSize.y);

        Widgets.DrawBoxSolid(newRect, taskInfoBG);
        AddGapLine(newRect, 0, out var newY);

        //TaskInfo
        var taskInfoStringHeight = Text.CalcHeight(CurTask.TaskInfo, newRect.width);
        var targetStringRect = new Rect(newRect.x, newY + 5, newRect.width, taskInfoStringHeight);
        Widgets.TextArea(targetStringRect, CurTask.TaskInfo, true);

        GUI.EndGroup();

        //TODO: REMOVE FROM
        if (!DebugSettings.godMode) return;
        Text.Anchor = TextAnchor.LowerLeft;
        Widgets.DrawHighlightIfMouseover(rect);
        GUI.color = Color.red;
        Widgets.Label(rect, rect.size.ToString());
        Text.Anchor = default;
        GUI.color = Color.white;
        //TODO: REMOVE TO
    }

    private void DrawTasks(Rect rect)
    {
        GUI.BeginGroup(rect);
        var outRect = rect.AtZero();
        DrawUtils.DrawColoredBox(outRect, taskBG, ColorWhite50P, 1);
        var viewRect = new Rect(0, 0, outRect.width, taskCurrentHeight * SelProject.tasks.Count);
        Widgets.BeginScrollView(outRect, ref taskScrollPos, viewRect, false);

        var curY = 0f;
        for (var i = 0; i < SelProject.tasks.Count; i++)
        {
            var task = SelProject.tasks[i];
            var taskRect = new Rect(0, curY, outRect.width, taskCurrentHeight).ContractedBy(2);
            DrawTask(taskRect, task, i, out var yHeight);
            curY += yHeight;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();

        //TODO: REMOVE FROM
        if (!DebugSettings.godMode) return;
        Text.Anchor = TextAnchor.LowerLeft;
        Widgets.DrawHighlightIfMouseover(rect);
        GUI.color = Color.red;
        Widgets.Label(rect, rect.size.ToString());
        Text.Anchor = default;
        GUI.color = Color.white;
        //TODO: REMOVE TO
    }

    //Draw Task - Design depends on status: current | to do | finished
    private void DrawTask(Rect rect, TResearchTaskDef task, int index, out float yHeight)
    {
        if (task.IsFinished)
            Widgets.DrawBoxSolid(rect, new Color(0, 1, 0.2f, 0.15f));
        yHeight = rect.height;
        var labelSize = Text.CalcSize(task.LabelCap);
        var descSize = Text.CalcSize(task.descriptionShort);
        var IconRect = new Rect(rect.x + 4f, rect.y, iconSize.x, iconSize.y);
        var labelY = (iconSize.y - labelSize.y) / 2f;
        var LabelRect = new Rect(IconRect.xMax + 4f, rect.y + labelY, labelSize.x, labelSize.y);
        var DescriptionRect = new Rect(IconRect.xMax, rect.yMax - labelSize.y, descSize.x, descSize.y);
        var RightInfoPartRect = rect.RightPart(0.30f);
        var ProgressBarRect = RightInfoPartRect.RightHalf().TopPart(0.65f).ContractedBy(5f);

        var debugRect = RightInfoPartRect.LeftHalf().TopHalf();
        var finButton = new Rect(debugRect.x, debugRect.y, 20, 15);
        var resetButt = new Rect(debugRect.x + 20, debugRect.y, 20, 15);

        Widgets.DrawTextureFitted(IconRect, ProjectStatusTexture(task.State), 1);

        if (Widgets.ButtonText(finButton, "fin"))
        {
            if (task.creationTasks != null)
                foreach (var option in task.creationTasks.thingsToCreate)
                    TRUtils.ResearchManager().CreationTable.taskCreations[task].AddProgress(option, option.amount);

            Manager.SetProgress(task, task.ProgressToDo);
        }

        if (Widgets.ButtonText(resetButt, "rst"))
        {
            Manager.SetProgress(task, 0);
            Manager.SetCompleted(task, false);
            if (task.creationTasks != null)
                foreach (var option in task.creationTasks.thingsToCreate)
                    TRUtils.ResearchManager().CreationTable.taskCreations[task].AddProgress(option, -option.amount);
        }

        var state = task.State;
        if (state == ResearchState.InProgress)
        {
        }
        else if (state == ResearchState.Available)
        {
        }
        else if (state == ResearchState.Finished)
        {
        }

        if (task == CurTask)
            DrawUtils.DrawBox(rect, 0.5f, 1);

        Widgets.FillableBar(ProgressBarRect, task.ProgressPct, TRMats.blue, TRMats.black, true);
        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(ProgressBarRect, task.WorkLabel);
        Text.Anchor = default;

        Widgets.Label(LabelRect, task.LabelCap);
        GUI.color = ColorWhite50P;
        Widgets.Label(DescriptionRect, task.descriptionShort);
        GUI.color = Color.white;

        if (Mouse.IsOver(rect) && DebugSettings.godMode) DrawUtils.DrawBox(rect, 0.5f, 1);
    }

    private void ColorsFor(Rect rect, TResearchDef def, out Color bgColor, out Color borderColor, out Color textColor)
    {
        bgColor = TexUI.LockedResearchColor;
        borderColor = TexUI.DefaultBorderResearchColor;
        textColor = Widgets.NormalOptionColor;

        if (SelProject == def)
            bgColor = TexUI.ActiveResearchColor;
        else if (def.IsFinished)
            bgColor = TexUI.FinishedResearchColor;
        else if (def.CanStartNow) bgColor = TexUI.AvailResearchColor;

        if (!def.RequisitesComplete)
        {
            bgColor = TexUI.LockedResearchColor;
            textColor = Color.gray;
        }

        if (SelProject == def)
        {
            bgColor += TexUI.HighlightBgResearchColor;
            borderColor = TexUI.HighlightBorderResearchColor;
        }

        if (Mouse.IsOver(rect))
        {
        }
    }

    /*
    private float HeightFrom(TResearchDef def)
    {
        float val = selSize.y;
        int num = 0;
        for (num = 0; def.unlocks.Count > 0; num += def.unlocks.Count) { }
        if (num > 0)
            val += (num - 1) * selSize.y;
        foreach (var def2 in def.unlocks)
            val += HeightFrom(def2);
        return val;
    }

    private float WidthFrom(TResearchDef def)
    {
        float val = selSize.x;
        if (def.unlocks.Count > 0)
            val += selSize.x;
        foreach (var def2 in def.unlocks)
            val += WidthFrom(def2);
        return val;
    }
    */
}