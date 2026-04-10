using RimWorld;
using TeleCore.Static;
using TeleCore.Utils;
using TR.TextureContent;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TR;

public class ITab_MechConstruction : ITab
{
    private static readonly Vector2 WinSize = new(420f, 480f);
    private static readonly Rect WinRect = new(0, 0, WinSize.x, WinSize.y);
    private static readonly Vector2 BillSize = new(WinSize.x, 50f);
    private static float billPadding;
    private Vector2 scrollPosition;

    public ITab_MechConstruction()
    {
        size = WinSize;
        labelKey = "TR.TabMechs";
    }

    private Building_Hangar Hangar => SelThing as Building_Hangar;
    public Comp_MechStation MechStation => SelThing.TryGetComp<Comp_MechStation>();

    public override void FillTab()
    {
        var rect = WinRect.ContractedBy(10);

        Widgets.BeginGroup(rect);
        Text.Font = GameFont.Small;

        var rect2 = new Rect(0f, 0f, 150f, 29f);
        if (Widgets.ButtonText(rect2, "AddBill".Translate()))
            Find.WindowStack.Add(new FloatMenu(MechStation.RecipeOptions(Hangar)));

        Text.Anchor = TextAnchor.UpperLeft;
        GUI.color = Color.white;

        //Draw Bills
        //Bills are drawn as icon of the thing, label and a progressbar (first for resources collected, second for time)
        var bills = Hangar.Bills.Queue;

        var billListHeight = (BillSize.y + billPadding) * bills.Count;
        var billAreaRect = new Rect(0, 35f, rect.width, rect.height - 35f);
        var billViewRect = new Rect(0, 0, billAreaRect.width, billListHeight);

        var curY = 0;
        var indexer = 0;
        Widgets.BeginScrollView(billAreaRect, ref scrollPosition, billViewRect);

        for (var i = 0; i < bills.Count; i++)
        {
            var bill = bills[i];
            var billRect = new Rect(0, curY, rect.width, BillSize.y);
            DrawBill(billRect, bill, indexer);
            curY += (int)BillSize.y + (int)billPadding;
            indexer++;
        }

        Widgets.EndScrollView();
        Widgets.EndGroup();
    }

    private void DrawBill(Rect rect, MechConstructionBill bill, int index)
    {
        var leftPart = rect.LeftPartPixels(rect.height);
        var rightPart = rect.RightPartPixels(rect.width - leftPart.width);
        var iconRect = leftPart.ContractedBy(2);
        var labelRect = rightPart.TopHalf().ContractedBy(2);
        var barArea = rightPart.BottomHalf();
        var itemProgressRect = barArea.LeftHalf().ContractedBy(8, 2);
        var progressRect = barArea.RightHalf().ContractedBy(8, 2);

        if (index % 2 == 0) Widgets.DrawAltRect(rect);
        Widgets.DrawHighlightIfMouseover(rect);

        //Draw Icon
        Widgets.DrawTextureFitted(iconRect, bill.Recipe.mechDef.race.uiIcon, 1);

        //Draw Label
        Widgets.Label(labelRect, bill.Recipe.mechDef.LabelCap);

        //Draw Settings
        var rect4 = new Rect(rect.width - 24f, 0f, 24f, 24f);
        if (Widgets.ButtonImage(rect4, TexButton.Delete, Color.white, Color.white * GenUI.SubtleMouseoverColor))
        {
            Hangar.Bills.Delete(bill);
            SoundDefOf.Click.PlayOneShotOnCamera();
        }

        TooltipHandler.TipRegionByKey(rect4, "DeleteBillTip");

        //Draw Progress
        Widgets.FillableBar(progressRect, bill.ProgressPercent, TiberiumContent.GreenBar, TiberiumContent.BarBG, false);
        TWidgets.Label(progressRect, bill.ProgressLabel, TextAnchor.MiddleCenter);
        Widgets.FillableBar(itemProgressRect, bill.ItemProgress, TiberiumContent.BlueBar, TiberiumContent.BarBG, false);
        TWidgets.Label(itemProgressRect, bill.ItemProgressLabel, TextAnchor.MiddleCenter);

        TWidgets.DrawBox(progressRect, TColor.BGDarker, 3);
        TWidgets.DrawBox(itemProgressRect, TColor.BGDarker, 3);

        if (bill.IsPreparing)
        {
        }
    }
}

