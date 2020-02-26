using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public enum IncidentCondition
    {
        Failed,
        Finished,
        Started
    }

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

    public class IncidentProperties
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

        public IncidentProperties()
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "category", "ThreatSmall");
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "letterDef", "NeutralEvent");
        }

        public IncidentWorker Worker
        {
            get
            {
                if (workerInt == null)
                    if (workerClass != null)
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

    public class EventDef : Def
    {
        [Unsaved]
        private EventWorker workerInt;
        [Unsaved]
        public List<TResearchDef> unlocksResearch = new List<TResearchDef>();

        public Type worker = typeof(EventWorker);

        public EventWorker Worker
        {
            get
            {
                if (workerInt == null && worker != null)
                    workerInt = (EventWorker) Activator.CreateInstance(worker, this);
                return workerInt;
            }
        }

        public bool HasBeenTriggered => TRUtils.EventManager().HasBeenTriggered(this);



        public override void ResolveReferences()
        {
            foreach (var research in DefDatabase<TResearchDef>.AllDefs)
            {
                if (research.requisites?.events?.Contains(this) ?? false)
                {
                    unlocksResearch.Add(research);
                }
            }
        }
    }
}
