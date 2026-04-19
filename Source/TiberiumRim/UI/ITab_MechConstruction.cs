using System.Collections.Generic;
using RimWorld;
using TiberiumRim;
using UnityEngine;
using Verse;

namespace TR;

public class ITab_MechConstruction : ITab
{
    private static readonly Vector2 WinSize = new(420f, 480f);
    private static readonly Rect WinRect = new(0, 0, WinSize.x, WinSize.y);
    private static readonly Vector2 BillSize = new(WinSize.x, 50f);
    private static float billPadding;
    private readonly float viewHeight = 1000f;
    private Vector2 scrollPosition;

    public bool[] selBools = new bool[4] { false, false, false, false };

    private MechRecipeDef selectedRecipe;

    public ITab_MechConstruction()
    {
        size = WinSize;
        labelKey = "TR.TabMechs";
        //this.blueprint = new MechBlueprint("Pawns/Common/Harvester/Blueprint/Harvester");
    }

    public TRThingDef SelThingDef => SelThing.def as TRThingDef;

    private Building_Hangar Hangar => SelThing as Building_Hangar;
    public Comp_MechStation MechStation => SelThing.TryGetComp<Comp_MechStation>();
    public MechBlueprint SelBlueprint => selectedRecipe?.Blueprint;

    private void SelectRecipe(MechRecipeDef recipe)
    {
        selectedRecipe = recipe;
    }

    protected override void FillTab()
    {
        var tabRect = new Rect(0, 0, WinSize.x, WinSize.y).ContractedBy(10);
        GUI.BeginGroup(tabRect);
        var outRect = new Rect(0f, 35f, tabRect.width, tabRect.height - 35f);
        var viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var num = 0f;
        for (var i = 0; i < MechStation.Props.mechRecipes.Count; i++)
        {
            var recipe = MechStation.Props.mechRecipes[i];
            var recipeRect = new Rect(0, num, viewRect.width, 75f);
            DoMechListing(recipeRect, recipe, i);
        }

        Widgets.EndScrollView();
        GUI.EndGroup();

        if (selectedRecipe != null)
            DoMechConstructor();
    }

    private void DoMechListing(Rect rect, MechRecipeDef recipe, int index)
    {
        rect = rect.ContractedBy(5f);
        if (index % 2 == 0) Widgets.DrawAltRect(rect);
        var iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
        Widgets.DrawTextureFitted(iconRect, recipe.Blueprint.ActualMech, 1);
        var labelRect = new Rect(iconRect.xMax, rect.y, rect.width - iconRect.width, rect.height);
        Widgets.Label(labelRect, recipe.mechDef.LabelCap);
        if (Widgets.ButtonInvisible(rect)) SelectRecipe(recipe);
    }

    private void DoMechConstructor()
    {
        var windowRect = new Rect((UI.screenWidth - BPWinSize.x) * 0.5f, (UI.screenHeight - BPWinSize.x) * 0.5f,
            BPWinSize.x, BPWinSize.y);
        Find.WindowStack.ImmediateWindow(947528, windowRect, WindowLayer.GameUI, delegate
        {
            var BPRect = new Rect(new Vector2((BPWinSize.x - BPSize.x) * 0.5f, (BPWinSize.y - BPSize.y) * 0.5f),
                BPSize);
            Widgets.DrawTextureFitted(BPRect, SelBlueprint.Blueprint, 1f);

            var partSelection =
                new Rect(0, 0, BPWinSize.y - BPSize.y, selectedRecipe.parts.Count * 15f).ContractedBy(5f);
            var partX = 0;
            for (var i = 0; i < selectedRecipe.parts.Count; i++)
            {
                var part = selectedRecipe.parts[i];
                var size = Text.CalcSize(part.label);
                var labelRect = new Rect(new Vector2(partX, 15f), size);
                partX += 15;
            }

            var rectTwo = new Rect(BPRect.x, BPRect.yMax, 500, 25);
            Widgets.Checkbox(new Vector2(0, BPRect.yMax), ref selBools[0]);
            Widgets.Checkbox(new Vector2(30, BPRect.yMax), ref selBools[1]);
            Widgets.Checkbox(new Vector2(60, BPRect.yMax), ref selBools[2]);
            Widgets.Checkbox(new Vector2(90, BPRect.yMax), ref selBools[3]);
            if (selBools[0]) Widgets.DrawTextureFitted(BPRect, SelBlueprint.Head, 1f);

            if (selBools[1]) Widgets.DrawTextureFitted(BPRect, SelBlueprint.Body, 1f);

            if (selBools[2]) Widgets.DrawTextureFitted(BPRect, SelBlueprint.Movement, 1f);

            if (selBools[3]) Widgets.DrawTextureFitted(BPRect, SelBlueprint.Manipulation, 1f);
        });
    }

    protected override void ExtraOnGUI()
    {
        base.ExtraOnGUI();
    }

    protected override void CloseTab()
    {
        SelectRecipe(null);
        //Find.WindowStack.TryRemove(Find.WindowStack.Windows.First(w => w.ID == 947528), false);
    }
}

public class MechBlueprint
{
    public Texture2D ActualMech;
    public Texture2D Blueprint;
    public Texture2D Body;
    public Texture2D Head;
    public Texture2D Manipulation;
    public Texture2D Movement;


    public MechBlueprint(string path)
    {
        Blueprint = ContentFinder<Texture2D>.Get(path + "/Blueprint/Blueprint");
        Head = ContentFinder<Texture2D>.Get(path + "/Blueprint/Head");
        Body = ContentFinder<Texture2D>.Get(path + "/Blueprint/Body");
        Movement = ContentFinder<Texture2D>.Get(path + "/Blueprint/Movement");
        Manipulation = ContentFinder<Texture2D>.Get(path + "/Blueprint/Tool");

        var splitted = path.Split('/');
        ActualMech = ContentFinder<Texture2D>.Get(path + "/" + splitted[splitted.Length - 1] + "_south");
    }
}

//TODO: Add as defs in xml
public class MechRecipeDef : Def
{
    [Unsaved] private MechBlueprint cachedBP;
    public string graphicPath;
    public PawnKindDef mechDef;
    public List<MechRecipePart> parts;

    public MechBlueprint Blueprint
    {
        get
        {
            if (cachedBP == null)
                cachedBP = new MechBlueprint(graphicPath);
            return cachedBP;
        }
    }
}

public class MechRecipePart
{
    public List<ThingDefCountClass> ingredients;
    public string label;
    public MechRecipePartType type;

    //TODO: Upgrade Options, speed, resistance etc
}

public class MechUpgradeDef : Def
{
    public List<ThingDefCountClass> cost;
    //public 
}

public enum MechRecipePartType
{
    Head,
    Body,
    Tool,
    Movement
}

public interface IMechSpawner
{
    ThingOwner Container { get; set; }
    bool HoldsMech { get; set; }

    void SpawnMech();
}