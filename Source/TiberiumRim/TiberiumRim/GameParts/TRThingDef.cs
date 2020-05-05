using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TRThingDef : FXThingDef
    {
        //Designation
        public FactionDesignationDef factionDesignation = null;
        public TRThingCategoryDef TRCategory = null;
        public Requisites requisites;

        //Graphics
        public GraphicData extraGraphicData;

        //Properties
        public TurretHolderProperties turret;
        public BeamHubProperties beamHub; 
        public ProjectileProperties_Extended projectileExtended;
        public SuperWeaponProperties superWeapon;

        //Creation Events
        public TerrainDef makesTerrain;
        public TRThingDef leavesThing;

        public bool hidden = false;
        public bool devObject = false;
        public bool clearTiberium = false;

        public string discoverTag;
        public string unknownLabel;
        public string unknownDescription;
        public string extraDescription;

        [Unsaved(false)]
        private TaggedString cachedUnknownLabelCap = null;

        private bool optionDiscovered = false;

        public override IEnumerable<string> ConfigErrors()
        {
            List<string> strings = new List<string>();
            strings.AddRange(base.ConfigErrors());
            /*
            if(factionDesignation != FactionDesignationDefOf.None && thingClass.IsAssignableFrom(typeof(Building)) && !thingClass.IsAssignableFrom(typeof(TRBuilding)))
                strings.Add(this.defName + " won't have a build designator.");
            */
            return strings;

        }

        public string UnknownLabelCap
        {
            get
            {
                if (cachedUnknownLabelCap.NullOrEmpty())
                    cachedUnknownLabelCap = unknownLabel.CapitalizeFirst();
                return cachedUnknownLabelCap;
            }
        }

        public bool RequisitesFulfilled => requisites == null || requisites.FulFilled();

        public bool ConstructionOptionDiscovered
        {
            get => optionDiscovered || devObject;
            set => optionDiscovered = value;
        }

        public bool IsActive(out string reason)
        {
            reason = "";
            string research = "";
            bool flag = true;
            var sb = new StringBuilder();
            sb.AppendLine("Locked due to:\n");
            if (DebugSettings.godMode)
            {
                return true;
            }
            if (devObject)
            {
                return DebugSettings.godMode;
            }
            if (minTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel < minTechLevelToBuild)
            {
                flag = false;
                sb.AppendLine("- Need min tech level: " + minTechLevelToBuild);
            }
            if (maxTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel > maxTechLevelToBuild)
            {
                flag = false;
                sb.AppendLine("- Need max tech level: " + maxTechLevelToBuild);
            }
            if (!IsResearchFinished)
            {
                flag = false;
                foreach (var res in researchPrerequisites)
                {
                    research += "   - " + res.LabelCap;
                }
                sb.AppendLine("- Need research:\n" + research);
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
                    string buildings = "";
                    foreach (var build in this.buildingPrerequisites)
                    {
                        buildings += "   - " + build.LabelCap;
                    }
                    sb.AppendLine("- Need constructed buildings:\n" + buildings);
                }
            }
            reason = sb.ToString().TrimEndNewlines();
            return flag;
        }
    }

    public class FactionDesignationDef : Def
    {
        public List<TRThingCategoryDef> subCategories = new List<TRThingCategoryDef>();
        public string packPath = "";
    }

    public class DesignationTexturePack
    {
        public Texture2D BackGround;
        public Texture2D Tab;
        public Texture2D TabSelected;
        public Texture2D Designator;
        public Texture2D DesignatorSelected;

        public DesignationTexturePack(FactionDesignationDef def)
        {
            BackGround = ContentFinder<Texture2D>.Get(def.packPath + "/" + "BuildMenu");
            Tab = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Tab");
            TabSelected = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Tab_Selected");
            Designator = ContentFinder<Texture2D>.Get(def.packPath + "/" + "Des");
            DesignatorSelected = ContentFinder<Texture2D>.Get((def.packPath + "/" + "Des_Selected"));
        }
    }

    public class TRThingCategoryDef : Def
    {
    }
}
