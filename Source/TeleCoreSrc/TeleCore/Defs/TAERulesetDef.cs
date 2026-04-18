using System.Collections.Generic;
using RimWorld;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class TAERulesetDef : Def
{
    public AtmosphericRealm realm = AtmosphericRealm.AnyBiome;
    public List<BiomeDef> biomes;
    public List<DefValueLoadable<AtmosphericValueDef, float>> atmospheres;
    
    //IncidentRules
    public List<AtmosphericIncidentFilter> incidentFilters;
}