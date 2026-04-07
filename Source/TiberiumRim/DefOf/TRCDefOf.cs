using RimWorld;
using TeleCore.Defs;
using TeleCore.Rendering.UI.SpecialSubMenu;
using Verse;
using TRMainButtonDef = TR.Research.TRMainButtonDef;

namespace TR.DefOf;

[RimWorld.DefOf]
public class TRCDefOf
{
    //MainButton
    public static TRMainButtonDef TiberiumTab;

    //DesignationCategory
    public static SubMenuDesignationCategoryDef TiberiumBuildings;

    //
    public static LetterDef EventLetter;
    public static LetterDef DiscoveryLetter;

    //
    public static StatDef ExtraCarryWeight;

    //
    public static JobDef TiberiumResearch;

    //FleshTypes
    public static FleshTypeDef Mechanical;

    //
    public static JobDef DoMechConstructionBill;
    public static ThingGroupDef MechHangars;

    public static JobDef RepairMechanicalPawn;
    public static JobDef ReturnFromRepair;
}