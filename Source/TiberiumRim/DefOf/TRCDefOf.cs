using RimWorld;
using TeleCore.GameData.Defs;
using TeleCore.UI.SpecialSubMenu;
using Verse;
using TRMainButtonDef = TR.TRMainButtonDef;

namespace TR;

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