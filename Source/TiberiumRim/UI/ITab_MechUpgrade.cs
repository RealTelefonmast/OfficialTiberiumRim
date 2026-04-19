using RimWorld;
using UnityEngine;

namespace TR;

public class ITab_MechUpgrade : ITab
{
    private static readonly Vector2 WinSize = new(420f, 480f);
    private static Vector2 BPWinSize = new(275, 275);
    private static Vector2 BPSize = new(200, 200);
    private Vector2 scrollPosition = default;
    private MechRecipeDef selectedRecipe;
    private float viewHeight = 1000f;


    public ITab_MechUpgrade()
    {
        size = WinSize;
        labelKey = "TabMechsUpgrade";
        //this.blueprint = new MechBlueprint("Pawns/Common/Harvester/Blueprint/Harvester");
    }

    public override void FillTab()
    {
    }
}