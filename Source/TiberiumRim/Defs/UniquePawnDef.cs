using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TR.Defs;

public class UniquePawnDef : Def
{
    public BackstoryDef adulthood;
    public BeardDef beardDef;
    public int biologicalAge;
    public int birthDate;

    //Basic Body
    public BodyTypeDef bodyType;

    public BackstoryDef childhood;
    public List<UniqueApparel> clothes;
    public FactionDef faction;
    public Gender gender;
    public Color hairColor;
    public HairDef hairDef;
    public HeadTypeDef headType;

    public List<HediffDef> hediffs;
    public List<ThingDef> inventory;
    public PawnKindDef kindDef;
    public float melanin;

    public List<TraitDef> traits;

    //Identity
    public NameTriple uniqueName;

    //Equipment
    public List<ThingDef> weapons;
}

public class UniqueApparel
{
    public ThingDef stuff;
    public ThingDef thing;
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