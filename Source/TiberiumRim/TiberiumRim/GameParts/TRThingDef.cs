using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using StoryFramework;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TRThingDef : FXThingDef
    {
        public FactionDesignationDef factionDesignation = null;
        public TRThingCategoryDef TRCategory = null;
        public GraphicData extraGraphicData;
        public TurretHolderProps turret;
        public BeamHubProperties beamHub; 
        public ProjectileProperties_Extended projectileExtended;
        public SuperWeaponProperties superWeapon;
        public TerrainDef makesTerrain;
        public bool needsBlueprint = false;
        public bool hidden = false;
        public bool devObject = false;
        public bool destroyTiberium = false;

        private bool discovered = false;

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

        public bool Discovered
        {
            get => discovered || devObject;
            set => discovered = value;
        }

        public bool IsActive(out string reason)
        {
            reason = "";
            bool b = true;
            var sb = new StringBuilder();
            sb.AppendLine("Locked due to:\n");
            if (DebugSettings.godMode)
            {
                return true;
            }
            if (this.devObject)
            {
                return DebugSettings.godMode;
            }
            if (this.minTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel < this.minTechLevelToBuild)
            {
                b = false;
                sb.AppendLine("- Need min tech level: " + this.minTechLevelToBuild);
            }
            if (this.maxTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel > this.maxTechLevelToBuild)
            {
                b = false;
                sb.AppendLine("- Need max tech level: " + this.maxTechLevelToBuild);
            }
            if (!this.IsResearchFinished)
            {
                b = false;
                string research = "";
                foreach (var res in this.researchPrerequisites)
                {
                    research += "   - " + res.LabelCap;
                }
                sb.AppendLine("- Need research:\n" + research);
            }
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
            if (this.buildingPrerequisites != null)
            {
                b = b && this.buildingPrerequisites.All(t => Find.CurrentMap.listerBuildings.ColonistsHaveBuilding(t));
                if (!b)
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
            return b;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
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
