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
        Events,
        Discoveries
    }

    public class MainTabWindow_TibResearch : MainTabWindow
    {
        private TResearchDef researchInt;

        //Dimensions
        private static float leftWidth = 250; //Original: 200
        private static float tabHeight = 32;
        private static float bannerHeight = 50;

        private static float tabMargin = 10f;
        private static float mainRectLeftPct = 0.40f;

        private Vector2 projectScrollPos = Vector2.zero;
        private Vector2 taskScrollPos = Vector2.zero;
        //Sizes
        //Research Group
        private static readonly Vector2 researchGroupSize = new Vector2(220, 30);

        //Research Option
        private static readonly Vector2 researchOptionSize = new Vector2(200, 30);
        private static readonly float researchOptionXOffset = (researchGroupSize.x - researchOptionSize.x) / 2;
        private static readonly float researchOptionIconSize = 24;

        private static readonly Vector2 startButtonSize = new Vector2(120, 40);
        private static readonly Vector2 iconSize = new Vector2(20, 20);
        private static readonly float taskCurrentHeight = 50;
        private static readonly float taskOtherHeight = 30;
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

        //Images
        private int curImage = 0;
        private Dictionary<TResearchTaskDef, List<Texture2D>> cachedImages = new Dictionary<TResearchTaskDef, List<Texture2D>>();

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

        protected override float Margin => 0f;

        public override Vector2 RequestedTabSize => new Vector2(1280f, 720f); //new Vector2(UI.screenWidth, UI.screenHeight * 0.6f);

        public bool HasUnseenProjects => Manager.Groups.Where(g => g.IsVisible).Any(g => g.HasUnseenProjects);

        public TResearchManager Manager => Find.World.GetComponent<TResearchManager>();

        public ResearchTabOption SelTab { get; set; } = ResearchTabOption.Projects;

        public TResearchDef SelProject
        {
            get => researchInt;
            set
            {
                SetDiaShowFor(value);
                researchInt = value;
            }
        }

        public TResearchTaskDef CurTask
        {
            get => Manager.TaskOverride ?? SelProject.CurrentTask;
            set => Manager.TaskOverride = value;
        }

        public TResearchDef MainProject => Manager.currentProject;

        private void SetDiaShowFor(TResearchDef def)
        {
            curImage = 0;
            cachedImages.Clear();
            foreach (var task in def.tasks)
            {
                if(task.images == null) continue;
                var textures = task.images.Select(i => ContentFinder<Texture2D>.Get(i, false)).ToList();
                cachedImages.Add(task, textures);
            }
        }

        //
        public override void DoWindowContents(Rect inRect)
        {
            //Draw BackGround Image Here

            Rect rect = inRect.ContractedBy(5f);
            GUI.BeginGroup(rect);

            Rect LeftRect = new Rect(0, 0, leftWidth, rect.height);
            Rect RightRect = new Rect(LeftRect.xMax, 0, rect.width - LeftRect.width, rect.height);

            DrawLeftPart(LeftRect.ContractedBy(10f));
            if (SelProject != null)
                DrawRightPart(RightRect.ContractedBy(10f));

            GUI.EndGroup();

            //TODO: REMOVE FROM
            // if (!DebugSettings.godMode) return;
            // Text.Anchor = TextAnchor.UpperLeft;
            // Widgets.DrawHighlightIfMouseover(inRect);
            // GUI.color = Color.red;
            // Widgets.Label(inRect, inRect.size.ToString());
            // Text.Anchor = default;
            // GUI.color = Color.white;
            //TODO: REMOVE TO
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
                case ResearchTabOption.Discoveries:
                    DrawDiscoveries(menuRect.ContractedBy(5f));
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
            var outRect  = new Rect(0, 0, rect.width, rect.height - bannerHeight);
            var viewRect = new Rect(0, 0, outRect.width, outRect.height);
            var curY= bannerRect.height + 5; 
            Widgets.BeginScrollView(outRect, ref projectScrollPos, viewRect, true);
            foreach (var researchGroup in Manager.Groups)
            {
                if(researchGroup.IsVisible)
                    DrawResearchGroup(ref curY, researchGroup);
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void DrawResearchGroup(ref float curY, TResearchGroupDef group)
        {
            if (group.ActiveProjects.NullOrEmpty()) return;

            var height     = group.ActiveProjects.Count() * researchOptionSize.y;
            var textHeight = Text.CalcHeight(group.LabelCap, researchGroupSize.x);
            var groupOptionRect = new Rect(0, curY, researchGroupSize.x, researchGroupSize.y + textHeight);
            curY += groupOptionRect.height;

            if (TRWidgets.ButtonColoredHighlight(groupOptionRect, group.LabelCap.RawText.Bold(), TRMats.MenuSectionBGFillColor, TRMats.MenuSectionBGBorderColor))
            {
                Manager.OpenClose(group);
            }

            if (group.HasUnseenProjects)
            {
                TRWidgets.DrawTextureInCorner(groupOptionRect, TiberiumContent.NewMarker, 50, TextAnchor.UpperRight, new Vector2(-1,1));
            }
            if (Manager.IsOpen(group))
            {
                var groupOptionSelection = new Rect(researchOptionXOffset, curY, researchOptionSize.x, height);
                Widgets.DrawMenuSection(groupOptionSelection);

                foreach (var project in group.ActiveProjects)
                {
                    float margin = (researchOptionSize.y - 24f) / 2;
                    WidgetRow row = new WidgetRow(researchOptionXOffset + margin, curY + margin, UIDirection.RightThenDown);
                    row.Icon(project.HasBeenSeen ? ProjectStatusTexture(project.State) : TiberiumContent.AttentionMarker);
                    row.Label(project.LabelCap);
                    
                    var projectOptionRect = new Rect(researchOptionXOffset, curY, researchOptionSize.x, researchOptionSize.y);

                    if (Mouse.IsOver(projectOptionRect) || project == SelProject)
                    {
                        TRUtils.DiscoveryTable().DiscoverResearch(project);
                        Widgets.DrawHighlight(projectOptionRect);
                    }

                    if (Widgets.ButtonInvisible(projectOptionRect))
                    {
                        SelProject = project;
                        CurTask = null;
                    }

                    curY += projectOptionRect.height;
                }
            }
            curY += + 5f;
            //Text.Anchor = default;
            //curY += height;
        }

        // EVENT MENU
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
            Widgets.Label(rect, baseEvent.def.LabelCap + " " + baseEvent.TimeReadOut + " " + baseEvent.def.IsFinished);
        }

        // DISCOVERY MENU // WIKI
        private void DrawDiscoveries(Rect rect)
        {

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
            Rect RightPart = menuRect.RightPart(1f - mainRectLeftPct);

            // ##++ LEFT PART ++##
            //Desc
            Rect TopQuarterRect = LeftPart.TopPart(0.40f).ContractedBy(5);
            Rect BottomRestRect = LeftPart.BottomPart(0.60f).ContractedBy(5);

            //Title
            Text.Font = GameFont.Medium;
            float mainTitleHeight = Text.CalcHeight(SelProject.LabelCap, LeftPart.width);
            Rect TitleRect = new Rect(0, 0, TopQuarterRect.width, mainTitleHeight);
            Widgets.Label(TitleRect, SelProject.LabelCap);
            Text.Font = GameFont.Tiny;
            float subTitleHeight = Text.CalcHeight(SelProject.researchType, LeftPart.width);
            Rect SubTitleRect = new Rect(0, mainTitleHeight, TopQuarterRect.width, subTitleHeight);
            Widgets.Label(SubTitleRect, SelProject.researchType);
            Text.Font = GameFont.Small;
            float fullTitleHeight = mainTitleHeight + subTitleHeight;

            Rect DescRect = new Rect(0, fullTitleHeight, TopQuarterRect.width,
                TopQuarterRect.height - fullTitleHeight - startButtonSize.y);
            Rect StartButtonRect = new Rect(TopQuarterRect.xMax - (startButtonSize.x + 10), DescRect.yMax,
                startButtonSize.x, startButtonSize.y);

            Widgets.TextArea(DescRect, SelProject.description, true);

            //Debug
            if (DebugSettings.godMode)
            {
                Rect debug_Res = new Rect(StartButtonRect.x - 20, StartButtonRect.y, 20, 20);
                Rect debug_Fin = new Rect(debug_Res.x - 20, debug_Res.y, 20, 20);
                if (Widgets.ButtonText(debug_Fin, "fin"))
                    SelProject.tasks.ForEach(t => t.Debug_Finish());
                if (Widgets.ButtonText(debug_Res, "rst"))
                    SelProject.tasks.ForEach(t => t.Debug_Reset());
            }


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
            DrawTaskInfo(BottomRestRect);


            // ##++ RIGHT PART ++##
            //Image 
            float imageHeight = 280;
            Rect ImageRect = RightPart.TopPartPixels(imageHeight).ContractedBy(5f);
            if (CurTask != null && cachedImages.ContainsKey(CurTask))
            {
                DrawImage(ImageRect, CurTask);
            }

            //Tasks
            Rect TaskRect = RightPart.BottomPartPixels(RightPart.height - imageHeight).ContractedBy(5f);
            if (SelProject != null && !SelProject.tasks.NullOrEmpty())
            {
                DrawTasks(TaskRect);
            }

            GUI.EndGroup();
        }

        private void DrawImage(Rect rect, TResearchTaskDef task)
        {
            Vector2 buttonSize = new Vector2(45f, rect.height);
            Rect ImageButtonLeft = new Rect(new Vector2(rect.x, rect.y), buttonSize).ContractedBy(5f);
            Rect ImageButtonRight = new Rect(new Vector2(rect.xMax - 45f, rect.y), buttonSize).ContractedBy(5f);

            Widgets.DrawShadowAround(rect);
            Widgets.DrawTextureFitted(rect, cachedImages[task][curImage], 1);

            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            if (Mouse.IsOver(rect) && cachedImages.TryGetValue(CurTask, out List<Texture2D> texts))
            {
                string imageCount = (curImage + 1) + "/" + texts.Count;
                Vector2 size = Text.CalcSize(imageCount);
                Rect imageCountRect = new Rect(new Vector2(0f, 0f), size);
                imageCountRect.center = new Vector2(rect.center.x, rect.yMax - (size.y * 0.5f));
                Widgets.Label(imageCountRect, imageCount);
                if (curImage > 0)
                {
                    UI.RotateAroundPivot(180f, ImageButtonLeft.center);
                    Widgets.DrawTextureFitted(ImageButtonLeft, TiberiumContent.SideBarArrow, 1f);
                    UI.RotateAroundPivot(180f, ImageButtonLeft.center);
                    if (Widgets.ButtonInvisible(ImageButtonLeft, true))
                    {
                        SoundDefOf.TabClose.PlayOneShotOnCamera(null);
                        curImage -= 1;
                    }
                }
                if (curImage < texts.Count - 1)
                {
                    Widgets.DrawTextureFitted(ImageButtonRight, TiberiumContent.SideBarArrow, 1f);
                    if (Widgets.ButtonInvisible(ImageButtonRight, true))
                    {
                        SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
                        curImage += 1;
                    }
                }
            }
            GUI.color = Color.white;
        }

        private void DrawTaskInfo(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            Widgets.DrawBoxSolid(rect.ContractedBy(1), taskInfoBG);
            if (CurTask == null)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "TR_NoTaskAvailable".Translate());
                Text.Anchor = default;
                return;
            }

            rect = rect.ContractedBy(5f);
            GUI.BeginGroup(rect);
            Rect TaskInfoRect = rect.AtZero();
            float curY = TaskInfoRect.y;

            // Do Title
            string title = "TR_CurTask".Translate(CurTask.LabelCap);
            Rect TitleRect = new Rect(new Vector2(0, curY), Text.CalcSize(title));
            Widgets.Label(TitleRect, title);
            curY += TitleRect.height;

            TRWidgets.AddGapLine(ref curY, TaskInfoRect.width, 4, 0, TextAnchor.UpperCenter);

            // Do Desc
            if (!CurTask.description.NullOrEmpty())
            {
                float descriptionHeight = Text.CalcHeight(CurTask.description, TaskInfoRect.width);
                Rect DescRect = new Rect(0, curY, TaskInfoRect.width, descriptionHeight);
                curY += descriptionHeight;
                Widgets.Label(DescRect, CurTask.description);
                TRWidgets.AddGapLine(ref curY, TaskInfoRect.width, 6);
            }

            // Do Info
            //float TaskInfoHeight = Text.CalcHeight(CurTask.TaskInfo, TaskInfoRect.width);
            //Rect TaskInfoTextRect = new Rect(0, curY, TaskInfoRect.width, TaskInfoHeight);
            CurTask.WriteTaskInfo(new Rect(0, curY, TaskInfoRect.width, 500), out float infoHeight);
            curY += infoHeight;
            //Widgets.Label(TaskInfoTextRect, CurTask.TaskInfo);

            TRWidgets.AddGapLine(ref curY, TaskInfoRect.width, 6);

            // Do Outcomes
            if (CurTask.UnlocksThings.Any())
            {
                var label = "TR_ResearchUnlocks".Translate();
                var labelSize = Text.CalcSize(label);
                Widgets.Label(new Rect(0, curY, TaskInfoRect.width, labelSize.y),  label);
                curY += labelSize.y;
                int rowIndex = 0, columnIndex = 0;
                float iconSize = TaskInfoRect.width / 10f;
                foreach (var thing in CurTask.UnlocksThings)
                {
                    Rect thingRect = new Rect(rowIndex * iconSize, curY + (columnIndex * iconSize), iconSize, iconSize);
                    if(Mouse.IsOver(thingRect))
                        Widgets.DrawBoxSolid(thingRect, taskInfoBG + new Color(0.05f, 0.05f, 0.05f));

                    Widgets.DrawTextureFitted(thingRect.ContractedBy(1), thing.uiIcon, 1);
                    TooltipHandler.TipRegion(thingRect, thing.LabelCap);
                    if (Widgets.ButtonInvisible(thingRect))
                    {
                        Dialog_InfoCard.Hyperlink hyperlink = new Dialog_InfoCard.Hyperlink(thing);
                        hyperlink.OpenDialog();
                    }

                    rowIndex += 1;
                    if (rowIndex >= 10)
                    {
                        rowIndex = 0;
                        columnIndex += 1;
                    }
                }
                curY += columnIndex * iconSize;
            }

            GUI.EndGroup();
        }

        private void DrawTasks(Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect outRect = rect.AtZero();
            TRWidgets.DrawColoredBox(outRect, taskBG, ColorWhite50P, 1);
            Rect viewRect = new Rect(0, 0, outRect.width, taskCurrentHeight * SelProject.tasks.Count);
            Widgets.BeginScrollView(outRect, ref taskScrollPos, viewRect, false);

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
            Rect RecheckButton = RightInfoPartRect.LeftHalf().RightHalf().ContractedBy(2f);
            Rect ProgressBarRect = RightInfoPartRect.RightHalf().TopPart(0.65f).ContractedBy(5f);

            Rect debugRect = RightInfoPartRect.LeftHalf().TopHalf();
            Rect finButton = new Rect(debugRect.x, debugRect.y, 20, 15);
            Rect resetButt = new Rect(debugRect.x + 20, debugRect.y, 20, 15);

            Widgets.DrawTextureFitted(IconRect, ProjectStatusTexture(task.State), 1);
            
            //Debug
            if (DebugSettings.godMode)
            {
                if (Widgets.ButtonText(finButton, "fin"))
                    task.Debug_Finish();

                if (Widgets.ButtonText(resetButt, "rst"))
                    task.Debug_Reset();
            }

            /*Button to check objective state
            if (task.CanCheckTargets)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                bool recheckOver = Mouse.IsOver(RecheckButton);
                Color recheckColor = recheckOver ? Color.white : ColorWhite50P;
                GUI.color = recheckColor;

                TRWidgets.DrawBox(RecheckButton, recheckColor, 1);
                Widgets.Label(RecheckButton, "TR_RecheckButton".Translate());
                if (recheckOver)
                {
                    //TipRegion
                }

                if (Widgets.ButtonInvisible(RecheckButton, false))
                {
                    task.CheckTargets();
                }

                Text.Anchor = default;
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
            }
            */

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
                TRWidgets.DrawBox(rect, 0.5f, 1);



            Widgets.FillableBar(ProgressBarRect, task.ProgressPct, TRMats.blue, TRMats.black, true);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(ProgressBarRect, task.WorkLabel);
            Text.Anchor = default;

            Widgets.Label(LabelRect, task.LabelCap);
            GUI.color = ColorWhite50P;
            Widgets.Label(DescriptionRect, task.descriptionShort);
            GUI.color = Color.white;

            if (Mouse.IsOver(rect) && (DebugSettings.godMode || task.IsFinished || task == SelProject.CurrentTask))
            {
                TRWidgets.DrawBox(rect, 0.5f, 1);
                if (Widgets.ButtonInvisible(rect, false))
                {
                    CurTask = task;
                }
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
