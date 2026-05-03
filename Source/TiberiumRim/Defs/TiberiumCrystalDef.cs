using System.Collections.Generic;
using Verse;

namespace TiberiumRim;

public class TiberiumCrystalDef : TRThingDef
{
    public ThingDef chunk;

    //Terrain
    public TerrainDef dead;

    //Corruptions
    public ThingDef monolith;
    public TiberiumTerrainDef plantTerrain;
    public ThingDef rock;
    public List<TerrainSupport> supportsTerrain = new();
    public TiberiumCrystalProperties tiberium;
    public ThingDef wall;

    public TiberiumValueType TiberiumValueType => tiberium.type;

    public HarvestType HarvestType
    {
        get
        {
            if (TiberiumValueType == TiberiumValueType.Unharvestable) return HarvestType.Unharvestable;
            if (TiberiumValueType == TiberiumValueType.Sludge) return HarvestType.Unvaluable;
            return HarvestType.Valuable;
        }
    }

    public bool IsInfective => tiberium.infects;
    public bool IsMoss => HarvestType == HarvestType.Unvaluable;

    public TerrainSupport TerrainSupportFor(TerrainDef def)
    {
        return supportsTerrain.Find(s => s.TerrainTag.SupportsDef(def));
    }
}