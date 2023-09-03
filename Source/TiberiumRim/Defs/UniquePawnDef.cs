using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TR
{
    public class UniquePawnDef : Def
    {
        public PawnKindDef kindDef;
        public FactionDef faction;

        //Basic Body
        public BodyTypeDef bodyType;
        public HeadTypeDef headType;
        public HairDef hairDef;
        public BeardDef beardDef;
        public Gender gender;
        public float melanin;
        public Color hairColor;

        public List<TraitDef> traits;

        //Identity
        public NameTriple uniqueName;
        public int birthDate;
        public int biologicalAge;

        public BackstoryDef childhood;
        public BackstoryDef adulthood;

        //Equipment
        public List<ThingDef> weapons;
        public List<ThingDef> inventory;
        public List<UniqueApparel> clothes;

        public List<HediffDef> hediffs;
    }

    public class UniqueApparel
    {
        public ThingDef thing;
        public ThingDef stuff;
    }

    /*
    public class UniqueBackstoryDef : Def
    {
        private BackstoryDef def;
        public string identifier;
        public BackstorySlot slot;
        public string title;
        public string titleFemale;
        public string titleShort;
        public string titleShortFemale;
        public string baseDesc;
        public WorkTags workDisables;
        public WorkTags requiredWorkTags;
        public List<string> spawnCategories = new List<string>();
        public List<TraitEntry> forcedTraits;
        public List<TraitEntry> disallowedTraits;
        public List<string> hairTags;
        private string nameMaker;
        private RulePackDef nameMakerResolved;

        public Dictionary<string, int> skillGains;

        public bool shuffleable = true;

        public Backstory BackstoryFromThis()
        {
            Backstory backstory = new Backstory()
            {
                identifier = identifier,
                slot = slot,
                
            };
            backstory.PostLoad();
            backstory.ResolveReferences();
            return backstory;
        }
    }
    */
}