/*[HotSwappable]
public class ITab_MechConstruction : ITab
{
    private static readonly Vector2 WinSize = new Vector2(420f, 480f);
    private static Rect WinRect = new Rect(0, 0, WinSize.x, WinSize.y).ContractedBy(10);
    private static Vector2 BPWinSize = new Vector2(350, 350);
    private static Vector2 BPSize = new Vector2(200, 200);

    private float viewHeight = 1000f;
    private Vector2 scrollPosition = default(Vector2);

    private const int optionsPerRow = 4;
    private const int padding = 2;

    private float OptionSize => (WinRect.width - (optionsPerRow * padding)) / optionsPerRow;

    public TRThingDef SelThingDef => SelThing.def as TRThingDef;
    public Comp_MechStation MechStation => SelThing.TryGetComp<Comp_MechStation>();
    Building_Hangar Hangar => SelThing as Building_Hangar;

    public ITab_MechConstruction()
    {
        this.size = WinSize;
        this.labelKey = "TabMechs";
        //this.blueprint = new MechBlueprint("Pawns/Common/Harvester/Blueprint/Harvester");
    }

    private void SelectRecipe(MechRecipeDef recipe)
    {
        Hangar.AddMechConstructionBill(recipe);
    }

    public override void FillTab()
    {
        Widgets.BeginGroup(WinRect);
        {
            Rect outRect = new Rect(0f, 35f, WinRect.width, WinRect.height - 35f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
            if (MechStation.Props.mechRecipes != null)
            {
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
                float num = 0f;
                for (var i = 0; i < MechStation.Props.mechRecipes.Count; i++)
                {
                    var recipe = MechStation.Props.mechRecipes[i];
                    Rect recipeRect = new Rect(0, num, viewRect.width, 75f);
                    DoMechListing(recipeRect, recipe, i);
                }
                Widgets.EndScrollView();
            }
        }
        Widgets.EndGroup();
    }

    private Texture2D SelectionTex = ContentFinder<Texture2D>.Get("Menu/SubBuildMenu/Des");
    private Texture2D SelectionTex2 = ContentFinder<Texture2D>.Get("Menu/SubBuildMenu/Des_Sel");

    private void DrawMechOptions(Rect rect)
    {
        var recipes = MechStation.Props.mechRecipes;

        for (int i = 0; i < recipes.Count; i++)
        {
            var recipe = recipes[i];
            Rect rect2 = new Rect(rect.x + OptionSize * (float)(i % optionsPerRow), rect.y + (float)(i / optionsPerRow) * 100f, OptionSize, 100f);
            DrawOption(rect2, recipe);
        }
    }

    private void DrawOption(Rect rect, MechRecipeDef recipe)
    {
        var mouseOver = Mouse.IsOver(rect);
        var tex = mouseOver ? SelectionTex : SelectionTex2;
        Widgets.DrawTextureFitted(rect, tex, 1f);
        Widgets.DrawTextureFitted(rect.ContractedBy(2), recipe.mechDef.race.uiIcon, 1f);
    }

    private void DrawMechBills(Rect rect)
    {
        var bills = Hangar.Bills.All;
    }

    private void DoMechListing(Rect rect, MechRecipeDef recipe, int index)
    {
        rect = rect.ContractedBy(5f);
        if (index % 2 == 0)
        {
            Widgets.DrawAltRect(rect);
        }

        Widgets.DrawHighlightIfMouseover(rect);

        Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
        //Widgets.DrawTextureFitted(iconRect, recipe.Blueprint.ActualMech, 1);
        Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.width-iconRect.width, rect.height);
        Widgets.Label(labelRect, recipe.mechDef.LabelCap);
        Widgets.FillableBar(labelRect.BottomHalf(), Hangar.Bills.GetItemProgress(recipe));
        if (Widgets.ButtonInvisible(rect))
        {
            SelectRecipe(recipe);
        }
    }

    public override void ExtraOnGUI()
    {
        base.ExtraOnGUI();
    }

    public override void CloseTab()
    {
        SelectRecipe(null);
    }
}*/