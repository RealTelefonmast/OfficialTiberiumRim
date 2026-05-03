using System.Collections.Generic;
using RimWorld;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class TAERulesetDef : Def
{
    public List<DefValueLoadable<AtmosphericValueDef, float>> atmospheres;
    public List<BiomeDef> biomes;

    //IncidentRules
    public List<AtmosphericIncidentFilter> incidentFilters;
    public AtmosphericRealm realm = AtmosphericRealm.AnyBiome;
}