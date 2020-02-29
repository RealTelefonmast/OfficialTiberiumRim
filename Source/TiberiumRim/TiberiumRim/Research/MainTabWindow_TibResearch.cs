using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;

namespace TiberiumRim
{

    public enum ResearchTabOption
    {
        Projects,
        Events
    }

    public class MainTabWindow_TibResearch : MainTabWindow
    {
        //

        //Dimensions
        private static float leftWidth = 250; //Original: 200
        private static float tabHeight = 32;
        private static float bannerHeight = 50;

        private static float tabMargin = 10f;
        private static float mainRectLeftPct = 0.35f;

        private Vector2 projectScrollPos = Vector2.zero;
        private Vector2 taskScrollPos = Vector2.zero;
        //Sizes
        private static Vector2 researchGroupSize = new Vector2(220, 20);
        private static Vector2 researchOptionSize = new Vector2(200, 30);
        private static Vector2 startButtonSize = new Vector2(120, 40);
        private static Vector2 iconSize = new Vector2(20, 20);
        private static float taskCurrentHeight = 50;
        private static float taskOtherHeight = 30;
        //private static Vector2 tabOptionSize = new Vector2(80, 20);

        //Colors
        private static readonly Color taskBG = new Color(0, 0, 0, 0.1f);
        private static readonly Color ColorWhite50P = new Color(1, 1, 1, 0.5f);
        private static readonly Color ColorWhite05P = new Color(1, 1, 1, 0.05f);
        private static readonly Color taskInfoBG = new ColorInt(33, 33, 33).ToColor;

        private static readonly Color TaskAvailable = new Color(1, 1, 1, 0.1f);
        private static readonly Color TaskInProgress = new Color(1, 1, 1, 0.5f);
        private static readonly Color TaskFinished = new Color(0, 0, 0, 0.75f);

        private static string startProjLabel = "TR_StartResearch".Translate(),
                               stopProjLabel = "TR_StopResearch".Translate();

        public MainTabWindow_TibResearch()
        {
            //Calc Banner height
            float diff = TiberiumContent.Banner.width - (leftWidth - 2);
            float pct = 1 - (diff / TiberiumContent.Banner.width);
            bannerHeight = TiberiumContent.Banner.height * pct;
        }

        public override void PreOpen()
        {
            base.PreOpen();
        }

        public override void PostOpen()
        {
            base.PostOpen();
            //esearchRoots.AddRange(DefDatabase<TResearchDef>.AllDefs.Where(t => t.requisites?.tiberiumResearch.NullOrEmpty() ?? false));
        }

        protected override float Margin => 5f;

        public override Vector2 RequestedTabSize => new Vector2(1281f, 721f); //new Vector2(UI.screenWidth, UI.screenHeight * 0.6f);

        public TResearchManager Manager => Find.World.GetComponent<TResearchManager>();

        public static ResearchTabOption SelTab { get; set; } = ResearchTabOption.Projects;
        public static TResearchDef SelProject { get; set; }

        public TResearchTaskDef CurTask => SelProject.CurrentTask;

        public TResearchDef MainProject
        {
            get => Manager.currentProject;
        }

        //
        public override void DoWindowContents(Rect inRect)
        {
            Rect LeftRect = new Rect(0, 0, leftWidth, inRect.height);
            Rect RightRect = new Rect(LeftRect.xMax, 0, inRect.width - LeftRect.width, inRect.height);

            DrawLeftPart(LeftRect.ContractedBy(10f));
            if (SelProject != null)
                DrawRightPart(RightRect.ContractedBy(10f));
        }

