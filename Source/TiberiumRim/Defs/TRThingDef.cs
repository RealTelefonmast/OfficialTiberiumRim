using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TRThingDef : ThingDef
    {
        //Designation
        public FactionDesignationDef factionDesignation = null;
        public TRThingCategoryDef TRCategory = null;

        public TRGroupDef TRGroup;

        public Requisites requisites;

        //Properties
        public BeamHubProperties beamHub;

        public SuperWeaponProperties superWeapon;
        public TRGasProperties gasProps;

        //Creation Events
        public TerrainDef makesTerrain;
        public TRThingDef leavesThing;

        //
        public List<ConditionalStatModifier> conditionalStatOffsets;

        public bool isNatural = false;
        public bool hidden = false;
        public bool devObject = false;
        public bool clearTiberium = false;

        public DiscoveryProperties discovery;

        [Unsaved(false)]
        private TaggedString cachedUnknownLabelCap = null;

        public override IEnumerable<string> ConfigErrors()
        {
            List<string> strings = new List<string>();
            strings.AddRange(base.ConfigErrors());

            if (TRGroup == null)
            {
                //strings.Add("Missing TRGroupDef, adding basic...");
                TRGroup = TRGroupDefOf.All;
            }

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

        public bool IsActive(out string reason)
        {
            reason = "";
            string research = "";
            bool flag = true;
            var sb = new StringBuilder();
            sb.AppendLine("TR_LockedReason".Translate());
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
                sb.AppendLine("TR_LockedDueMinTech".Translate(minTechLevelToBuild.ToString()));
            }
            if ( maxTechLevelToBuild != TechLevel.Undefined && Faction.OfPlayer.def.techLevel > maxTechLevelToBuild)
            {
                flag = false;
                sb.AppendLine("TR_LockedDueMaxTech".Translate(maxTechLevelToBuild.ToString()));
            }
            if (!IsResearchFinished)
            {
                flag = false;
                foreach (var res in researchPrerequisites)
                {
                    research += "   - " + res.LabelCap;
                }
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
}
