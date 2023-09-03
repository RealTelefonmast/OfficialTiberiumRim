using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TR
{
    public enum IncidentType
    {
        CustomWorker,
        Reward,
        Research,
        Appear,
        Skyfaller,
        Raid,
        None
    }

    //TODO:Revive Incidenproperties and all components- Refactor fun!
    //TODO: Idea: HediffProperties?
    /*
     from incidentdef:
    public HediffDef diseaseIncident;
    public FloatRange diseaseVictimFractionRange = new FloatRange(0f, 0.49f);
    public int diseaseMaxVictims = 99999;
    public List<BodyPartDef> diseasePartsToAffect;
    */

    public class IncidentProperties
    {
        [Unsaved] 
        private IncidentWorker workerInt;

        public Type workerClass;
        public IncidentDef incidentDef;
        //Incident Values
        public TaleDef tale;
        public IncidentCategoryDef category;
        public int minDifficulty;
        public int points = -1;
        public float pointMultiplier = 1f;

        //Optional
        public GameConditionDef gameCondition;
        public List<BiomeDef> allowedBiomes;
        public QuestScriptDef questScriptDef;

        //LetterSettings
        public string letterLabel;
        public string letterDesc;
        public LetterDef letterDef;

        public IncidentProperties()
        {

        }

        public IncidentWorker Worker
        {
            get
            {
                if (workerInt == null && workerClass != null)
                    workerInt = (IncidentWorker)Activator.CreateInstance(workerClass);
                return workerInt;
            }
        }

        private Faction Faction
        {
            get
            {
                return null; //Find.FactionManager.AllFactions.First(f => f.props == raidSettings.faction);
            }
        }

        public void Execute(Map map, TargetInfo target)
        {

        }

        private void TryExecute()
        {

        }

        private IncidentParms BasicIncidentParms(IIncidentTarget target)
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(category, target);
            parms.points = points >= 0 ? points : parms.points;
            parms.points *= pointMultiplier;
            parms.forced = true;

            parms.customLetterDef = letterDef;
            parms.customLetterLabel = letterLabel;
            parms.customLetterText = letterDesc;
            parms.faction = Faction;
            return parms;
        }

        private IncidentParms RaidParms(IIncidentTarget target)
        {
            IncidentParms parms = BasicIncidentParms(target);
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidNeverFleeIndividual = false;
            parms.raidForceOneDowned = false;
            
            //Arrival
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            parms.raidArrivalModeForQuickMilitaryAid = false;
            parms.podOpenDelay = 140;
            /*
            parms.biocodeWeaponsChance;
            parms.dontUseSingleUseRocketLaunchers;
            parms.generateFightersOnly;
            parms.
                */
            return parms;
        }
    }

    public class IncidentProperties2
    {
        public IncidentType type = IncidentType.None;
        public Type workerClass;
        private IncidentWorker workerInt;
        public IncidentCategoryDef category;
        public string letterLabel;
        public string letterDesc;
        public LetterDef letterDef;
        public RaidSettings raidSettings = new RaidSettings();
        public TaleDef tale;
        public int points = -1;
        public float pointMultiplier = 1f;
        public List<ResearchProjectDef> researchUnlocks = new List<ResearchProjectDef>();
        //public PositionFilter positionFilter = new PositionFilter();
        //public SpawnSettings spawnSettings = new SpawnSettings();

        public IncidentProperties2()
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "category", "ThreatSmall");
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "letterDef", "NeutralEvent");
        }

        public IncidentWorker Worker
        {
            get
            {
                if (workerInt == null && workerClass != null)
                    workerInt = (IncidentWorker)Activator.CreateInstance(workerClass);
                return workerInt;
            }
        }

        private Faction Faction
        {
            get
            {
                return Find.FactionManager.AllFactions.First(f => f.def == raidSettings.faction);
            }
        }
    }
}