        private void DrawLeftPart(Rect rect)
        {
            //
            GUI.BeginGroup(rect);

            Rect tabRect = new Rect(0, tabHeight, rect.width, tabHeight);
            Rect menuRect = new Rect(0, tabHeight, rect.width, rect.height - tabHeight);
            Widgets.DrawMenuSection(menuRect);

            //Draw Tabs
            var tabs = new List<TabRecord>();
            tabs.Add(new TabRecord("TR_MainTabResearch".Translate(), delegate { SelTab = ResearchTabOption.Projects; }, SelTab == ResearchTabOption.Projects));
            tabs.Add(new TabRecord("TR_MainTabEvents".Translate(), delegate { SelTab = ResearchTabOption.Events; }, SelTab == ResearchTabOption.Events));
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
            Rect bannerRect = new Rect(rect.x - 4, rect.y - 5, rect.width + 8, bannerHeight);
            Widgets.DrawTextureFitted(bannerRect, TiberiumContent.Banner, 1f);

            GUI.BeginGroup(rect);
            Rect outRect = new Rect(0, 0, rect.width, rect.height - bannerHeight);
            Rect viewRect = new Rect(0, 0, outRect.width, outRect.height);
            Widgets.BeginScrollView(outRect, ref projectScrollPos, viewRect, true);
            float curY = bannerRect.height + 5; //new Vector2(rect.width, 0); //Width and yPos
            foreach (var researchGroup in TRUtils.ResearchManager().Groups)
            {
                if (ShouldShow(researchGroup))
                    DrawResearchGroup(ref curY, researchGroup);
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private bool ShouldShow(TResearchGroupDef group)
        {
            return !group.IsFinished && !group.ActiveProjects.NullOrEmpty() || (group.IsFinished && !TResearchManager.hideGroups);
        }

        private void DrawResearchGroup(ref float curY, TResearchGroupDef group)
        {
            if (group.ActiveProjects.NullOrEmpty())
                return;
            float height = group.ActiveProjects.Count() * researchOptionSize.y;
            float textHeight = Text.CalcHeight(group.LabelCap, researchGroupSize.x);
            Rect groupOptionRect = new Rect(0, curY, researchGroupSize.x, researchGroupSize.y + textHeight);
            Widgets.DrawMenuSection(groupOptionRect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(groupOptionRect, group.LabelCap);

            curY += groupOptionRect.height;
            if (Widgets.ButtonInvisible(groupOptionRect))
            {
                Manager.OpenClose(group);
            }
            if (Manager.IsOpen(group))
            {
                float xOff = (researchGroupSize.x - researchOptionSize.x) / 2;
                Rect groupOptionSelection = new Rect(xOff, curY, researchOptionSize.x, height);
                Widgets.DrawMenuSection(groupOptionSelection);
                foreach (var project in group.ActiveProjects)
                {
                    Rect ProjectOptionRect = new Rect(xOff, curY, researchOptionSize.x, researchOptionSize.y);
                    Rect IconRect = ProjectOptionRect.LeftPartPixels(iconSize.x);
                    Rect TextRect = ProjectOptionRect.RightPartPixels(ProjectOptionRect.width - 30);

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
            Rect outRect = new Rect(0, 0, rect.width, rect.height - bannerHeight);
            Rect viewRect = new Rect(0, 0, outRect.width, outRect.height);
            Widgets.BeginScrollView(outRect, ref projectScrollPos, viewRect, true);
            float curY = 0; //new Vector2(rect.width, 0); //Width and yPos
            foreach (var TRevent in TRUtils.EventManager().allEvents)
            {
                DrawEvent(TRevent, new Rect(0, curY, researchGroupSize.x, researchGroupSize.y ));
                curY += researchGroupSize.y;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        public void DrawEvent(BaseEvent baseEvent, Rect rect)
        {
            //BaseEvent activeEvent = TRUtils.EventManager().activeEvents.First(e => e != null && e.def == def);
            Widgets.DrawMenuSection(rect);
            Widgets.Label(rect, baseEvent.def.LabelCap + " " + baseEvent.TimeReadOut);
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
            Rect menuRect = new Rect(rect.x, rect.y + tabHeight, rect.width, rect.height - tabHeight);
            Widgets.DrawMenuSection(menuRect);

            menuRect = menuRect.ContractedBy(5f);
            GUI.BeginGroup(menuRect);
            menuRect = new Rect(0, 0, menuRect.width, menuRect.height);

            Rect LeftPart = menuRect.LeftPart(mainRectLeftPct);
            Rect RightPart = menuRect.RightPart(1f - mainRectLeftPct);//(new Rect(LeftThird.width, 0, menuRect.width - LeftThird.width, menuRect.height));

            //LeftPart
            //Desc

            Rect TopHalfRect = LeftPart.TopHalf().ContractedBy(5);
            Rect BottomHalfRect = LeftPart.BottomHalf().ContractedBy(5);

            //Title
            Text.Font = GameFont.Medium;
            float mainTitleHeight = Text.CalcHeight(SelProject.LabelCap, LeftPart.width);
            Rect TitleRect = new Rect(0, 0, TopHalfRect.width, mainTitleHeight);
            Widgets.Label(TitleRect, SelProject.LabelCap);
            Text.Font = GameFont.Tiny;
            float subTitleHeight = Text.CalcHeight(SelProject.researchType, LeftPart.width);
            Rect SubTitleRect = new Rect(0, mainTitleHeight, TopHalfRect.width, subTitleHeight);
            Widgets.Label(SubTitleRect, SelProject.researchType);
            Text.Font = GameFont.Small;
            float fullTitleHeight = mainTitleHeight + subTitleHeight;

            Rect DescRect = new Rect(0, fullTitleHeight, TopHalfRect.width, TopHalfRect.height - fullTitleHeight - startButtonSize.y);
            Rect StartButtonRect = new Rect(TopHalfRect.xMax - (startButtonSize.x + 10), DescRect.yMax, startButtonSize.x, startButtonSize.y);


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
                bool sameFlag = SelProject.Equals(MainProject);
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
            Rect ImageRect = RightPart.TopHalf().ContractedBy(5f);
            DrawImage(ImageRect);

            //Tasks
            Rect TaskRect = RightPart.BottomHalf().ContractedBy(5f);
            if (SelProject != null && !SelProject.tasks.NullOrEmpty())
                DrawTasks(TaskRect);

            GUI.EndGroup();
        }

        private void AddGapLine(Rect rect, float gapSize, out float newY)
        {
            GUI.color = TRMats.GapLineColor;
            Widgets.DrawLineHorizontal(rect.x, rect.y + (gapSize / 2), rect.width);
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

            rect = rect.ContractedBy(5);
            string taskInfoTitle = "TR_CurTask".Translate(CurTask.LabelCap);
            Vector2 titleSize = Text.CalcSize(taskInfoTitle);
            Rect TitlePart = rect.TopPartPixels(titleSize.y);
            Widgets.Label(TitlePart, taskInfoTitle);
            rect = rect.BottomPartPixels(rect.height - titleSize.y);

            Widgets.DrawBoxSolid(rect, taskInfoBG);
            AddGapLine(rect, 0, out float newY);

            //TaskInfo
            float taskInfoStringHeight = Text.CalcHeight(CurTask.TaskInfo, rect.width);
            Rect targetStringRect = new Rect(rect.x, newY + 5, rect.width, taskInfoStringHeight);
            Widgets.TextArea(targetStringRect, CurTask.TaskInfo, true);
        }

        private void DrawTasks(Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect outRect = rect.AtZero();
            DrawUtils.DrawColoredBox(outRect, taskBG, ColorWhite50P, 1);
            Rect viewRect = new Rect(0, 0, outRect.width, taskCurrentHeight * SelProject.tasks.Count);
            Widgets.BeginScrollView(outRect, ref taskScrollPos, viewRect, true);

            float curY = 0f;
            for (var i = 0; i < SelProject.tasks.Count; i++)
            {
                TResearchTaskDef task = SelProject.tasks[i];
                Rect taskRect = new Rect(0, curY, outRect.width, taskCurrentHeight).ContractedBy(2);
                DrawTask(taskRect, task, i, out float yHeight);
                curY += yHeight;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        //Draw Task - Design depends on status: current | to do | finished
        private void DrawTask(Rect rect, TResearchTaskDef task, int index, out float yHeight)
        {
            if (task.IsFinished)
                Widgets.DrawBoxSolid(rect, new Color(0, 1, 0.2f, 0.15f));
            yHeight = rect.height;
            Vector2 labelSize = Text.CalcSize(task.LabelCap);
            Vector2 descSize = Text.CalcSize(task.descriptionShort);
            Rect IconRect = new Rect(rect.x + 4f, rect.y, iconSize.x, iconSize.y);
            float labelY = (iconSize.y - labelSize.y) / 2f;
            Rect LabelRect = new Rect(IconRect.xMax + 4f, rect.y + labelY, labelSize.x, labelSize.y);
            Rect DescriptionRect = new Rect(IconRect.xMax, rect.yMax - labelSize.y, descSize.x, descSize.y);
            Rect RightInfoPartRect = rect.RightPart(0.30f);
            Rect ProgressBarRect = RightInfoPartRect.RightHalf().TopPart(0.65f).ContractedBy(5f);

            Rect debugRect = RightInfoPartRect.LeftHalf().TopHalf();
            Rect finButton = new Rect(debugRect.x, debugRect.y, 20, 15);
            Rect resetButt = new Rect(debugRect.x + 20, debugRect.y, 20, 15);

            Widgets.DrawTextureFitted(IconRect, ProjectStatusTexture(task.State), 1);

            if (Widgets.ButtonText(finButton, "fin"))
            {
                if (task.creationTasks != null)
                {
                    foreach (var option in task.creationTasks.thingsToCreate)
                    {
                        ResearchCreationTable.taskCreations[task].AddProgress(option, option.amount);
                    }
                }
                Manager.SetProgress(task, task.ProgressToDo);
            }
            if (Widgets.ButtonText(resetButt, "rst"))
            {
                Manager.SetProgress(task, 0);
                Manager.SetCompleted(task, false);
                if (task.creationTasks != null)
                {
                    foreach (var option in task.creationTasks.thingsToCreate)
                    {
                        ResearchCreationTable.taskCreations[task].AddProgress(option, -option.amount);
                    }
                }
            }

            ResearchState state = task.State;
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

            if (Mouse.IsOver(rect) && DebugSettings.godMode)
            {
                DrawUtils.DrawBox(rect, 0.5f, 1);
            }

        }

        private void ColorsFor(Rect rect, TResearchDef def, out Color bgColor, out Color borderColor, out Color textColor)
        {
            bgColor = TexUI.LockedResearchColor;
            borderColor = TexUI.DefaultBorderResearchColor;
            textColor = Widgets.NormalOptionColor;

            if (SelProject == def)
            {
                bgColor = TexUI.ActiveResearchColor;
            }
            else if (def.IsFinished)
            {
                bgColor = TexUI.FinishedResearchColor;
            }
            else if (def.CanStartNow)
            {
                bgColor = TexUI.AvailResearchColor;
            }

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
}
