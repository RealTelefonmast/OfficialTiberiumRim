using System.Linq;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim.Research.Window;

public class SubWindow_Projects
{
    //
    private readonly MainTabWindow_TibResearch parent;
    private TResearchDef _selResearch;
    
    //
    private static readonly Vector2 researchGroupSize = new Vector2(220, 30);
    
    //Research Option
    private static readonly Vector2 researchOptionSize = new Vector2(200, 30);
    private static readonly float researchOptionXOffset = (researchGroupSize.x - researchOptionSize.x) / 2;
    private const float researchOptionIconSize = 24;
    private const float bannerHeight = 50;
    private Vector2 projectScrollPos = Vector2.zero;

    #region Properties
    private TResearchManager Manager { get; }
    
    public TResearchDef SelProject
    {
        get => _selResearch;
        set
        {
            SetDiaShowFor(value);
            _selResearch = value;
        }
    }

    public TResearchTaskDef CurTask
    {
        get => Manager.TaskOverride ?? SelProject.CurrentTask;
        set => Manager.TaskOverride = value;
    }

    #endregion

    public SubWindow_Projects(MainTabWindow_TibResearch parent)
    {
        this.parent = parent;
        Manager = Find.World.GetComponent<TResearchManager>();
    }
    
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

    public void DrawLeft(Rect rect)
    {
        Rect bannerRect = new Rect(rect.x - 4, rect.y - 5, rect.width + 8, bannerHeight);
        Widgets.DrawTextureFitted(bannerRect, TiberiumContent.Banner, 1f);

        Widgets.BeginGroup(rect);
        var outRect = new Rect(0, 0, rect.width, rect.height - bannerHeight);
        var viewRect = new Rect(0, 0, outRect.width, outRect.height);
        var curY = bannerRect.height + 5;
        Widgets.BeginScrollView(outRect, ref projectScrollPos, viewRect, true);
        foreach (var researchGroup in Manager.Groups)
        {
            if (researchGroup.IsVisible)
                DrawResearchGroup(ref curY, researchGroup);
        }

        Widgets.EndScrollView();
        Widgets.EndGroup();
    }

    private void DrawResearchGroup(ref float curY, TResearchGroupDef group)
    {
        if (group.ActiveProjects.NullOrEmpty()) return;

        var height = group.ActiveProjects.Count() * researchOptionSize.y;
        var textHeight = Text.CalcHeight(group.LabelCap, researchGroupSize.x);
        var groupOptionRect = new Rect(0, curY, researchGroupSize.x, researchGroupSize.y + textHeight);
        curY += groupOptionRect.height;

        if (TRWidgets.ButtonColoredHighlight(groupOptionRect, group.LabelCap.RawText.Bold(),
                TRColor.MenuSectionBGFillColor, TRColor.MenuSectionBGBorderColor))
        {
            Manager.OpenClose(group);
        }

        if (group.HasUnseenProjects)
        {
            TWidgets.DrawTextureInCorner(groupOptionRect, TiberiumContent.NewResearch, 50, TextAnchor.UpperRight,
                new Vector2(-1, 1));
        }

        if (Manager.IsOpen(group))
        {
            var groupOptionSelection = new Rect(researchOptionXOffset, curY, researchOptionSize.x, height);
            Widgets.DrawMenuSection(groupOptionSelection);

            foreach (var project in group.ActiveProjects)
            {
                float margin = (researchOptionSize.y - 24f) / 2;
                WidgetRow row = new WidgetRow(researchOptionXOffset + margin, curY + margin, UIDirection.RightThenDown);
                row.Icon(project.HasBeenSeen ? ProjectStatusTexture(project.State) : TiberiumContent.UnseenResearch);
                row.Label(project.LabelCap);

                var projectOptionRect =
                    new Rect(researchOptionXOffset, curY, researchOptionSize.x, researchOptionSize.y);

                if (Mouse.IsOver(projectOptionRect) || project == SelProject)
                {
                    TRUtils.ResearchDiscoveryTable().DiscoverResearch(project);
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

        curY += +5f;
        //Text.Anchor = default;
        //curY += height;
    }
    
    private static Texture2D ProjectStatusTexture(ResearchState state)
    {
        return state switch
        {
            ResearchState.Finished => Widgets.CheckboxOnTex,
            ResearchState.InProgress => TiberiumContent.Research_Active,
            ResearchState.Available => TiberiumContent.Research_Available,
            _ => BaseContent.BadTex
        };
    }
}