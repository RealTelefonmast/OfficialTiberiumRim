using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ITab_MechConstruction : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(420f, 480f);
        private static Vector2 BPWinSize = new Vector2(350, 350);
        private static Vector2 BPSize = new Vector2(200, 200);

        private MechRecipeDef selectedRecipe;
        private float viewHeight = 1000f;
        private Vector2 scrollPosition = default(Vector2);

        public TRThingDef SelThingDef => SelThing.def as TRThingDef;


        public ITab_MechConstruction()
        {
            this.size = WinSize;
            this.labelKey = "TabMechs";
            //this.blueprint = new MechBlueprint("Pawns/Common/Harvester/Blueprint/Harvester");
        }

        public Comp_MechStation MechStation => SelThing.TryGetComp<Comp_MechStation>();

        private void SelectRecipe(MechRecipeDef recipe)
        {
            selectedRecipe = recipe;
        }

        public override void FillTab()
        {
            Rect tabRect = new Rect(0,0, WinSize.x, WinSize.y).ContractedBy(10);
            Widgets.BeginGroup(tabRect);
            Rect outRect = new Rect(0f, 35f, tabRect.width, tabRect.height - 35f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
            float num = 0f;
            for (var i = 0; i < MechStation.Props.mechRecipes.Count; i++)
            {
                var recipe = MechStation.Props.mechRecipes[i];
                Rect recipeRect = new Rect(0, num, viewRect.width, 75f);
                DoMechListing(recipeRect, recipe, i);
            }

            Widgets.EndScrollView();
            Widgets.EndGroup();
        }

        private void DoMechListing(Rect rect, MechRecipeDef recipe, int index)
        {
            rect = rect.ContractedBy(5f);
            if (index % 2 == 0)
            {
                Widgets.DrawAltRect(rect);
            }
            Rect iconRect = new Rect(rect.x, rect.y, rect.height, rect.height);
            //Widgets.DrawTextureFitted(iconRect, recipe.Blueprint.ActualMech, 1);
            Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.width-iconRect.width, rect.height);
            Widgets.Label(labelRect, recipe.mechDef.LabelCap);
            if (Widgets.ButtonInvisible(rect))
            {
                SelectRecipe(recipe);
            }
        }

        /*
        private void DoMechConstructor()
        {
            Rect windowRect = new Rect((UI.screenWidth - BPWinSize.x) * 0.5f, (UI.screenHeight - BPWinSize.x) * 0.5f, BPWinSize.x, BPWinSize.y);
            Find.WindowStack.ImmediateWindow(947528, windowRect, WindowLayer.GameUI, delegate
            {
                Rect BPRect = new Rect(new Vector2((BPWinSize.x - BPSize.x) * 0.5f, (BPWinSize.y - BPSize.y) * 0.5f), BPSize);
                Widgets.DrawTextureFitted(BPRect, SelBlueprint.Blueprint, 1f);

                Rect partSelection = new Rect(0,0, BPWinSize.y - BPSize.y, selectedRecipe.parts.Count * 15f).ContractedBy(5f);
                int partX = 0;
                for (int i = 0; i < selectedRecipe.parts.Count; i++)
                {
                    MechRecipePart part = (MechRecipePart)selectedRecipe.parts[i];
                    Vector2 size = Text.CalcSize(part.label);
                    Rect labelRect = new Rect(new Vector2(partX, 15f), size);
                    partX += 15;
                }

                Rect rectTwo = new Rect(BPRect.x, BPRect.yMax, 500, 25);
                Widgets.Checkbox(new Vector2(0, BPRect.yMax), ref selBools[0], 24f);
                Widgets.Checkbox(new Vector2(30, BPRect.yMax), ref selBools[1], 24f);
                Widgets.Checkbox(new Vector2(60, BPRect.yMax), ref selBools[2], 24f);
                Widgets.Checkbox(new Vector2(90, BPRect.yMax), ref selBools[3], 24f);
                if (selBools[0])
                {
                    Widgets.DrawTextureFitted(BPRect, SelBlueprint.Head, 1f);
                }

                if (selBools[1])
                {
                    Widgets.DrawTextureFitted(BPRect, SelBlueprint.Body, 1f);
                }

                if (selBools[2])
                {
                    Widgets.DrawTextureFitted(BPRect, SelBlueprint.Movement, 1f);
                }

                if (selBools[3])
                {
                    Widgets.DrawTextureFitted(BPRect, SelBlueprint.Manipulation, 1f);
                }
            }, true, false, 1f);
        }
        */

        public override void ExtraOnGUI()
        {
            base.ExtraOnGUI();
        }

        public override void CloseTab()
        {
            SelectRecipe(null);
            //Find.WindowStack.TryRemove(Find.WindowStack.Windows.First(w => w.ID == 947528), false);
        }
    }

    public class MechBlueprint
    {

    }
}
