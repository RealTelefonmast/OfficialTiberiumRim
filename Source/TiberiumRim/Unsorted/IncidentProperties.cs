using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TiberiumRim;

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
    public List<BiomeDef> allowedBiomes;
    public IncidentCategoryDef category;

    //Optional
    public GameConditionDef gameCondition;
    public IncidentDef incidentDef;
    public LetterDef letterDef;
    public string letterDesc;

    //LetterSettings
    public string letterLabel;
    public int minDifficulty;
    public float pointMultiplier = 1f;
    public int points = -1;

    public QuestScriptDef questScriptDef;

    //Incident Values
    public TaleDef tale;

    public Type workerClass;

    [Unsaved] private IncidentWorker workerInt;

    public IncidentWorker Worker
    {
        get
        {
            if (workerInt == null && workerClass != null)
                workerInt = (IncidentWorker)Activator.CreateInstance(workerClass);
            return workerInt;
        }
    }

    private Faction Faction => null; //Find.FactionManager.AllFactions.First(f => f.def == raidSettings.faction);

    public void Execute(Map map, TargetInfo target)
    {
    }

    private void TryExecute()
    {
    }

    private IncidentParms BasicIncidentParms(IIncidentTarget target)
    {
        var parms = StorytellerUtility.DefaultParmsNow(category, target);
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
        var parms = BasicIncidentParms(target);
        parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        parms.raidNeverFleeIndividual = false;
        parms.raidForceOneIncap = false;

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
    public IncidentCategoryDef category;
    public LetterDef letterDef;
    public string letterDesc;
    public string letterLabel;
    public float pointMultiplier = 1f;
    public int points = -1;
    public RaidSettings raidSettings = new();
    public List<ResearchProjectDef> researchUnlocks = new();
    public TaleDef tale;
    public IncidentType type = IncidentType.None;
    public Type workerClass;

    private IncidentWorker workerInt;
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
        get { return Find.FactionManager.AllFactions.First(f => f.def == raidSettings.faction); }
    }
}