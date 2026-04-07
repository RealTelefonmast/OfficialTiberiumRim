using UnityEngine;
using Verse;

namespace TeleCore.Rendering.UI.SpecialSubMenu;

public class Designator_SubBuildMenu : Designator
{
    private readonly SubBuildMenuDef subMenuDef;

    public Designator_SubBuildMenu(SubBuildMenuDef menuDef)
    {
        order = -1;
        subMenuDef = menuDef;
    }

    public override string Label => "Reset";
    public override string Desc => "Reset window position.";

    public void Toggle_Menu(bool opening)
    {
        SubBuildMenu.ToggleOpen(subMenuDef, opening);
    }

    public override void ProcessInput(Event ev)
    {
        SubBuildMenu.ResetMenuWindow(subMenuDef);
    }

    public override AcceptanceReport CanDesignateCell(IntVec3 loc)
    {
        return AcceptanceReport.WasRejected;
    }
}