// TODO: Decouple from TiberiumRim-specific types (TRUtils, TRLog, TRContent, TRCDefOf, TRThingDef)

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Defs;
using TeleCore.WorldComponents;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public enum ResearchTabOption
{
    Projects,
    Events,
    Wiki
}

public class MainTabWindow_TResearch : MainTabWindow
{
    //Dimensions
    private const float leftWidth = 250; //Original: 200
    private const float tabHeight = 32;


    private static readonly float taskOtherHeight = 30;
    //private static Vector2 tabOptionSize = new Vector2(80, 20);

    //Colors
    private static readonly Color taskBG = new(0, 0, 0, 0.1f);
    private static readonly Color ColorWhite50P = new(1, 1, 1, 0.5f);
    private static readonly Color ColorWhite05P = new(1, 1, 1, 0.05f);
    private static readonly Color taskInfoBG = new ColorInt(33, 33, 33).ToColor;

    private readonly List<TabRecord> cachedTabs;
    private readonly SubWindow_Events eventsView;
    private readonly SubWindow_Projects projectsView;
    private readonly SubWindow_Wiki wikiView;

    //
    private float bannerHeight = 50;

    private Vector2 taskScrollPos = Vector2.zero;

    public MainTabWindow_TResearch()
    {
        //Calc Banner height
        var diff = TRContent.Banner.width - (leftWidth - 2);
        var pct = 1 - diff / TRContent.Banner.width;
        bannerHeight = TRContent.Banner.height * pct;

        eventsView = new SubWindow_Events();
        projectsView = new SubWindow_Projects(this);
        wikiView = new SubWindow_Wiki();

        cachedTabs = new List<TabRecord>
        {
            new("TR_MainTabResearch".Translate(), delegate { SelTab = ResearchTabOption.Projects; },
                () => SelTab == ResearchTabOption.Projects)
            // new("TR_MainTabEvents".Translate(), delegate { SelTab = ResearchTabOption.Events; },
            //     () => SelTab == ResearchTabOption.Events),
            // new("TR_MainTabWiki".Translate(), delegate { SelTab = ResearchTabOption.Wiki; },
            //     () => SelTab == ResearchTabOption.Wiki)
        };
    }

    public override float Margin => 0f;

    public override Vector2 RequestedTabSize => new(1280f, 720f); //new Vector2(UI.screenWidth, UI.screenHeight * 0.6f);

    public bool HasUnseenProjects => Manager.Groups.Where(g => g.IsVisible).Any(g => g.HasUnseenProjects);

    public TResearchManager Manager => Find.World.GetComponent<TResearchManager>();

    public ResearchTabOption SelTab { get; set; } = ResearchTabOption.Projects;

    public override void PreOpen()
    {
        base.PreOpen();
    }

    public override void PostOpen()
    {
        base.PostOpen();
        //Find.WindowStack.Add(new ModuleVisualizer());
        //esearchRoots.AddRange(DefDatabase<TResearchDef>.AllDefs.Where(t => t.requisites?.tiberiumResearch.NullOrEmpty() ?? false));
    }

    //
    public override void DoWindowContents(Rect inRect)
    {
        //Draw BackGround Image Here

        var rect = inRect.ContractedBy(5f);
        Widgets.BeginGroup(rect);

        var LeftRect = new Rect(0, 0, leftWidth, rect.height);
        var RightRect = new Rect(LeftRect.xMax, 0, rect.width - LeftRect.width, rect.height);

        DrawLeftPart(LeftRect.ContractedBy(10f));
        DrawRightPart(RightRect.ContractedBy(10f));

        Widgets.EndGroup();

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
        Widgets.BeginGroup(rect);

        var tabRect = new Rect(0, tabHeight, rect.width, tabHeight);
        var menuRect = new Rect(0, tabHeight, rect.width, rect.height - tabHeight);
        Widgets.DrawMenuSection(menuRect);

        //Draw Tabs
        //
        TabDrawer.DrawTabs(tabRect, cachedTabs);

        //Rect bannerRect = new Rect(rect.x - 4, rect.y - 5, rect.width + 8, bannerHeight);
        //Widgets.DrawTextureFitted(bannerRect, TiberiumContent.Banner, 1f);

        switch (SelTab)
        {
            case ResearchTabOption.Projects:
                projectsView.DrawMenu(menuRect.ContractedBy(5f));
                break;
            case ResearchTabOption.Events:
                eventsView.DrawMenu(menuRect.ContractedBy(5f));
                break;
            case ResearchTabOption.Wiki:
                wikiView.DrawMenu(menuRect.ContractedBy(5f));
                break;
        }

        Widgets.EndGroup();
    }

    private void DrawRightPart(Rect rect)
    {
        switch (SelTab)
        {
            case ResearchTabOption.Projects:
                projectsView.DrawMain(rect.ContractedBy(5f));
                break;
            case ResearchTabOption.Events:
                eventsView.DrawMain(rect.ContractedBy(5f));
                break;
            case ResearchTabOption.Wiki:
                wikiView.DrawMain(rect.ContractedBy(5f));
                break;
        }
    }

    /*
    private void DrawTabsWindow(Rect rect)
    {
        Find.WindowStack.ImmediateWindow(873459, rect, WindowLayer.GameUI, delegate
        {
            DrawProject(rect.ContractedBy(10f));
        }, true, true, 1f);
    }
    */

    public void SetProject(TResearchDef proj)
    {
        projectsView.SelProject = proj;
    }

    /*
    private float HeightFrom(TResearchDef props)
    {
        float val = selSize.y;
        int num = 0;
        for (num = 0; props.unlocks.Count > 0; num += props.unlocks.Count) { }
        if (num > 0)
            val += (num - 1) * selSize.y;
        foreach (var def2 in props.unlocks)
            val += HeightFrom(def2);
        return val;
    }

    private float WidthFrom(TResearchDef props)
    {
        float val = selSize.x;
        if (props.unlocks.Count > 0)
            val += selSize.x;
        foreach (var def2 in props.unlocks)
            val += WidthFrom(def2);
        return val;
    }
    */
}