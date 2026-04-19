using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TR.RARelics;

public class UniquePawnDef : Def
{
    public UniqueBackstoryDef adulthood;
    public int biologicalAge;
    public int birthDate;

    //Basic Body
    public BodyTypeDef bodyType;

    public UniqueBackstoryDef childhood;
    public List<UniqueApparel> clothes;
    public CrownType crownType;
    public FactionDef faction;
    public Gender gender;
    public Color hairColor;
    public HairDef hairDef;

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

public class UniqueBackstoryDef : Def
{
    public string baseDesc;
    public List<TraitEntry> disallowedTraits;
    public List<TraitEntry> forcedTraits;
    public List<string> hairTags;
    public string identifier;
    private string nameMaker;
    private RulePackDef nameMakerResolved;
    public WorkTags requiredWorkTags;

    public bool shuffleable = true;

    public Dictionary<string, int> skillGains;
    public BackstorySlot slot;
    public List<string> spawnCategories = new();
    public string title;
    public string titleFemale;
    public string titleShort;
    public string titleShortFemale;
    public WorkTags workDisables;

    public Backstory BackstoryFromThis()
    {
        Backstory backstory = new Backstory
        {
            identifier = identifier,
            slot = slot
        };
        backstory.PostLoad();
        backstory.ResolveReferences();
        return backstory;
    }
}