using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TeleCore.Research;
using TeleCore.RWExtended.ThingClasses;
using TeleCore.Types;
using TR.GameParts;
using TR.Util;
using TR.Weaponry;
using UnityEngine;
using Verse;

namespace TR.Defs;

public class TRThingDef : FXThingDef
{
    public BeamHubProperties beamHub;

    [Unsaved] private TaggedString cachedUnknownLabelCap = null;

    public bool clearTiberium = false;

    public List<ConditionalStatModifier> conditionalStatOffsets;
    public bool devObject = false;

    public DiscoveryProperties discovery;

    //Designation
    public FactionDesignationDef factionDesignation = null;
    public bool hidden = false;
    public bool isNatural = false;
    public TRThingDef leavesThing;

    //Creation Events
    public TerrainDef makesTerrain;
    public ProjectileProperties_Extended projectileExtended;
    public Requisites requisites;
    public SuperWeaponProperties superWeapon;
    public TRThingCategoryDef TRCategory = null;

    //Properties
    public TurretHolderProperties turret;

    public string UnknownLabelCap
    {
        get
        {
            if (cachedUnknownLabelCap.NullOrEmpty())
                cachedUnknownLabelCap = discovery.unknownLabel.CapitalizeFirst();
            return cachedUnknownLabelCap;
        }
    }

    public bool RequisitesFulfilled => requisites == null || requisites.FulFilled();

    public bool ConstructionOptionDiscovered
    {
        get => TRUtils.Tiberium().DiscoveryTable.MenuOptionHasBeenSeen(this) || devObject;
        set
        {
            if (value) TRUtils.Tiberium().DiscoveryTable.DiscoverInMenu(this);
        }
    }

    public override IEnumerable<string> ConfigErrors()
    {
        var strings = new List<string>();
        strings.AddRange(base.ConfigErrors());

        /*
        if (TRGroup == null)
        {
            //strings.Add("Missing TRGroupDef, adding basic...");
            TRGroup = ThingGroupDefOf.All;
        }
        */

        /*
        if(factionDesignation != FactionDesignationDefOf.None && thingClass.IsAssignableFrom(typeof(Building)) && !thingClass.IsAssignableFrom(typeof(TRBuilding)))
            strings.Add(this.defName + " won't have a build designator.");
        */
        return strings;
    }

    public bool IsActive(out string reason)
    {
        reason = "";
        var research = "";
        var flag = true;
        var sb = new StringBuilder();
        sb.AppendLine("TR_LockedReason".Translate());
        if (DebugSettings.godMode) return true;
        if (devObject) return DebugSettings.godMode;
        if (minTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel < minTechLevelToBuild)
        {
            flag = false;
            sb.AppendLine("TR_LockedDueMinTech".Translate(minTechLevelToBuild.ToString()));
        }

        if (maxTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel > maxTechLevelToBuild)
        {
            flag = false;
            sb.AppendLine("TR_LockedDueMaxTech".Translate(maxTechLevelToBuild.ToString()));
        }

        if (!IsResearchFinished)
        {
            flag = false;
            foreach (var res in researchPrerequisites) research += "   - " + res.LabelCap;
            sb.AppendLine("TR_LockedDueMissingResearch".Translate(research));
            research = "";
        }

        if (!RequisitesFulfilled)
        {
            flag = false;
            sb.AppendLine(requisites.MissingString());
        }

        /*
        if (this.HasStoryExtension())
        {
            bool r = false;
            b = b && StoryPatches.CanBeMade(this, ref r);
            if (!b)
            {
                var story = this.GetModExtension<StoryThingDefExtension>();
                string objectives = "";
                foreach (var obj in story.objectiveRequisites)
                {
                    objectives += "   - " + obj.LabelCap;
                }
                sb.AppendLine("- Need Objectives:\n" + objectives);
            }
        }
        */
        if (!buildingPrerequisites.NullOrEmpty())
        {
            flag = flag && buildingPrerequisites.All(t => Find.CurrentMap.listerBuildings.ColonistsHaveBuilding(t));
            if (!flag)
            {
                var buildings = "";
                foreach (var build in buildingPrerequisites) buildings += "   - " + build.LabelCap;
                sb.AppendLine("- Need constructed buildings:\n" + buildings);
            }
        }

        reason = sb.ToString().TrimEndNewlines();
        return flag;
    }
}

public class FactionDesignationDef : Def
{
    public string packPath = "";
    public List<TRThingCategoryDef> subCategories = new();
}

public class DesignationTexturePack
{
    public Texture2D BackGround;
    public Texture2D Designator;
    public Texture2D DesignatorSelected;
    public Texture2D Tab;
    public Texture2D TabSelected;

    public DesignationTexturePack(FactionDesignationDef def)
    {
        BackGround = ContentFinder<Texture2D>.Get(def.packPath + "/" + "BuildMenu");
        Tab = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Tab");
        TabSelected = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Tab_Selected");
        Designator = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Des");
        DesignatorSelected = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Des_Selected");
    }
}

public class TRThingCategoryDef : Def
{
}